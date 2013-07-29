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
        string appId = "295662587237899";
        string appSecret = "2456db6dc3c7c0e8e76913ab6d6e1028"; // TODO: RESET AFTER COMMITTED!

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
        public string ConnectFacebook(FacebookConnectionViewModel model)
        {
            var fbClient = new FacebookClient();
            

            string redirectAfterLoginUri = "http://localhost:5376/FacebookConnection/ProcessFacebookLogin";
            string fbRedirectUrl = string.Format("https://www.facebook.com/dialog/oauth"
                                                 + "?client_id={0}"
                                                 + "&redirect_uri={1}",
                                                 appId, redirectAfterLoginUri); // TODO: state, response_type: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/
            Response.Redirect(fbRedirectUrl);

            // Do we ever get here?
            return fbClient.Get("zach.hoefler").ToString();
        }

        [HttpGet]
        public string ProcessFacebookLogin()
        {
            // TODO: process login

            if (Request.QueryString["error"] != null)
                return "An error occurred :(";

            // Exchange code for an access token
            string code = Request.QueryString["code"];
            string redirectAfterLoginUri = "http://localhost:5376/FacebookConnection/ProcessFacebookLogin";
            object accessTokenGetParams = new
            {
                client_id = appId,
                redirect_uri = redirectAfterLoginUri,
                client_secret = appSecret,
                code = code
            };

            var fbClient = new FacebookClient();
            JsonObject result = fbClient.Get<JsonObject>("https://graph.facebook.com/oauth/access_token", accessTokenGetParams);
/*
    GET https://graph.facebook.com/oauth/access_token?
    client_id={app-id}
   &redirect_uri={redirect-uri}
   &client_secret={app-secret}
   &code={code-parameter} */

            string accessToken = (string)result["access_token"];
            Int64 secondsTilExpiration = (Int64)result["expires"];
            DateTime expireTime = DateTime.Now.AddSeconds(secondsTilExpiration);

            // TODO: inspect token

            return "Expiration DateTime: " + expireTime + ", Seconds til expiration: " + secondsTilExpiration.ToString() + "\n, accessToken: " + accessToken;
        }
    }
}
