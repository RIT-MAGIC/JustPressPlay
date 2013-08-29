using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JustPressPlay.ViewModels;

namespace JustPressPlay.Controllers
{
	public class HomeController : Controller
	{
		/// <summary>
		/// The home page of the whole site
		/// Logged in: A user's timeline
		/// Logged out: The public timeline
		/// </summary>
		/// <returns>GET: /</returns>
		public ActionResult Index()
		{
			HomeViewModel model = HomeViewModel.Populate(includePublic:true);

			return View(model);
		}

        public ActionResult Credits()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
	}
}
