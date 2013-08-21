using JustPressPlay.Models.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    }
}
