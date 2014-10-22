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
        /// <summary>
        /// A player's settings
        /// </summary>
        /// <returns>GET: /Settings</returns>
        public ActionResult Index()
        {
            ViewBag.IsFacebookEnabled = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled));

            if (TempData["FacebookResultMessage"] != null)
                ViewBag.FacebookResultMessage = TempData["FacebookResultMessage"];

            UserSettingsViewModel model = UserSettingsViewModel.Populate(WebSecurity.CurrentUserId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(UserSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (UnitOfWork work = new UnitOfWork())
                {
                    work.UserRepository.UpdateUserSettings(WebSecurity.CurrentUserId, model.CommunicationSettings, model.PrivacySettings);
                    work.SaveChanges();
                }

                // TODO: Figure out why model isn't being refreshed properly
                return RedirectToAction("Index");
            }

            // TODO: refresh model?
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
            string redirectAfterLoginUri = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("Default", new { Controller = "Settings", Action = "ProcessFacebookLogin" });
            string scope = string.Empty; // NOTE: No change in scope needed for notifications; apps don't need to ask permission
            if (model.AutomaticSharingEnabled)
            {
                scope += "publish_actions,";
            }
            string appId = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId);
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
                client_id = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId),
                redirect_uri = redirectAfterLoginUri,
                client_secret = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppSecret),
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

            string appAccessToken = JppFacebookHelper.GetAppAccessToken(fbClient);

            // Validate token
            object debugTokenParams = new { input_token = userAccessToken, access_token = appAccessToken };
            dynamic debugTokenResult = fbClient.Get("/debug_token", debugTokenParams);

            string debugTokenAppId = debugTokenResult.data.app_id.ToString();

            // TODO: log error before returning if invalid?

            return debugTokenAppId.Equals(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId)); // TODO: verify user ID?
            // && debugTokenResult["user_id"] == userFacebookId;
        }
    }
}
