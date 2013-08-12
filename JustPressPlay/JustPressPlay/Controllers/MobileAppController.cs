using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JustPressPlay.Controllers
{
    public class MobileAppController : Controller
    {
        //
        // GET: /MobileApp/

       /* public ActionResult Index()
        {
            return View();
        }*/

        public JsonResult Login(String username, String password)
        {
            if (Membership.ValidateUser(username, password))
            {
                UnitOfWork work = new UnitOfWork();

                external_token token = work.SystemRepository.GenerateAuthorizationToken(username, Request.UserHostAddress);

                TokenModel tokenModel = new TokenModel()
                {
                    Token = token.token,
                    RefreshToken = token.token
                };

                return Json(tokenModel, JsonRequestBehavior.AllowGet);
            }

            return Json("Login Failed", JsonRequestBehavior.AllowGet);
        }

    }

    public class TokenModel
    {
        public String Token { get; set; }
        public String RefreshToken { get; set; }
    }
}
