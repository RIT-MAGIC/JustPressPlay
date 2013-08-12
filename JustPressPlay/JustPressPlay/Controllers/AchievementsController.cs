using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.ViewModels;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.Controllers
{
    public class AchievementsController : Controller
    {
		/// <summary>
		/// Handles the home page of the Achievements section
		/// </summary>
		/// <returns>GET: /Achievements</returns>
        public ActionResult Index()
        {
			// Get the list of ALL achievements
			AchievementsListViewModel model = AchievementsListViewModel.Populate();
            return View(model);
        }

		/*/// <summary>
		/// Handles an individual achievement's page
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <returns>GET: /Achievements/{id}</returns>
		public ActionResult IndividualAchievement(int id)
		{
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("IndividualAchievement", new { id = id, playerID = WebSecurity.CurrentUserId });

			AchievementViewModel model =
				AchievementViewModel.Populate(
					id,
					WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserId : (int?)null);

			return View(model);
		}*/

        
        public ActionResult IndividualAchievement(int id, int? playerID)
        {
            //if (playerID == null && WebSecurity.IsAuthenticated)
            //    return RedirectToAction("IndividualAchievement", new { id = id, playerID = WebSecurity.CurrentUserId });

            AchievementViewModel model =
                AchievementViewModel.Populate(
                    id,
                    playerID);

            ViewBag.servername = Request.Url.GetLeftPart(UriPartial.Authority);

            return View(model);
        }

		/*/// <summary>
		/// Page for a player's specific instance of an achievement
		/// </summary>
		/// <param name="achievementID">The id of the achievement</param>
		/// <param name="playerID">The id of the player</param>
		/// <returns>GET: /Achievements/{achievementID}/{playerID}</returns>
		public ActionResult AchievementPlayer(int achievementID, int playerID)
		{
			ViewBag.achievementID = achievementID;
			ViewBag.playerID = playerID;
			return View();
		}*/

        //TODO: FIX IT FELIX (CHANGE RETURN TYPE AND VALUES TO PLUG INTO FRONTEND)
        [Authorize]
        [HttpPost]
        public Boolean AchievementImageSubmission(int achievementID, HttpPostedFileBase image, String text)
        {
            if (!HttpContext.Request.IsAjaxRequest())
            {
                return false;
            }
            if (image == null)
            {
                return false;
            }

            if(!Utilities.JPPImage.FileIsWebFriendlyImage(image.InputStream))
                return false;

            Utilities.JPPDirectory.CheckAndCreateUserDirectory(WebSecurity.CurrentUserId, Server);            
            String filepath = Utilities.JPPDirectory.CreateFilePath(Utilities.JPPDirectory.ImageTypes.ContentSubmission, WebSecurity.CurrentUserId);
            //CHANGE IMAGE SIZE
            Utilities.JPPImage.Save(Server, filepath, image.InputStream, 2000, false);

            UnitOfWork work = new UnitOfWork();


            return work.AchievementRepository.UserSubmittedContentForImage(achievementID, WebSecurity.CurrentUserId, filepath, text);
        }


        [Authorize]
        [HttpPost]
        public Boolean AchievementTextSubmission(int achievementID, String text)
        {
            if (!HttpContext.Request.IsAjaxRequest())
            {
                return false;
            }
            if (String.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            UnitOfWork work = new UnitOfWork();
            return work.AchievementRepository.UserSubmittedContentForText(achievementID, WebSecurity.CurrentUserId, text);
        }

        [Authorize]
        [HttpPost]
        public Boolean AchievementURLSubmission(int achievementID, String text, String url)
        {
            if (!HttpContext.Request.IsAjaxRequest())
            {
                return false;
            }
            if(String.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            UnitOfWork work = new UnitOfWork();
            return work.AchievementRepository.UserSubmittedContentForURL(achievementID, WebSecurity.CurrentUserId, text, url);
        }

        [Authorize]
        [HttpPost]
        public Boolean AddAchievementStoryImage(int instanceID, HttpPostedFileBase image)
        {
            if (!HttpContext.Request.IsAjaxRequest())
            {
                return false;
            }
            if (image == null)
            {
                return false;
            }
            if (!Utilities.JPPImage.FileIsWebFriendlyImage(image.InputStream))
            {
                return false;
            }

            Utilities.JPPDirectory.CheckAndCreateUserDirectory(WebSecurity.CurrentUserId, Server);

            String filepath = Utilities.JPPDirectory.CreateFilePath(Utilities.JPPDirectory.ImageTypes.UserStory, WebSecurity.CurrentUserId);
            Utilities.JPPImage.Save(Server, filepath, image.InputStream, 2000, false);

            UnitOfWork work = new UnitOfWork();

            return work.AchievementRepository.UserAddAchievementStoryImage(instanceID, filepath);
        }

        [Authorize]
        [HttpPost]
        public Boolean AddAchievementStoryImage(int instanceID, String text)
        {
            if (!HttpContext.Request.IsAjaxRequest())
            {
                return false;
            }
            if (String.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            UnitOfWork work = new UnitOfWork();
            return work.AchievementRepository.UserAddAchievementStoryText(instanceID, text);
        }
    }
}
