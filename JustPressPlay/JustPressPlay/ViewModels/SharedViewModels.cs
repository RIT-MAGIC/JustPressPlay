﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using WebMatrix.WebData;

using JustPressPlay.Utilities;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Holds a single comment on an earning
	/// </summary>
	[DataContract]
	public class EarningComment
	{
		[DataMember]
		public String DisplayName { get; set; }

		[DataMember]
		public String PlayerImage { get; set; }

		[DataMember]
		public String Text { get; set; }
	}

	/// <summary>
	/// Holds a list of earnings
	/// </summary>
	[DataContract]
	public class EarningsViewModel
	{
		[DataMember]
		public IEnumerable<Earning> Earnings { get; set; }

		/// <summary>
		/// Contains information about one earning
		/// </summary>
		[DataContract]
		public class Earning
		{
			[DataMember]
			public int PlayerID { get; set; }

			[DataMember]
			public int EarningID { get; set; }

			[DataMember]
			public Boolean EarningIsAchievement { get; set; }

			[DataMember]
			public String DisplayName { get; set; }

			[DataMember]
			public String PlayerImage { get; set; }

			[DataMember]
			public DateTime EarnedDate { get; set; }

			[DataMember]
			public String Title { get; set; }

			[DataMember]
			public String Image { get; set; }

			[DataMember]
			public String StoryPhoto { get; set; }

			[DataMember]
			public String StoryText { get; set; }

			[DataMember]
			public IEnumerable<EarningComment> Comments { get; set; }

		}

		/// <summary>
		/// Returns a list of earnings for the specified user.
		/// TODO: Secure this so users can't spoof and get non-friend info
		/// </summary>
		/// <param name="id">The id of the player whose earnings should be returned</param>
		/// <param name="achievementID">Use this to return earnings relating to the specified achievement</param>
		/// <param name="questID">
		/// Use this to return earnings relating to the achievements that correspond 
		/// to the specified quest.  This overrides the "achievementID" parameter.\
		/// </param>
		/// <param name="friendsOf">Return earnings of players who are friends with the specified player instead?</param>
		/// <param name="includePublic">Include public users earnings?</param>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">How many earnings should be returned?</param>
		/// <param name="startComments">The zero-based index of the first comment to be returned</param>
		/// <param name="countComments">How many comments should be returned?</param>
		/// <param name="includeDeletedComments">Should deleted comments be returned?</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created</param>
		/// <returns>A list of earnings</returns>
		public static EarningsViewModel Populate(
			int? id = null,
			int? achievementID = null,
			int? questID = null,
			bool friendsOf = false,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Basic queries
			var aq = from a in work.EntityContext.achievement_instance
					 select a;
			var qq = from q in work.EntityContext.quest_instance
					 select q;

			// Check for user
			if (id != null)
			{
				if (friendsOf)
				{
					aq = from a in aq
						 join f in work.EntityContext.friend
						 on a.user_id equals f.source_id
						 where f.destination_id == id.Value
						 select a;
					qq = from q in qq
						 join f in work.EntityContext.friend
						 on q.user_id equals f.source_id
						 where f.destination_id == id.Value
						 select q;
				}
				else
				{
					aq = from a in aq
						 where a.user_id == id.Value
						 select a;
					qq = from q in qq
						 where q.user_id == id.Value
						 select q;
				}
			}

			// Strip out public?
			if (WebSecurity.IsAuthenticated)
			{
				// ACHIEVEMENTS - "Friends only" for my friends
				var aqFriendsOnly = from a in aq
									join f in work.EntityContext.friend
									on a.user_id equals f.source_id
									where !(a.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly && f.destination_id != WebSecurity.CurrentUserId)
									select a;
				// Me or non-friends only
				aq = from a in aq
					 where a.user_id == WebSecurity.CurrentUserId || a.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
					 select a;
				// Combine
				aq = aq.Union(aqFriendsOnly);

				// QUESTS - "Friends only" for my friends
				var qqFriendsOnly = from q in qq
									join f in work.EntityContext.friend
									 on q.user_id equals f.source_id
									where !(q.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly && f.destination_id != WebSecurity.CurrentUserId)
									select q;
				// Me or non-friends only
				qq = from q in qq
					 where q.user_id == WebSecurity.CurrentUserId || q.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
					 select q;
				// Combine
				qq = qq.Union(qqFriendsOnly);
			}
			else
			{
				// Public only!
				aq = from a in aq
					 where a.user.privacy_settings == (int)JPPConstants.PrivacySettings.Public
					 select a;
				qq = from q in qq
					 where q.user.privacy_settings == (int)JPPConstants.PrivacySettings.Public
					 select q;
			}

			IQueryable<Earning> final;
			IQueryable<Earning> quests = from q in qq
										 select new Earning()
										 {
											 // Need a junk query here to match initialization below, or query freaks out
											 Comments = from c in work.EntityContext.comment
														where c.id == -1
														select new EarningComment()
														{
															Text = "",
															PlayerImage = "",
															DisplayName = ""
														},
											 DisplayName = q.user.display_name,
											 EarnedDate = q.completed_date,
											 EarningID = q.quest_template.id,
											 EarningIsAchievement = false,
											 Image = q.quest_template.icon,
											 PlayerID = q.user_id,
											 PlayerImage = q.user.image,
											 Title = q.quest_template.title,
											 StoryPhoto = "",
											 StoryText = ""
										 };
			IQueryable<Earning> achievements = from a in aq
											   select new Earning()
											   {
												   // Need a junk query here to match initialization below, or query freaks out
												   Comments = from c in work.EntityContext.comment
															  where c.id == -1
															  select new EarningComment()
															  {
																  Text = "",
																  PlayerImage = "",
																  DisplayName = ""
															  },
												   DisplayName = a.user.display_name,
												   EarnedDate = a.achieved_date,
												   EarningID = a.achievement_template.id,
												   EarningIsAchievement = true,
												   Image = a.achievement_template.icon,
												   PlayerID = a.user_id,
												   PlayerImage = a.user.image,
												   Title = a.achievement_template.title,
												   StoryPhoto = a.user_story.image,
												   StoryText = a.user_story.text
											   };

			// Handle types
			if (questID != null)
			{
				final = from q in quests
						where q.EarningID == questID.Value
						select q;
			}
			else if (achievementID != null)
			{
				final = from a in achievements
						where a.EarningID == achievementID.Value
						select a;
			}
			else
			{
				final = achievements.Concat(quests);
			}

			// Get comments
			final = from e in final
					select new Earning()
					{
						Comments = from c in work.EntityContext.comment
								   where c.location_id == e.EarningID && 
								   ((e.EarningIsAchievement == true && c.location_type == (int)JPPConstants.CommentLocation.Achievement) ||
								   (e.EarningIsAchievement == false && c.location_type == (int)JPPConstants.CommentLocation.Quest))
								   select new EarningComment()
								   {
									   Text = c.text,
									   PlayerImage = c.user.image,
									   DisplayName = c.user.display_name
								   },
						DisplayName = e.DisplayName,
						EarnedDate = e.EarnedDate,
						EarningID = e.EarningID,
						EarningIsAchievement = e.EarningIsAchievement,
						Image = e.Image,
						PlayerID = e.PlayerID,
						PlayerImage = e.PlayerImage,
						Title = e.Title,
						StoryPhoto = e.StoryPhoto,
						StoryText = e.StoryText
					};

			final = final.OrderByDescending(e => e.EarnedDate);

			// Start at a specific index?
			if (start != null && start.Value > 0)
			{
				final = final.Skip(start.Value);
			}

			// Keep only a specific amount?
			if (count != null)
			{
				final = final.Take(count.Value);
			}

			// All done
			return new EarningsViewModel()
			{
				Earnings = final
			};
		}
	}

	/// <summary>
	/// Comments on a single earning
	/// </summary>
	[DataContract]
	public class EarningCommentsViewModel
	{
		[DataMember]
		public List<EarningComment> Comments { get; set; }

		/// <summary>
		/// A single comment on an earning
		/// </summary>
		[DataContract]
		public class EarningComment
		{
			[DataMember]
			public String DisplayName { get; set; }

			[DataMember]
			public String PlayerImage { get; set; }

			[DataMember]
			public String Text { get; set; }
		}

		/// <summary>
		/// Returns a populated list of earning comments
		/// TODO: Test once we have comments!
		/// </summary>
		/// <param name="id">The id of the user who's achievement the comments are on (requires 'achievementID' to be present as well)</param>
		/// <param name="achievementID">The id of the achievement template the comments are on (requires 'id' to be present as well)</param>
		/// <param name="achievementInstanceID">The id of the achievement instance the comments are on (overrides the use of 'achievementID' and 'id')</param>
		/// <param name="start">The zero-based index of the first comment to get</param>
		/// <param name="count">How many comments should be returned?</param>
		/// <param name="includeDeleted">Should deleted comments be included? TODO: Require specific roles for this!</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created</param>
		/// <returns>A populated list of comments</returns>
		public static EarningCommentsViewModel Populate(
			int? id = null,
			int? achievementID = null,
			int? achievementInstanceID = null,
			int? start = null,
			int? count = null,
			bool? includeDeleted = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Begin the query with everything
			var q = from c in work.EntityContext.comment
					select c;

			// What kind of look-up?
			if (achievementInstanceID != null)
			{
				// Use the instance id itself
				q = from c in q
					where c.location_id == achievementInstanceID.Value
					select c;
			}
			else if (id != null && achievementID != null)
			{
				// Look up based on id and achievement id
				q = from c in q
					from a in work.EntityContext.achievement_instance
					where a.user_id == id && a.achievement_id == achievementID
					where c.location_id == a.id
					select c;
			}
			else
			{
				// No way to look up the comments
				return null;
			}

			// Set up the final query
			var final = from c in q
						select new EarningComment()
						{
							DisplayName = c.user.display_name,
							PlayerImage = c.user.image,
							Text = c.text
						};

			// Start at a specific index?
			if (start != null && start.Value > 0)
			{
				final = final.Skip(start.Value);
			}

			// Keep only a specific amount?
			if (count != null)
			{
				final = final.Take(count.Value);
			}

			// All done
			return new EarningCommentsViewModel()
			{
				Comments = final.ToList()
			};
		}
	}
}