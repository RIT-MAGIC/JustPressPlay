using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class AdminController : Controller
    {
        /// <summary>
        /// Admin home page
        /// </summary>
        /// <returns>GET: /Admin</returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// Page for creating a white list user
		/// </summary>
		/// <returns>GET: /Admin/CreateUser</returns>
		public ActionResult CreateUser()
		{
			return View();
		}
    }
}
