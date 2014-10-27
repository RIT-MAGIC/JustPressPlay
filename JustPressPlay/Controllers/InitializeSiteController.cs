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
using System.Web.Security;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;
using WebMatrix.WebData;
using System.Net;

namespace JustPressPlay.Controllers
{
    public class InitializeSiteController : Controller
    {
        //
        // GET: /InitializeSite/

        public ActionResult Index()
        {
            if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminAccountCreated)))
                return RedirectToAction("Index", "Home");
            ViewBag.EmailSent = false;
            var model = new CreateAdminAccountViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(CreateAdminAccountViewModel model)
        {
            if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminAccountCreated)))
                return RedirectToAction("Index", "Home");
            if (ModelState.IsValid)
            {
                String confirmationToken = WebSecurity.CreateUserAndAccount(
                        model.Username,
                        model.Password,
                        new
                        {
                            first_name = "Admin",
                            middle_name = "Admin",
                            last_name = "Admin",
                            is_player = false,
                            created_date = DateTime.Now,
                            status = (int)JPPConstants.UserStatus.Active,
                            first_login = false,
                            email = model.Email,
                            last_login_date = DateTime.Now,
                            display_name = "Admin",
                            privacy_settings = (int)JPPConstants.PrivacySettings.FriendsOnly,
                            has_agreed_to_tos = true,
                            communication_settings = (int)JPPConstants.CommunicationSettings.All,
                            notification_settings = 0
                        },
                        true);

                String[] roles = new String[1];
                roles[0] = JPPConstants.Roles.FullAdmin;
                JPPConstants.Roles.UpdateUserRoles(model.Username, roles);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminAccountCreated, "true");
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminUsername, model.Username);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminEmail, model.Email);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SMTPServer, model.SMTPServer);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SMTPPort, model.Port.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SMTPPassword, model.SMTPPassword.ToString());

                String confirmLink = "http://" + Request.Url.Authority + "/InitializeSite/Confirm?token=" + confirmationToken;

                Email.Send(model.Email, "Admin Account Created", "Here is your registration confirmation link:\n\n" + "<a href='" + confirmLink + "'>" + confirmLink + "</a>", true);

                // All done
                ViewBag.EmailSent = true;

                return View();

            }


            return View(model);
        }

        /// <summary>
        /// Confirms the user's registration
        /// </summary>
        /// <param name="token">The confirmation token</param>
        /// <returns>GET: /Players/Confirm?token=...</returns>
        [AllowAnonymous]
        public ActionResult Confirm(String token)
        {
            if (String.IsNullOrWhiteSpace(token))
                return View();
            String username = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminUsername);
            DateTime createdDate = WebSecurity.GetCreateDate(username);
            // Attempt to validate
            if (WebSecurity.ConfirmAccount(token))
            {
                if (createdDate.AddHours(12) > DateTime.Now)
                {
                    return RedirectToAction("InitializeSiteSettings");
                }
                else
                {
                    if (Roles.GetRolesForUser(username).Count() > 0)
                    {
                        Roles.RemoveUserFromRoles(username, Roles.GetRolesForUser(username));
                    }
                    ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(username);
                    ((SimpleMembershipProvider)Membership.Provider).DeleteUser(username, true);
                    JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminAccountCreated, false.ToString());
                    JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminEmail, "");
                    JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.AdminUsername, "");
                }
            }
            else
            {
                ViewBag.InvalidToken = true;

            }
            

            return View();
        }

        [Authorize]
        public ActionResult InitializeSiteSettings()
        {
            if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SiteInitialized)))
                return RedirectToAction("Index", "Home");

            ManageSiteSettingsViewModel model = ManageSiteSettingsViewModel.Populate();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult InitializeSiteSettings(ManageSiteSettingsViewModel model)
        {
            if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SiteInitialized)))
                return RedirectToAction("Index", "Home");

            if (model.SiteLogo != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.SiteLogo.InputStream))
                    ModelState.AddModelError("Icon", "Image must be of type .jpg, .gif, or .png");

            if (ModelState.IsValid)
            {
                if (model.SiteLogo != null)
                {
                    Utilities.JPPDirectory.CheckAndCreateSiteContentDirectory(Server);
                    model.SiteLogoFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.SiteContent);
                    Utilities.JPPImage.Save(Server, model.SiteLogoFilePath, model.SiteLogo.InputStream, JPPConstants.Images.SiteLogoMaxSize, 0, false);
                }

                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorNavBar, model.NavBarColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorCreate, model.CreateColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorExplore, model.ExploreColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorLearn, model.LearnColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorSocialize, model.SocializeColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorQuest, model.QuestColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SchoolName, model.OrganizationName);
                if (model.SiteLogoFilePath != null) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SchoolLogo, model.SiteLogoFilePath);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.CardDistributionEnabled, model.EnableCardDistribution.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SelfRegistrationEnabled, model.AllowSelfRegistration.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.UserGeneratedQuestsEnabled, model.AllowUserGeneratedQuests.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.CommentsEnabled, model.AllowComments.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled, model.EnableFacebookIntegration.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.DevPassword, model.DevPassword.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.DevPasswordEnabled, model.DevPasswordEnabled.ToString());
                if (!string.IsNullOrWhiteSpace(model.FacebookAppId)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppId, model.FacebookAppId);
                if (!string.IsNullOrWhiteSpace(model.FacebookAppSecret)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppSecret, model.FacebookAppSecret);
                if (!string.IsNullOrWhiteSpace(model.FacebookAppNamespace)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppNamespace, model.FacebookAppNamespace);

                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SiteInitialized, true.ToString());
                return RedirectToAction("Index"); // TODO: show success?
            }
            return View();
        }



    }
}
