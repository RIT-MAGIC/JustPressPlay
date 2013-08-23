using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

using JustPressPlay.Utilities;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.Controllers
{
	[Authorize]
    public class CommentsController : Controller
    {
		/// <summary>
		/// Testing comments - to be removed
		/// </summary>
		/// <returns>GET: /Comments</returns>
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Handles adding a comment for the currently logged in user
		/// </summary>
		/// <param name="earningID">The id of the earning</param>
		/// <param name="earningIsAchievement">True if the earning is an achievement, false for quests</param>
		/// <param name="text">The text of the comment</param>
		/// <returns>POST: /Comments/Add</returns>
		[HttpPost]
		public Boolean Add(int earningID, bool earningIsAchievement, String text)
		{
			// Need text for a comment
			if (String.IsNullOrWhiteSpace(text))
				return false;

			UnitOfWork work = new UnitOfWork();

			// Are comments enabled, and can we access the earning?
			user earningUser = null;
			object template = null;
			if (!CommentsEnabled(earningID, earningIsAchievement, work) || 
				!UserCanAccessEarning(earningID, earningIsAchievement, work, out earningUser, out template))
				return false;

			// Access is validated, create comment
			work.EntityContext.comment.Add(new comment()
			{
				date = DateTime.Now,
				deleted = false,
				last_modified_by_id = WebSecurity.CurrentUserId,
				last_modified_date = null, // Not being modified, just created, so this is null
				location_id = earningID,
				location_type = earningIsAchievement ? (int)JPPConstants.CommentLocation.Achievement : (int)JPPConstants.CommentLocation.Quest,
				text = text,
				user_id = WebSecurity.CurrentUserId
			});

			// Get the current user's display name
			user u = work.EntityContext.user.Find(WebSecurity.CurrentUserId);

			// Send a notification
			if (earningIsAchievement)
			{
				achievement_template a = template as achievement_template;
				work.SystemRepository.AddNotification(
					earningUser.id,
					WebSecurity.CurrentUserId,
					"[" + u.display_name + "] commented on [" + a.title + "]",
					u.image,
					new UrlHelper(Request.RequestContext).Action(
						"IndividualAchievement",
						"Achievements",
						new { id = a.id }
					) + "#" + earningUser.id + "-" + earningID,
					false);
			}
			else
			{
				quest_template q = template as quest_template;
				work.SystemRepository.AddNotification(
					earningUser.id,
					WebSecurity.CurrentUserId,
					"[" + u.display_name + "] commented on [" + q.title + "]",
					u.image,
					new UrlHelper(Request.RequestContext).Action(
						"IndividualQuest",
						"Quests",
						new { id = q.id }
					) + "#" + earningUser.id + "-" + earningID,
					false);
			}
			// Success
			work.SaveChanges();
			return true;
		}

		/// <summary>
		/// Edits a comment if the user is the owner or an admin
		/// </summary>
		/// <param name="commentID">The id of the comment</param>
		/// <param name="text">The new text</param>
		/// <returns>POST: /Comments/Edit</returns>
		[HttpPost]
		public Boolean Edit(int commentID, String text)
		{
			// Need text for a comment
			if (String.IsNullOrWhiteSpace(text))
				return false;

			UnitOfWork work = new UnitOfWork();

			// Grab the comment and check for edit capabilities
 			comment c = work.EntityContext.comment.Find(commentID);
			if (c.deleted)
				return false;
			if (c.user_id != WebSecurity.CurrentUserId && !Roles.IsUserInRole(JPPConstants.Roles.FullAdmin))
				return false;

			// Edit the comment
            LoggerModel logCommentEdit = new LoggerModel()
            {
                Action = Logger.CommentBehaviorLogType.CommentEdit.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = c.id,
                IDType1 = Logger.LogIDType.Comment.ToString(),
                Value1 = c.text,
                Value2 = text
            };

            Logger.LogSingleEntry(logCommentEdit, work.EntityContext);

			c.text = text;
			c.last_modified_by_id = WebSecurity.CurrentUserId;
			c.last_modified_date = DateTime.Now;
			work.SaveChanges();
			return true;
		}

		// Delete - Me, or instance owner, or admin
		/// <summary>
		/// Deletes a comment, which can be done by the comment owner,
		/// the earning owner or a full admin
		/// </summary>
		/// <param name="commentID">The id of the comment</param>
		/// <returns>POST: /Comments/Delete</returns>
		[HttpPost]
		public Boolean Delete(int commentID)
		{
			UnitOfWork work = new UnitOfWork();

			// Grab the comment and check for edit capabilities
			comment c = work.EntityContext.comment.Find(commentID);

			// Is the current user the instance owner?
			bool instanceOwner = false;
			if (c.location_type == (int)JPPConstants.CommentLocation.Achievement)
			{
				instanceOwner = (from e in work.EntityContext.achievement_instance
								 where e.id == c.location_id && e.user_id == WebSecurity.CurrentUserId
								 select e).Any();
			}
			else if(c.location_type == (int)JPPConstants.CommentLocation.Quest)
			{
				instanceOwner = (from e in work.EntityContext.quest_instance
								 where e.id == c.location_id && e.user_id == WebSecurity.CurrentUserId
								 select e).Any();
			}

			// Instance owner, comment owner or admin?
			if (!instanceOwner && c.user_id != WebSecurity.CurrentUserId && !Roles.IsUserInRole(JPPConstants.Roles.FullAdmin))
				return false;

            LoggerModel logCommentDelete = new LoggerModel()
            {
                Action = Logger.CommentBehaviorLogType.CommentDelete.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = c.id,
                IDType1 = Logger.LogIDType.Comment.ToString(),
                Value1 = c.text
            };

            Logger.LogSingleEntry(logCommentDelete, work.EntityContext);

			// Mark as deleted
			c.deleted = true;
			c.last_modified_by_id = WebSecurity.CurrentUserId;
			c.last_modified_date = DateTime.Now;
			work.SaveChanges();
			return true;
		}

		/// <summary>
		/// Enables comments for an earning
		/// </summary>
		/// <param name="earningID">The id of the earning</param>
		/// <param name="earningIsAchievement">Is it an achievement earning?</param>
		/// <returns>POST: /Comments/Enable</returns>
		[HttpPost]
		public Boolean Enable(int earningID, bool earningIsAchievement)
		{
			return EnableDisable(earningID, earningIsAchievement, false);
		}

		/// <summary>
		/// Enables comments for an earning
		/// </summary>
		/// <param name="earningID">The id of the earning</param>
		/// <param name="earningIsAchievement">Is it an achievement earning?</param>
		/// <returns>POST: /Comments/Enable</returns>
		[HttpPost]
		public Boolean Disable(int earningID, bool earningIsAchievement)
		{
			return EnableDisable(earningID, earningIsAchievement, true);
		}
		
		/// <summary>
		/// Helper for enabling or disabling comments on earnings
		/// </summary>
		/// <param name="earningID">The id of the earning</param>
		/// <param name="earningIsAchievement">Is this earning an achievement?  If not, assume quest.</param>
		/// <param name="newState">The new state (true = DISABLED, false = ENABLED)</param>
		/// <returns>True if successful, false otherwise</returns>
		private Boolean EnableDisable(int earningID, bool earningIsAchievement, bool newState)
		{
			UnitOfWork work = new UnitOfWork();

			// Get the instance, check the user and alter
			if (earningIsAchievement)
			{
				// Only instance owners or admins
				achievement_instance instance = work.EntityContext.achievement_instance.Find(earningID);
				if (instance.user_id != WebSecurity.CurrentUserId && !Roles.IsUserInRole(JPPConstants.Roles.FullAdmin))
					return false;

				// Already enabled?
				if (instance.comments_disabled == newState)
					return false;

				instance.comments_disabled = newState;
			}
			else
			{
				// Only instance owners or admins
				quest_instance instance = work.EntityContext.quest_instance.Find(earningID);
				if (instance.user_id != WebSecurity.CurrentUserId && !Roles.IsUserInRole(JPPConstants.Roles.FullAdmin))
					return false;

				// Already enabled?
				if (instance.comments_disabled == newState)
					return false;

				instance.comments_disabled = newState;
			}

			work.SaveChanges();
			return true;
		}

		/// <summary>
		/// Determines if comments are enabled in both the site and the individual earning
		/// </summary>
		/// <param name="earningID">The earning ID</param>
		/// <param name="earningIsAchievement">Is this an achievement?</param>
		/// <param name="work">DB access</param>
		/// <returns>True if comments can be created on this earning, false otherwise</returns>
		private bool CommentsEnabled(int earningID, bool earningIsAchievement, UnitOfWork work)
		{
			// Site-wide check
			bool siteWideEnabled = Boolean.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CommentsEnabled));
			bool thisInstanceEnabled = !(earningIsAchievement ?
										(from e in work.EntityContext.achievement_instance
										where e.id == earningID
										select e.comments_disabled).FirstOrDefault() :
										(from e in work.EntityContext.quest_instance
										where e.id == earningID
										select e.comments_disabled).FirstOrDefault());
			
			return siteWideEnabled && thisInstanceEnabled;
		}

		/// <summary>
		/// Determines if the logged in user has access to the earning
		/// </summary>
		/// <param name="earningID">The ID of the earning</param>
		/// <param name="earningIsAchievement">Is the earning an achievement?  If not, it's a quest</param>
		/// <param name="work">The DB access</param>
		/// <returns>True if the user can access, false otherwise</returns>
		private bool UserCanAccessEarning(int earningID, bool earningIsAchievement, UnitOfWork work, out user earningUser, out object earningTemplate)
		{
			// Set up out param
			earningUser = null;
			earningTemplate = null;

			// Assume invalid, then check
			bool valid = false;
			if (earningIsAchievement)
			{
				// Base query for achievements
				var q = from e in work.EntityContext.achievement_instance
						where e.id == earningID
						select e;

				// Can the user "see" this earning?
				var mine = from e in q
						   where e.user_id == WebSecurity.CurrentUserId
						   select e;

				var publicUsers = from e in q
								  where e.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
								  select e;

				var friendOnlyFriends = from e in q
										join f in work.EntityContext.friend
										on e.user_id equals f.source_id
										where e.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly &&
											f.destination_id == WebSecurity.CurrentUserId
										select e;

				// Concat the queries and see if we have anything
				achievement_instance instance = mine.Concat(publicUsers).Concat(friendOnlyFriends).FirstOrDefault();
				if (instance != null)
				{
					valid = true;
					earningUser = instance.user;
					earningTemplate = instance.achievement_template;
				}	
			}
			else
			{
				// Base query for quests
				var q = from e in work.EntityContext.quest_instance
						where e.id == earningID
						select e;

				// Check different scenarios
				var mine = from e in q
						   where e.user_id == WebSecurity.CurrentUserId
						   select e;

				var publicUsers = from e in q
								  where e.user.privacy_settings != (int)JPPConstants.PrivacySettings.FriendsOnly
								  select e;

				var friendOnlyFriends = from e in q
										join f in work.EntityContext.friend
										on e.user_id equals f.source_id
										where e.user.privacy_settings == (int)JPPConstants.PrivacySettings.FriendsOnly &&
											f.destination_id == WebSecurity.CurrentUserId
										select e;

				// Concat the queries and see if we have anything
				quest_instance instance = mine.Concat(publicUsers).Concat(friendOnlyFriends).FirstOrDefault();
				if (instance != null)
				{
					valid = true;
					earningUser = instance.user;
					earningTemplate = instance.quest_template;
				}
			}

			return valid;
		}

    }
}
