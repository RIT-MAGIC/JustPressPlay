using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class QuestsController : Controller
    {
        /// <summary>
        /// The list of all quests
        /// </summary>
        /// <returns>GET: /Quests</returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// An individual quest page
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>GET: /Quests/{id}</returns>
		public ActionResult IndividualQuest(int id)
		{
			return View();
		}

		/// <summary>
		/// A specific quest instance for the specified player
		/// </summary>
		/// <param name="questID">The id of the quest</param>
		/// <param name="playerID">The id of the player</param>
		/// <returns>GET: /Quests/{questID}/{playerID}</returns>
		public ActionResult QuestPlayer(int questID, int playerID)
		{
			return View();
		}

    }
}
