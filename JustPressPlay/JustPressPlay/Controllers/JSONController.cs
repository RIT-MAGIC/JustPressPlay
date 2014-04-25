using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JustPressPlay.Utilities;
using JustPressPlay.ViewModels;

namespace JustPressPlay.Controllers
{
	/// <summary>
	/// Handles all JSON requests.
	/// </summary>
    public class JSONController : Controller
    {
		/// <summary>
		/// Returns a list of players
		/// </summary>
		/// <param name="start">The zero-based index of the first player to return</param>
		/// <param name="count">The total number of players to return</param>
		/// <param name="userID">The user ID for friend-related stuff</param>
		/// <param name="friendsWith">True for friends, false for non-friends, null for everywhere</param>
		/// <param name="earnedAchievement">Only return players who earned the specified achievement (by id)</param>
		/// <param name="earnedQuest">Only return players who earned the specified quest (by id)</param>
		/// <returns>GET: /JSON/Players</returns>
		public JsonResult Players(
			int? start = null, 
			int? count = null,
			int? userID = null,
			bool? friendsWith = null,
			int? earnedAchievement = null,
			int? earnedQuest = null)
		{
			// Get the player list
			return Json(PlayersListViewModel.Populate(start, count, userID, friendsWith, earnedAchievement, earnedQuest), JsonRequestBehavior.AllowGet);
		}

        

		/// <summary>
		/// Returns a list of achievements
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="achievementsEarned">Should earned achievements be returned? Requires the userID parameter. Default is true.</param>
		/// <param name="achievementsNotEarned">Should not-yet-earned achievements be returned? Requires the userID parameter. Default is true.</param>
		/// <param name="inactiveAchievements">Should inactive achievements be returned? Default is false.</param>
		/// <param name="createPoints">Require create points?</param>
		/// <param name="explorePoints">Require explore points?</param>
		/// <param name="learnPoints">Require learn points?</param>
		/// <param name="socializePoints">Require socialize points?</param>
		/// <param name="search">A string for searching</param>
		/// <returns>A populated view model with a list of achievements</returns>
		public JsonResult Achievements(
			int? userID = null,
			int? questID = null,
			bool? achievementsEarned = null,
			bool inactiveAchievements = false,
			bool? createPoints = null,
			bool? explorePoints = null,
			bool? learnPoints = null,
			bool? socializePoints = null,
			int? start = null,
			int? count = null,
			String search = null)
		{
			return Json(
				AchievementsListViewModel.Populate(
					userID,
					questID,
					achievementsEarned,
					inactiveAchievements,
					createPoints,
					explorePoints,
					learnPoints,
					socializePoints,
					start,
					count,
					search),
				JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns a list of quests
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="completedQuests">Only include completed quests?</param>
		/// <param name="partiallyCompletedQuests">Only include partially completed quests?</param>
		/// <param name="incompleteQuests">Only include fully incomplete quests?</param>
		/// <param name="inactiveQuests">Include inactive quests?</param>
		/// <param name="trackedQuests">Show only tracked quests?</param>
		/// <param name="userGeneratedQuests">Include user generated quests?</param>
		/// <param name="search">A string for searching</param>
		/// <returns>A populated view model with a list of quests</returns>
		public JsonResult Quests(
			int? userID = null,
			bool completedQuests = false,
			bool partiallyCompletedQuests = false,
			bool incompleteQuests = false,
			bool inactiveQuests = false,
			bool trackedQuests = false,
			bool userGeneratedQuests = false,
			int? start = null,
			int? count = null,
			String search = null)
		{
			return Json(
				QuestsListViewModel.Populate(
					userID,
					completedQuests,
					partiallyCompletedQuests,
					incompleteQuests,
					inactiveQuests,
					trackedQuests,
					userGeneratedQuests,
					start,
					count,
					search),
				JsonRequestBehavior.AllowGet);

		}

		/// <summary>
		/// Returns a list of earnings
		/// </summary>
		/// <param name="id">The id of the player whose earnings should be returned</param>
		/// <param name="achievementID">Use this to return earnings relating to the specified achievement</param>
		/// <param name="questID">
		/// Use this to return earnings relating to the achievements that correspond 
		/// to the specified quest.  This overrides the "achievementID" parameter.\
		/// </param>
		/// <param name="friendsOf">Return earnings of players who are friends with the specified player instead?</param>
		/// <param name="includePublic">Include public earnings?</param>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">How many earnings should be returned?</param>
		/// <param name="startComments">The zero-based index of the first comment to be returned</param>
		/// <param name="countComments">How many comments should be returned?</param>
		/// <param name="includeDeletedComments">Should deleted comments be returned?</param>
		/// <returns>GET: /JSON/Earnings</returns>
		public JsonResult Earnings(
			int? id = null,
			int? achievementID = null,
			int? questID = null,
			bool friendsOf = false,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null)
		{
			return Json(EarningsViewModel.Populate(id, achievementID, questID, friendsOf, start, count, startComments, countComments), JsonRequestBehavior.AllowGet);
		}

        public JsonResult Earning(int id, bool isAchievement)
        {
            return Json(EarningsViewModel.SingleEarning(id, isAchievement), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns a list of comments for the specified earning
        /// </summary>
        /// <param name="id">The id of the player whose earning should be looked up</param>
        /// <param name="isAchievement">Is the comment for an achievement or quest</param>
        /// <param name="earningID">The id of the earning "instance"</param>
        /// <param name="achievementID">The id of the achievement template (Only needed if earningID is null)</param>
        /// <param name="questID">The id of the quest template (Only needed if earningID is null)</param>
        /// <param name="startComments">The zero-based index of the first comment to be returned</param>
        /// <param name="countComments">How many comments should be returned</param>
        /// <returns>GET : /JSON/Comments</returns>
        public JsonResult Comments(
            int? id = null,
            bool isAchievement = true,
            int? earningID = null, 
            int? achievementID = null, 
            int? questID = null, 
            int? startComments = null, 
            int? countComments = null)
        {
            return Json(EarningCommentsViewModel.Populate(id, isAchievement, earningID, achievementID, questID, startComments, countComments), JsonRequestBehavior.AllowGet);
        }

    }
}
