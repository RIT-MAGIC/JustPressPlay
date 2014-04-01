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
    //s[InitializeSiteAdminAndSettings]
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
            ViewBag.DevPassword = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.DevPasswordEnabled));
            ViewBag.Success = false;
            return View(model);
        }

        [HttpPost]
        public ActionResult Contact(ContactPageViewModel model)
        {
            if (bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.DevPasswordEnabled)))
            {
                if (model.DevPassword == null || !model.DevPassword.Equals(JPPConstants.devPassword))
                    ModelState.AddModelError("", "The Dev password is incorrect");
            }

            if (ModelState.IsValid)
            {
                List<String> testList = new List<String>();
                testList.Add(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminEmail));

                JPPSendGrid.JPPSendGridProperties sendgridProperties = new JPPSendGrid.JPPSendGridProperties()
                {
                    fromEmail = model.SenderEmail,
                    toEmail = testList,
                    subjectEmail = model.SenderName,
                    htmlEmail = model.SenderMessage
                };

                JPPSendGrid.SendEmail(sendgridProperties);
                ViewBag.DevPassword = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.DevPasswordEnabled));
                ViewBag.Success = true;
                return View();
            }
            ViewBag.DevPassword = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.DevPasswordEnabled));
            ViewBag.Success = false;
            return View(model);
        }
	}
}
