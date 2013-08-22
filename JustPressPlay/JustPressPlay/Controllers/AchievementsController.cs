using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.ViewModels;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;

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

            AchievementViewModel model = AchievementViewModel.Populate(id);

            ViewBag.servername = Request.Url.GetLeftPart(UriPartial.Authority);

            bool fbEnabled = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled));
            if (fbEnabled)
            {
                ViewBag.FacebookMetaEnabled = true;
                ViewBag.FacebookAppId = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId);
                ViewBag.FacebookOgUrl = Request.Url.GetLeftPart(UriPartial.Path);
                ViewBag.FacebookOgType = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppNamespace)
                                         + ":achievement";
                ViewBag.FacebookOgTitle = model.Title;
                ViewBag.FacebookOgImageUri = JppUriInfo.GetAbsoluteUri(Request, model.Image);
                ViewBag.FacebookOgDescription = model.Description;
            }

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

        [Authorize]
        [HttpPost]
        public void UserSubmission(int achievementID, string type, string text = null, HttpPostedFileBase image = null, string url = null)
        {
            if (String.IsNullOrWhiteSpace(type))
                return;
            if (achievementID == null)
                return;
            UnitOfWork work = new UnitOfWork();

            switch (type)
            {
                case "image":
                    //image stuff
                    if (image == null)
                        return;
                    if (!Utilities.JPPImage.FileIsWebFriendlyImage(image.InputStream))
                        return;                    
                    Utilities.JPPDirectory.CheckAndCreateUserDirectory(WebSecurity.CurrentUserId, Server);
                    String filepath = Utilities.JPPDirectory.CreateFilePath(Utilities.JPPDirectory.ImageTypes.ContentSubmission, WebSecurity.CurrentUserId);
                    //CHANGE IMAGE SIZE
                    Utilities.JPPImage.Save(Server, filepath, image.InputStream, 2000, false);
                    work.AchievementRepository.UserSubmittedContentForImage(achievementID, WebSecurity.CurrentUserId, filepath, text);

                break;
                case "text":
                    //text stuff
                    if (String.IsNullOrWhiteSpace(text))
                        return;
                    work.AchievementRepository.UserSubmittedContentForText(achievementID, WebSecurity.CurrentUserId, text);
                    break;
                case "url":
                    //url stuff
                        if(String.IsNullOrWhiteSpace(url))
                            return;
                    work.AchievementRepository.UserSubmittedContentForURL(achievementID, WebSecurity.CurrentUserId, text, url);
                    break;
                default:
                    //Nope
                    break;
            }

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
        public Boolean AddAchievementStoryText(int instanceID, String text)
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
