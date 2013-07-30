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
	[Authorize]
    public class JSONController : Controller
    {
		/// <summary>
		/// Returns information for the timeline.  The results change based on whether the
		/// user is currently logged in or not.
		/// </summary>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">The amount of earnings to return</param>
		/// <param name="startComments">The zero-based index of the first comment to return</param>
		/// <param name="countComments">The amount of comments to return per earning</param>
		/// <param name="includePublic">Include public earnings in the results?</param>
		/// <returns>GET: /JSON/Timeline</returns>
		public JsonResult Timeline(
			int? start = null, 
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includePublic = null)
		{
			return Json(TimelineViewModel.Populate(start, count, startComments, countComments, includePublic), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns a list of players
		/// </summary>
		/// <param name="start">The zero-based index of the first player to return</param>
		/// <param name="count">The total number of players to return</param>
		/// <param name="friendsWith">An id of the player whose friends should be returned</param>
		/// <param name="earnedAchievement">Only return players who earned the specified achievement (by id)</param>
		/// <param name="earnedQuest">Only return players who earned the specified quest (by id)</param>
		/// <param name="includeNonPlayers">Include users who are not "playing the game"?</param>
		/// <returns>GET: /JSON/Players</returns>
		public JsonResult Players(
			int? start = null, 
			int? count = null, 
			int? friendsWith = null,
			int? earnedAchievement = null,
			int? earnedQuest = null,
			bool? includeNonPlayers = null)
		{
			// Get the player list
			return Json(PlayersListViewModel.Populate(start, count, friendsWith, earnedAchievement, earnedQuest, includeNonPlayers), JsonRequestBehavior.AllowGet);
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
			bool? achievementsNotEarned = null,
			bool? inactiveAchievements = null,
			bool? createPoints = null,
			bool? explorePoints = null,
			bool? learnPoints = null,
			bool? socializePoints = null,
			String search = null)
		{
			return Json(
				AchievementsListViewModel.Populate(
					userID,
					questID,
					achievementsEarned,
					achievementsNotEarned,
					inactiveAchievements,
					createPoints,
					explorePoints,
					learnPoints,
					socializePoints,
					search),
				JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Returns a list of quests
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="completedQuests">Include completed quests?</param>
		/// <param name="partiallyCompletedQuests">Include partially completed quests?</param>
		/// <param name="incompleteQuests">Include fully incomplete (no progress) quests?</param>
		/// <param name="inactiveQuests">Include inactive quests?</param>
		/// <param name="userGeneratedQuests">Include user generated quests?</param>
		/// <param name="search">A string for searching</param>
		/// <returns>A populated view model with a list of quests</returns>
		public JsonResult Quests(
			int? userID = null,
			bool? completedQuests = null,
			bool? partiallyCompletedQuests = null,
			bool? incompleteQuests = null,
			bool? inactiveQuests = null,
			bool? userGeneratedQuests = null,
			String search = null)
		{
			return Json(
				QuestsListViewModel.Populate(
					userID,
					completedQuests,
					partiallyCompletedQuests,
					incompleteQuests,
					inactiveQuests,
					userGeneratedQuests,
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
			bool? friendsOf = null,
			bool? includePublic = null,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includeDeletedComments = null)
		{
			return Json(EarningsViewModel.Populate(id, achievementID, questID, friendsOf, includePublic, start, count, startComments, countComments, includeDeletedComments), JsonRequestBehavior.AllowGet);
		}
    }
}
