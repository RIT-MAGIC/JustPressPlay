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
        public JsonResult GetAssertion(int userID, int achievementID)
        {
            // Get data from DB
            string userEmail;
            string achievementImageURL;
            using (UnitOfWork work = new UnitOfWork())
            {
                userEmail = work.UserRepository.GetUser(userID).email;
                //achievement_template achievement = work.AchievementRepository.GetTemplateById(achievementID);
                //achievementImageURL = achievement.icon;
            }
            string hashedEmail, salt;
            Sha256Helper.HashString(userEmail, out hashedEmail, out salt);

            var badgeAssertion = new
            {
                uid = GenerateUniqueId(userID, achievementID),
                recipient = new {
                    identity = "sha256$" + hashedEmail,
                    type = "email",
                    hashed = true,
                    salt = salt
                    },

            };

            return Json(badgeAssertion, JsonRequestBehavior.AllowGet);
        }

        public static string GenerateUniqueId(int userID, int achievementID)
        {
            return 'u' + userID.ToString() + 'a' + achievementID.ToString();
        }
    }
}
