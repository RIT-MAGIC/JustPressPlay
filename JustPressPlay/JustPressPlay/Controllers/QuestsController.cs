using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using JustPressPlay.ViewModels;
using JustPressPlay.Models.Repositories;

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
			// Get the list of ALL achievements
			QuestsListViewModel model = QuestsListViewModel.Populate();
			return View(model);
        }

		/// <summary>
		/// An individual quest page
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>GET: /Quests/{id}</returns>
		public ActionResult IndividualQuest(int id)
		{
			QuestViewModel model = QuestViewModel.Populate(id);
			return View(model);
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

		/// <summary>
		/// Tracks the specific quest for the current user
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>POST: /Quests/Track</returns>
		[HttpPost]
		[Authorize]
		public Boolean Track(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.QuestRepository.Track(id);
		}

		/// <summary>
		/// Untracks the specific quest for the current user
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>POST: /Quests/Untrack</returns>
		[HttpPost]
		[Authorize]
		public Boolean Untrack(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.QuestRepository.Untrack(id);
		}

    }
}
