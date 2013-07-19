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
		/// TODO: Add parameters for filtering (and sorting?)
		/// </summary>
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
    }
}
