/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using WebMatrix.WebData;
using System.Web.Security;

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
		public int ID { get; set; }

        [DataMember]
        public int PlayerID { get; set; }

		[DataMember]
		public String DisplayName { get; set; }

		[DataMember]
		public String PlayerImage { get; set; }

		[DataMember]
		public String Text { get; set; }

		[DataMember]
		public Boolean Deleted { get; set; }

        [DataMember]
        public DateTime CommentDate { get; set; }

        [DataMember]
        public Boolean CurrentUserCanEdit { get; set; }

        [DataMember]
        public Boolean CurrentUserCanDelete { get; set; }
	}

    [DataContract]
    public class EditCommentResponseModel
    {
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

		[DataMember]
		public String DisplayName { get; set; }

		[DataMember]
		public Boolean PrivacyViewable { get; set; }

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
			public int TemplateID { get; set; }

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
			public String ContentPhoto { get; set; }

			[DataMember]
			public String ContentText { get; set; }

			[DataMember]
			public String ContentURL { get; set; }

            [DataMember]
            public Boolean CurrentUserCanAddStory { get; set; }

            [DataMember]
            public Boolean CurrentUserCanEditStory { get; set; }

			[DataMember]
			public Boolean CommentsDisabled { get; set; }

			[DataMember]
			public IEnumerable<EarningComment> Comments { get; set; }

		}

        public static Earning SingleEarning(int id, bool isAchievement)
        {
            UnitOfWork work = new UnitOfWork();
            JustPressPlayDBEntities _dbContext = new JustPressPlayDBEntities();

            Earning earning = new Earning();
            var loggedInID = WebSecurity.CurrentUserId;
            var loggedInIsAdmin = Roles.IsUserInRole(JPPConstants.Roles.FullAdmin);
            
            if (isAchievement)
            {
                var achievement = _dbContext.achievement_instance.Find(id);
                var user = achievement.user;
                var template = achievement.achievement_template;

                earning.CommentsDisabled = WebSecurity.IsAuthenticated ? achievement.comments_disabled : true;
                earning.DisplayName = user.display_name;
                earning.EarnedDate = achievement.achieved_date;
                earning.EarningID = achievement.id;
                earning.EarningIsAchievement = true;
                earning.Image = template.icon;
                earning.PlayerID = user.id;
                earning.PlayerImage = user.image;
                earning.TemplateID = template.id;
                earning.Title = template.title;

                if (achievement.has_user_content)
                {
                    var content = _dbContext.achievement_user_content.Find(achievement.user_content_id);
                    earning.ContentPhoto = content.image;
                    earning.ContentText = content.text;
                    earning.ContentURL = content.url;
                }
                if (achievement.has_user_story)
                {
                    var story = _dbContext.achievement_user_story.Find(achievement.user_story_id);
                    earning.StoryPhoto = story.image;
                    earning.StoryText = story.text;
                }
                
            }
            else
            {
               var quest =  _dbContext.quest_instance.Find(id);
               var user = quest.user;
               var template = quest.quest_template;

                earning.CommentsDisabled = WebSecurity.IsAuthenticated ? quest.comments_disabled : true;
               earning.DisplayName = user.display_name;
               earning.EarnedDate = quest.completed_date;
               earning.EarningID = quest.id;
               earning.EarningIsAchievement = false;
               earning.Image = template.icon;
               earning.PlayerID = user.id;
               earning.PlayerImage = user.image;
               earning.TemplateID = template.id;
               earning.Title = template.title;
            }

            // Get comments
            if (!earning.CommentsDisabled)
            {
                earning.Comments = from c in work.EntityContext.comment
                                   where c.location_id == earning.EarningID && 
                                   ((earning.EarningIsAchievement && c.location_type == (int)JPPConstants.CommentLocation.Achievement) ||
                                    (!earning.EarningIsAchievement && c.location_type == (int)JPPConstants.CommentLocation.Quest))
                                   select new EarningComment()
                                   {
                                       ID = c.id,
                                       PlayerID = c.deleted ? c.last_modified_by_id : c.user_id,
                                       // Replace comment text if deleted and not admin
                                       Text = c.deleted ? (JPPConstants.SiteSettings.DeletedCommentText + c.last_modified_by.display_name) : c.text,
                                       PlayerImage = c.deleted ? null : c.user.image,
                                       DisplayName = c.deleted ? null : c.user.display_name,
                                       Deleted = c.deleted,
                                       CommentDate = c.date,
                                       CurrentUserCanEdit = (loggedInID == c.user_id || loggedInIsAdmin) && !c.deleted,
                                       CurrentUserCanDelete = (loggedInID == c.user_id || loggedInID == earning.PlayerID || loggedInIsAdmin) && !c.deleted
                                   };
            }

            earning.CurrentUserCanAddStory = earning.EarningIsAchievement && loggedInID == earning.PlayerID;
            earning.CurrentUserCanEditStory = earning.EarningIsAchievement && ( loggedInID == earning.PlayerID || loggedInIsAdmin );


            return earning;
            
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

            // Hit WebSecurity once for the user id
            int currentUserID = WebSecurity.CurrentUserId;


            // Basic queries
            var aq = from a in work.EntityContext.achievement_instance
                     select a;
            var qq = from q in work.EntityContext.quest_instance
                     select q;
            if (achievementID == null)
            {
                // Basic queries
                aq = from a in work.EntityContext.achievement_instance
                         where a.globally_assigned == false
                         select a;
            }
			// If questID is present, also get associated achievements
			if (questID != null)
			{
				aq = from a in aq
					 join step in work.EntityContext.quest_achievement_step
					 on a.achievement_template.id equals step.achievement_id
					 where step.quest_id == questID.Value
					 select a;
			}

			// Check for user
            if (friendsOf && WebSecurity.IsAuthenticated)
            {
                if (id == null)
                    id = WebSecurity.CurrentUserId;

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
            else if (id != null)
				{
					aq = from a in aq
						 where a.user_id == id.Value
						 select a;
					qq = from q in qq
						 where q.user_id == id.Value
						 select q;
				}
			

			// Strip out public?
			if (WebSecurity.IsAuthenticated)
			{
				// ACHIEVEMENTS - "Friends only" for my friends
				var aqFriendsOnly = from a in aq
									join f in work.EntityContext.friend
									on a.user_id equals f.source_id
                                    where !(a.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly && f.destination_id != currentUserID)
									select a;
				// Me or non-friends only
				aq = from a in aq
                     where a.user_id == currentUserID || a.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
					 select a;
				// Combine
				aq = aq.Union(aqFriendsOnly);

				// QUESTS - "Friends only" for my friends
				var qqFriendsOnly = from q in qq
									join f in work.EntityContext.friend
									 on q.user_id equals f.source_id
                                    where !(q.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly && f.destination_id != currentUserID)
									select q;
				// Me or non-friends only
				qq = from q in qq
                     where q.user_id == currentUserID || q.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
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

			IQueryable<Earning> finalQueryable;
			IQueryable<Earning> quests = from q in qq
										 select new Earning()
										 {
											 CommentsDisabled = q.comments_disabled,
											 DisplayName = q.user.display_name,
											 EarnedDate = q.completed_date,
											 EarningID = q.id,
											 EarningIsAchievement = false,
											 Image = q.quest_template.icon,
											 PlayerID = q.user_id,
											 PlayerImage = q.user.image,
											 TemplateID = q.quest_template.id,
											 Title = q.quest_template.title,
											 StoryPhoto = null,
											 StoryText = null,
                                             ContentPhoto = null,
                                             ContentText = null,
                                             ContentURL = null,
										 };
			IQueryable<Earning> achievements = from a in aq
											   select new Earning()
											   {
												   CommentsDisabled = a.comments_disabled,
												   DisplayName = a.user.display_name,
												   EarnedDate = a.achieved_date,
												   EarningID = a.id,
												   EarningIsAchievement = true,
												   Image = a.achievement_template.icon,
												   PlayerID = a.user_id,
												   PlayerImage = a.user.image,
												   TemplateID = a.achievement_template.id,
												   Title = a.achievement_template.title,
												   StoryPhoto = a.user_story.image,
												   StoryText = a.user_story.text,
												   ContentPhoto = a.user_content.image,
												   ContentText = a.user_content.text,
												   ContentURL = a.user_content.url
											   };

			// Handle types
			if (questID != null)
			{
				finalQueryable = from q in quests
								 where q.TemplateID == questID.Value
								 select q;

				// Combine since we need associated achievements
				//finalQueryable = finalQueryable.Concat(achievements);
			}
			else if (achievementID != null)
			{
				finalQueryable = from a in achievements
								 where a.TemplateID == achievementID.Value
								 select a;
			}
			else
			{
				finalQueryable = achievements.Concat(quests);
			}

			// Full admin?
			bool admin = Roles.IsUserInRole(JPPConstants.Roles.FullAdmin);

			// Get comments
			var final = from e in finalQueryable.AsEnumerable()
						select new Earning()
						{
							Comments = WebSecurity.IsAuthenticated ?
								// If logged in, get comments
								from c in work.EntityContext.comment
								where c.location_id == e.EarningID &&
								((e.EarningIsAchievement == true && c.location_type == (int)JPPConstants.CommentLocation.Achievement) ||
								(e.EarningIsAchievement == false && c.location_type == (int)JPPConstants.CommentLocation.Quest))
								select new EarningComment()
								{
									ID = c.id,
                                    PlayerID = c.deleted ? c.last_modified_by_id : c.user_id,
                                    // Replace comment text if deleted and not admin
                                    Text = c.deleted ? ( JPPConstants.SiteSettings.DeletedCommentText + c.last_modified_by.display_name ) : c.text,
									PlayerImage = c.deleted ? null : c.user.image,
									DisplayName = c.deleted ? null : c.user.display_name,
									Deleted = c.deleted,
                                    CommentDate = c.date,
                                    CurrentUserCanEdit = (currentUserID == c.user_id || admin) && !c.deleted,
                                    CurrentUserCanDelete = (currentUserID == c.user_id || currentUserID == e.PlayerID || admin) && !c.deleted
								} :
								// If not logged in, no comments!
								from c in work.EntityContext.comment
								where c.id == -1
								select new EarningComment()
								{
									ID = -1,
									Text = null,
									PlayerImage = null,
									DisplayName = null,
									Deleted = false,
                                    CommentDate = c.date,
                                    CurrentUserCanEdit = false,
                                    CurrentUserCanDelete = false
								},
							CommentsDisabled = WebSecurity.IsAuthenticated ? e.CommentsDisabled : true,
                            CurrentUserCanAddStory = e.EarningIsAchievement && currentUserID == e.PlayerID,
                            CurrentUserCanEditStory = e.EarningIsAchievement && ( currentUserID == e.PlayerID || admin ),
							DisplayName = e.DisplayName,
							EarnedDate = e.EarnedDate,
							EarningID = e.EarningID,
							EarningIsAchievement = e.EarningIsAchievement,
							Image = e.Image,
							PlayerID = e.PlayerID,
							PlayerImage = e.PlayerImage,
							TemplateID = e.TemplateID,
							Title = e.Title,
							StoryPhoto = e.StoryPhoto,
							StoryText = e.StoryText,
							ContentPhoto = e.ContentPhoto,
							ContentText = e.ContentText,
							ContentURL = e.ContentURL
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

			// Get the user
			user user = null;
			bool viewable = false;
			if (id != null && WebSecurity.IsAuthenticated)
			{
				// Grab the user
				user = work.EntityContext.user.Find(id.Value);

				// If we have a user, check if we can "see" them
				if (user != null)
				{
					viewable =
                        user.id == currentUserID ||
						user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly ||
						(from f in work.EntityContext.friend
                         where f.source_id == currentUserID && f.destination_id == user.id
						 select f).Any() || admin;
				}
			}

			// All done
			return new EarningsViewModel()
			{
				Earnings = final,
			    DisplayName = viewable ? user.display_name : null,
				PrivacyViewable = viewable
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

            [DataMember]
            public DateTime CommentDate { get; set; }
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
            bool isAchievement = true,
			int? instanceID = null,
			int? achievementID = null,
            int? questID = null,
			int? start = null,
			int? count = null,
			bool? includeDeleted = null,
			UnitOfWork work = null)
		{
            if (!WebSecurity.IsAuthenticated)
                return null;

			if (work == null)
				work = new UnitOfWork();

			// Begin the query with everything
			var q = from c in work.EntityContext.comment
					select c;
            if(instanceID == null)
            {
                if (id != null)
			    {
                    if (isAchievement && achievementID != null)
                        instanceID = work.EntityContext.achievement_instance.Where(ai => ai.achievement_id == achievementID.Value && ai.user_id == id.Value).SingleOrDefault().id;
                    else if (!isAchievement && questID != null)
                        instanceID = work.EntityContext.quest_instance.Where(qi => qi.quest_id == questID.Value && qi.user_id == id.Value).SingleOrDefault().id;                    
			    }
            }
			// What kind of look-up?
			if (instanceID != null)
			{
                if (isAchievement)
                {                    
                    // Use the instance id itself
                    q = from c in q
                        where c.location_id == instanceID.Value && c.location_type == (int)JPPConstants.CommentLocation.Achievement
                        select c;
                }
                else
                {
                    q = from c in q 
                        where c.location_id == instanceID.Value && c.location_type == (int)JPPConstants.CommentLocation.Quest
                        select c;
                }
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
							Text = c.text,
                            CommentDate = c.date
						};
            final = final.OrderByDescending(c => c.CommentDate);
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