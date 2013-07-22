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
		/// <param name="achievementsEarned">Should earned achievements be returned? Requires the userID parameter. Default is true.</param>
		/// <param name="achievementsNotEarned">Should not-yet-earned achievements be returned? Requires the userID parameter. Default is true.</param>
		/// <param name="inactiveAchievements">Should inactive achievements be returned? Default is false.</param>
		/// <param name="createPoints">Require create points?</param>
		/// <param name="explorePoints">Require explore points?</param>
		/// <param name="learnPoints">Require learn points?</param>
		/// <param name="socializePoints">Require socialize points?</param>
		/// <param name="search">A string for searching</param>
		/// <param name="work">The unit of work for DB access. If null, one will be created.</param>
		/// <returns>A populated view model with a list of achievements</returns>
		public JsonResult Achievements(
			int? userID = null,
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
		/// Returns a list of earnings
		/// </summary>
		/// <param name="id">The id of the player whose earnings should be returned</param>
		/// <param name="friendsOf">Return earnings of players who are friends with the specified player instead?</param>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">How many earnings should be returned?</param>
		/// <param name="startComments">The zero-based index of the first comment to be returned</param>
		/// <param name="countComments">How many comments should be returned?</param>
		/// <param name="includeDeletedComments">Should deleted comments be returned?</param>
		/// <returns>GET: /JSON/Earnings</returns>
		public JsonResult Earnings(
			int? id = null,
			bool? friendsOf = null,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includeDeletedComments = null)
		{
			return Json(EarningsViewModel.Populate(id, friendsOf, start, count, startComments, countComments, includeDeletedComments), JsonRequestBehavior.AllowGet);
		}
    }
}
