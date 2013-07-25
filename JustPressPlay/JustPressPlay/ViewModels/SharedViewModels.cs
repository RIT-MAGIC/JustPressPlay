using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Holds a list of earnings
	/// </summary>
	[DataContract]
	public class EarningsViewModel
	{
		[DataMember]
		public List<Earning> Earnings { get; set; }

		/// <summary>
		/// Contains information about one earning
		/// </summary>
		[DataContract]
		public class Earning
		{
			[DataMember]
			public String DisplayName { get; set; }
			
			[DataMember]
			public String PlayerImage { get; set; }

			[DataMember]
			public DateTime EarnedDate { get; set; }

			[DataMember]
			public AchievementViewModel Achievement { get; set; }

			[DataMember]
			public String StoryPhoto { get; set; }

			[DataMember]
			public String StoryText { get; set; }

			[DataMember]
			public EarningCommentsViewModel Comments { get; set; }
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
			bool? friendsOf = null,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includeDeletedComments = null,
			UnitOfWork work = null)
		{
			if (work == null) 
				work = new UnitOfWork();

			// Start the query with all achievement instances
			var q = from e in work.EntityContext.achievement_instance
					select e;

			// Check for quest or achievements
			if (questID != null)
			{
				// Restrict to achievements related to the specific quest
				q = from e in q
					join step in work.EntityContext.quest_achievement_step
					on e.achievement_id equals step.achievement_id
					where step.quest_id == questID.Value
					select e;
				// TODO: Test this, might not work!
			}
			else if (achievementID != null)
			{
				// Restrict to a specific achievement
				q = from e in q
					where e.achievement_id == achievementID
					select e;
			}

			// What kind of user restrictions?
			if (id != null && friendsOf != null && friendsOf.Value == true)
			{
				// Get earnings of the user's friends
				q = from e in q
					from f in work.EntityContext.friend
					where e.user_id != id &&
						  ((f.source_id == id && f.destination_id == e.user_id) ||
						  (f.source_id == e.user_id && f.destination_id == id))
					select e;
			}
			else if (id != null)
			{
				// Get the earnings of just the user
				q = from e in q
					where e.user_id == id
					select e;
			}

			// Create the basic query without method calls
			var basic = from e in q
						join u in work.EntityContext.user
						on e.user_id equals u.id
						select new
						{
							AchievementID = e.achievement_id,
							AchievementInstanceID = e.id,
							DisplayName = u.display_name,
							PlayerImage = u.image,
							EarnedDate = e.achieved_date,
							StoryPhoto = e.user_story.image,
							StoryText = e.user_story.text
						};

			// Convert the query to enumerable and call methods
			var final = from e in basic.AsEnumerable()
						select new Earning()
						{
							DisplayName = e.DisplayName,
							PlayerImage = e.PlayerImage,
							Achievement = AchievementViewModel.Populate(e.AchievementID, id, null, work),
							EarnedDate  = e.EarnedDate,
							StoryPhoto = e.StoryPhoto,
							StoryText = e.StoryText,
							Comments = EarningCommentsViewModel.Populate(
								null,	// Null because we have the instance id
								null,	// Null because we have the instance id
								e.AchievementInstanceID,	// Use the instance id instead of user and template id
								startComments,
								countComments,
								includeDeletedComments,
								work)
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

			// Make the model and run the query
			return new EarningsViewModel()
			{
				Earnings = final.ToList()
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