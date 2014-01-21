using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

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
            ViewBag.Success = false;
            return View();
        }

        [HttpPost]
        public ActionResult Contact(FormCollection collection)
        {
            List<String> testList = new List<String>();
            testList.Add("bws7462@rit.edu");

            JPPSendGrid.JPPSendGridProperties sendgridProperties = new JPPSendGrid.JPPSendGridProperties()
            {
                fromEmail = collection["email"],
                toEmail = testList,
                subjectEmail = collection["name"],
                htmlEmail = collection["message"]
            };

            JPPSendGrid.SendEmail(sendgridProperties);
            ViewBag.Success = true;
            return View();
        }
	}
}
