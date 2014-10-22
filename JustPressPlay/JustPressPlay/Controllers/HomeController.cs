/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Timeline");
            }
            else
                return RedirectToAction("About");
		}

        public ActionResult Timeline()
        {
            HomeViewModel model = HomeViewModel.Populate(includePublic: true);

            return View("Index", model);
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
                //Email.Send("", model.SenderName, model.SenderMessage);

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
