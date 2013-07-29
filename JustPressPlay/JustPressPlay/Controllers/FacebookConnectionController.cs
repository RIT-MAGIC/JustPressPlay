using Facebook;
using JustPressPlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class FacebookConnectionController : Controller
    {
        //
        // GET: /FacebookConnection/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ConnectFacebook()
        {
            FacebookConnectionViewModel model = new FacebookConnectionViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConnectFacebook(FacebookConnectionViewModel model)
        {
            var fbClient = new FacebookClient();

            return RedirectToAction("Index");
        }
    }
}
