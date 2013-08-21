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
        public JsonResult Assertion(int userID, int achievementID)
        {
            string userEmail, achievementImageURL;
            DateTime achievementDate;
            using (UnitOfWork work = new UnitOfWork())
            {
                // Verify user actually has achievement
                if (!work.AchievementRepository.DoesUserHaveAchievement(userID, achievementID))
                {
                    return Json(new {});
                }

                // If so, get data and generate assertion
                userEmail = work.UserRepository.GetUser(userID).email;

                achievement_template achievementTemplate = work.AchievementRepository.GetTemplateById(achievementID);
                achievementImageURL = JppUriInfo.GetAbsoluteUri(Request, achievementTemplate.icon);

                achievement_instance achievementInstance = work.AchievementRepository.GetUserAchievementInstance(userID, achievementID);
                achievementDate = achievementInstance.achieved_date;
            }
            string hashedEmail, salt;
            Sha256Helper.HashString(userEmail, out hashedEmail, out salt);

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
                badge = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("OpenBadgeRoute", new { Action = "BadgeDescription", userID = userID, achievementID = achievementID }),
                verify = new
                {
                    type = "hosted",
                    url = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("OpenBadgeRoute", new { Action = "VerifyBadge", userID = userID, achievementID = achievementID }),
                },
                issuedOn = achievementDate,
                evidence = JppUriInfo.GetCurrentDomain(Request) + Url.RouteUrl("AchievementsPlayersRoute", new { id = achievementID, playerID = userID }),
            };

            return Json(badgeAssertion, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// See: https://github.com/mozilla/openbadges/wiki/Assertions#badgeclass
        /// </summary>
        [HttpGet]
        public JsonResult BadgeDescription(int userID, int achievementID)
        {
            // TODO: Generate badge description
            var badgeDescription = new
            {
            };

            throw new NotImplementedException("Badge descriptions have not yet been implemented");

            return Json(badgeDescription);
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
