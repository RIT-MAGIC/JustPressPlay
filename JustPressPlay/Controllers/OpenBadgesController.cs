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

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace JustPressPlay.Controllers
{
    public class OpenBadgesController : Controller
    {
        // TODO: Check if achievements are public before offering achievement data via API

        /// <summary>
        /// Verifies that a badge is valid
        /// </summary>
        /// <returns>HTTP 200 (OK) if valid; another status code if not</returns>
        [HttpGet]
        public ActionResult VerifyBadge(int userID, int achievementID)
        {
            bool validEarnedAchievement = false;

            using (UnitOfWork work = new UnitOfWork())
            {
                if (work.AchievementRepository.DoesUserHaveAchievement(userID, achievementID))
                    validEarnedAchievement = true;
            }

            if (validEarnedAchievement)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            // Not a valid achievement. Can be any status code besides 200
            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// See: https://github.com/mozilla/openbadges/wiki/Assertions
        /// </summary>
        [HttpGet]
        public ActionResult Assertion(int userID, int achievementID)
        {
            string userEmail, achievementImageURL;
            DateTime achievementDate;
            using (UnitOfWork work = new UnitOfWork())
            {
                // Verify user actually has achievement
                if (!work.AchievementRepository.DoesUserHaveAchievement(userID, achievementID))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                // If so, get data and generate assertion
                userEmail = work.UserRepository.GetUser(userID).email;

                achievement_template achievementTemplate = work.AchievementRepository.GetTemplateById(achievementID);
                achievementImageURL = JppUriInfo.GetAbsoluteUri(Request, achievementTemplate.icon);

                achievement_instance achievementInstance = work.AchievementRepository.GetUserAchievementInstance(userID, achievementID);
                achievementDate = achievementInstance.achieved_date;
            }
            string salt = "CoeA8DQf"; // As we are exposing the salt anyway, using a constant isn't an issue, and it saves us from having to store every randomly-generated salt in the db
            string hashedEmail;
            hashedEmail = Sha256Helper.HashStringWithSalt(userEmail, salt);

            var badgeAssertion = new
            {
                uid = GenerateUniqueId(userID, achievementID),
                recipient = new
                {
                    identity = "sha256$" + hashedEmail,
                    type = "email",
                    hashed = true,
                    salt = salt
                },
                image = achievementImageURL,
                badge = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("OpenBadgeDescriptionRoute", new { Action = "BadgeDescription", achievementID = achievementID }),
                verify = new
                {
                    type = "hosted",
                    url = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("OpenBadgeRoute", new { Action = "Assertion", userID = userID, achievementID = achievementID }),
                },
                issuedOn = achievementDate.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                evidence = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("AchievementsPlayersRoute", new { id = achievementID, playerID = userID }),
            };

            return Json(badgeAssertion, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// See: https://github.com/mozilla/openbadges/wiki/Assertions#badgeclass
        /// </summary>
        [HttpGet]
        public ActionResult BadgeDescription(int achievementID)
        {
            string achievementTitle, achievementDescription, imageUri;
            using(UnitOfWork work = new UnitOfWork())
            {
                achievement_template template = work.AchievementRepository.GetTemplateById(achievementID);
                if (template == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                achievementTitle = template.title;
                achievementDescription = template.description;
                imageUri = JppUriInfo.GetAbsoluteUri(Request, template.icon);
            }

            var badgeDescription = new
            {
                // TODO: truncate data if too large; see https://wiki.mozilla.org/Badges/Onboarding-Issuer#E._Metadata_Spec
                name = achievementTitle,
                description = achievementDescription,
                image = imageUri,
                criteria = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("AchievementsPlayersRoute", new { id = achievementID }),
                issuer = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("OpenBadgesIssuerRoute"),
                // TODO: tags (optional)
            };

            return Json(badgeDescription, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// See: https://github.com/mozilla/openbadges/wiki/Assertions#issuerorganization
        /// </summary>
        [HttpGet]
        public JsonResult Issuer()
        {
            string organizationName = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolName);
            string organizationLogo = JppUriInfo.GetAbsoluteUri(Request, JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolLogo));

            var issuerOrganization = new
            {
                name = organizationName,
                url = JppUriInfo.GetCurrentDomain(Request), // TODO: Get org-specific URL rather than badge site?
                image = organizationLogo
            };

            return Json(issuerOrganization, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Generates a unique ID for a badge. The ID should be unique local to our site, but not globally
        /// </summary>
        private static string GenerateUniqueId(int userID, int achievementID)
        {
            return 'u' + userID.ToString() + 'a' + achievementID.ToString();
        }
    }
}
