using Facebook;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace JustPressPlay.Controllers
{
    [Authorize]
    public class FacebookConnectionController : Controller
    {
        // TODO: Move to site settings DB once implemented
        string appId = "295662587237899";
        string appSecret = "2456db6dc3c7c0e8e76913ab6d6e1028"; // TODO: RESET IF COMMITTED TO GIT!

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
            // Save user settings to db
            using (UnitOfWork work = new UnitOfWork())
            {
                user currentUser = work.UserRepository.GetUser(WebSecurity.CurrentUserId);
                work.UserRepository.AddOrUpdateFacebookSettings(currentUser, model.NotificationsEnabled, model.AutomaticSharingEnabled);
                work.SaveChanges();
            }

            // Redirect to Facebook to ask for permissions
            string redirectAfterLoginUri = "http://localhost:5376/FacebookConnection/ProcessFacebookLogin"; // TODO: generate dynamically?
            string scope = string.Empty; // NOTE: No change in scope needed for notifications; apps don't need to ask permission
            if (model.AutomaticSharingEnabled)
            {
                scope += "publish_actions,";
            }
            string fbRedirectUrl = string.Format("https://www.facebook.com/dialog/oauth"
                                                 + "?client_id={0}"
                                                 + "&redirect_uri={1}",
                                                 appId, redirectAfterLoginUri); // TODO: state, response_type: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/
            Response.Redirect(fbRedirectUrl);

            // Shouldn't ever get here; if we do, re-show the form
            return View(model);
        }

        [HttpGet]
        public string ProcessFacebookLogin()
        {
            if (Request.QueryString["error"] != null)
                return "An error occurred :("; // TODO: More robust error handling

            // Exchange code for an access token
            string code = Request.QueryString["code"];
            string redirectAfterLoginUri = "http://localhost:5376/FacebookConnection/ProcessFacebookLogin"; // TODO: generate dynamically?
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
                return "Token was not valid :("; // TODO: More robust error handling
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

            return "user_id: " + fbUserId + ", Expiration DateTime: " + expireTime + ", Seconds til expiration: " + secondsTilExpiration.ToString() + ", accessToken: " + accessToken;
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

            return debugTokenAppId.Equals(appId); // TODO: verify user ID?
                // && debugTokenResult["user_id"] == userFacebookId;

        }
    }
}
