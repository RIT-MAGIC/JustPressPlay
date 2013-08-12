using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using JustPressPlay.ViewModels;

namespace JustPressPlay.Controllers
{
    public class AchievementsController : Controller
    {
		/// <summary>
		/// Handles the home page of the Achievements section
		/// </summary>
		/// <returns>GET: /Achievements</returns>
        public ActionResult Index()
        {
			// Get the list of ALL achievements
			AchievementsListViewModel model = AchievementsListViewModel.Populate();
            return View(model);
        }

		/*/// <summary>
		/// Handles an individual achievement's page
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <returns>GET: /Achievements/{id}</returns>
		public ActionResult IndividualAchievement(int id)
		{
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("IndividualAchievement", new { id = id, playerID = WebSecurity.CurrentUserId });

			AchievementViewModel model =
				AchievementViewModel.Populate(
					id,
					WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserId : (int?)null);

			return View(model);
		}*/

        
        public ActionResult IndividualAchievement(int id, int? playerID)
        {
            if (playerID == null && WebSecurity.IsAuthenticated)
                return RedirectToAction("IndividualAchievement", new { id = id, playerID = WebSecurity.CurrentUserId });

            AchievementViewModel model = AchievementViewModel.Populate(id);

            return View(model);
        }

		/*/// <summary>
		/// Page for a player's specific instance of an achievement
		/// </summary>
		/// <param name="achievementID">The id of the achievement</param>
		/// <param name="playerID">The id of the player</param>
		/// <returns>GET: /Achievements/{achievementID}/{playerID}</returns>
		public ActionResult AchievementPlayer(int achievementID, int playerID)
		{
			ViewBag.achievementID = achievementID;
			ViewBag.playerID = playerID;
			return View();
		}*/

        public ActionResult AchievementImageSubmission()
        {
            return View();
        }

        public ActionResult AchievementTextSubmission()
        {
            return View();
        }

        public ActionResult AchievementURLSubmission()
        {
            return View();
        }
    }
}
