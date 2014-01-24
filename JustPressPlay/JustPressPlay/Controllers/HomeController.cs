using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

namespace JustPressPlay.Controllers
{
    //Commented out to make dev easier
    //[InitializeSiteAdminAndSettings]
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
            var model = ContactPageViewModel.Populate();
            ViewBag.Success = false;
            return View(model);
        }

        [HttpPost]
        public ActionResult Contact(ContactPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<String> testList = new List<String>();
                testList.Add("bws7462@rit.edu");

                JPPSendGrid.JPPSendGridProperties sendgridProperties = new JPPSendGrid.JPPSendGridProperties()
                {
                    fromEmail = model.SenderEmail,
                    toEmail = testList,
                    subjectEmail = model.SenderName,
                    htmlEmail = model.SenderMessage
                };

                JPPSendGrid.SendEmail(sendgridProperties);
                ViewBag.Success = true;
                return View();
            }
            ViewBag.Success = false;
            return View(model);
        }
	}
}
