using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class NewsController : Controller
    {
        /// <summary>
        /// The list of all news (I think)
        /// </summary>
        /// <returns>GET: /News </returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// A single new item's page
		/// </summary>
		/// <returns>GET: /News/{id}</returns>
		public ActionResult IndividualNews(int id)
		{
			return View();
		}

    }
}
