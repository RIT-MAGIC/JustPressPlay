using Facebook;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;
using JustPressPlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace JustPressPlay.Controllers
{
    public class SettingsController : Controller
    {
        // TODO: Move to site settings DB once implemented
        string appId = "295662587237899";
        string appSecret = "2456db6dc3c7c0e8e76913ab6d6e1028"; // TODO: RESET IF COMMITTED TO GIT!

        /// <summary>
        /// A player's settings
        /// </summary>
        /// <returns>GET: /Settings</returns>
        public ActionResult Index()
        {
            if (TempData["FacebookResultMessage"] != null)
                ViewBag.FacebookResultMessage = TempData["FacebookResultMessage"];

            UserSettingsViewModel model = UserSettingsViewModel.Populate(WebSecurity.CurrentUserId);
            return View(model);
        }

        /*
        [HttpGet]
        public ActionResult ConnectFacebook()
        {
            FacebookConnectionViewModel model = new FacebookConnectionViewModel();
            return View(model);
        }*/

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConnectFacebook(FacebookConnectionViewModel model)
        {
            // Save user settings to db
            using (UnitOfWork work = new UnitOfWork())
            {
                user currentUser = work.UserRepository.GetUser(WebSecurity.CurrentUserId);
                work.UserRepository.AddOrUpdateFacebookSettings(currentUser, model.NotificationsEnabled, model.AutomaticSharingEnabled);
                work.SaveChanges();
            }

            // Redirect to Facebook to ask for permissions
            string redirectAfterLoginUri = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("Default", new { Controller = "Settings", Action = "ProcessFacebookLogin" });
            string scope = string.Empty; // NOTE: No change in scope needed for notifications; apps don't need to ask permission
            if (model.AutomaticSharingEnabled)
            {
                scope += "publish_actions,";
            }
            string fbRedirectUrl = string.Format("https://www.facebook.com/dialog/oauth"
                                                 + "?client_id={0}"
                                                 + "&redirect_uri={1}"
                                                 + "&scope={2}",
                                                 appId, redirectAfterLoginUri, scope); // TODO: state, response_type: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/
            Response.Redirect(fbRedirectUrl);

            // Shouldn't ever get here; if we do, re-show the form
            return View(model);
        }

        [HttpGet]
        public ActionResult ProcessFacebookLogin()
        {
            if (Request.QueryString["error"] != null)
            {
                TempData["FacebookResultMessage"] = "There was an error validating with Facebook: " + Request.QueryString["error_description"];
                return RedirectToAction("Index");
                // TODO: log error?
            }

            // Exchange code for an access token
            string code = Request.QueryString["code"];
            // Redirect to Facebook to ask for permissions
            string redirectAfterLoginUri = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("Default", new { Controller = "Settings", Action = "ProcessFacebookLogin" });
            object accessTokenGetParams = new
            {
                client_id = appId,
                redirect_uri = redirectAfterLoginUri,
                client_secret = appSecret,
                code = code
            };

            var fbClient = new FacebookClient();
            dynamic getAccessTokenResult = fbClient.Get("/oauth/access_token", accessTokenGetParams);

            string accessToken = getAccessTokenResult.access_token;
            Int64 secondsTilExpiration = getAccessTokenResult.expires;
            DateTime expireTime = DateTime.Now.AddSeconds(secondsTilExpiration);

            // Verify token is valid
            bool isTokenValid = IsUserAccessTokenValid(accessToken);
            if (!isTokenValid)
            {
                TempData["FacebookResultMessage"] = "There was an error validating with Facebook: User access token was invalid.";
                return RedirectToAction("Index");
            }

            // Get user ID
            fbClient = new FacebookClient(accessToken);
            dynamic fbMe = fbClient.Get("me");
            string fbUserId = fbMe.id.ToString();

            // Save data from Facebook into db
            using (UnitOfWork work = new UnitOfWork())
            {
                user currentUser = work.UserRepository.GetUser(WebSecurity.CurrentUserId);
                work.UserRepository.UpdateFacebookDataForExistingConnection(currentUser, fbUserId, accessToken, expireTime);
                work.SaveChanges();
            }

            TempData["FacebookResultMessage"] = "Successfully connected to Facebook!";
            return RedirectToAction("Index");
        }

        bool IsUserAccessTokenValid(string userAccessToken)
        {
            var fbClient = new FacebookClient();

            // TODO: Get app access token from DB
            object appAccessTokenParams = new { client_id = appId, client_secret = appSecret, grant_type = "client_credentials" };
            dynamic appAccessTokenObject = fbClient.Get("/oauth/access_token", appAccessTokenParams);
            string appAccessToken = appAccessTokenObject.access_token;

            // Validate token
            object debugTokenParams = new { input_token = userAccessToken, access_token = appAccessToken };
            dynamic debugTokenResult = fbClient.Get("/debug_token", debugTokenParams);

            string debugTokenAppId = debugTokenResult.data.app_id.ToString();

            // TODO: log error before returning if invalid?

            return debugTokenAppId.Equals(appId); // TODO: verify user ID?
            // && debugTokenResult["user_id"] == userFacebookId;
        }
    }
}
