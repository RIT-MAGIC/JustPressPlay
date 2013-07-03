using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class PlayersController : Controller
    {
        /// <summary>
        /// The list of all plages
        /// </summary>
        /// <returns>GET: /Players</returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// An individual player's profile
		/// If this is your own profile, it will be editable
		/// </summary>
		/// <param name="id">The player's id</param>
		/// <returns>GET: /Players/{id}</returns>
		public ActionResult IndividualPlayer(int id)
		{
			return View();
		}
    }
}
