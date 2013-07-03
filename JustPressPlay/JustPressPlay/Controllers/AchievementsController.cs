using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            return View();
        }

		/// <summary>
		/// Handles an individual achievement's page
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <returns>GET: /Achievements/{id}</returns>
		public ActionResult IndividualAchievement(int id)
		{
			ViewBag.id = id;
			return View();
		}

		/// <summary>
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
		}
    }
}
