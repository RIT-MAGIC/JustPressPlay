using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
	public class HomeController : Controller
	{
		/// <summary>
		/// The home page of the whole site
		/// Logged in: A user's timeline
		/// Logged out: General info
		/// </summary>
		/// <returns>GET: /</returns>
		public ActionResult Index()
		{
			ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

			return View();
		}
	}
}
