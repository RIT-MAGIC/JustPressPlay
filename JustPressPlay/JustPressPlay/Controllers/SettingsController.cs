using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class SettingsController : Controller
    {
        /// <summary>
        /// A player's settings
        /// </summary>
        /// <returns>GET: /Settings</returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}
