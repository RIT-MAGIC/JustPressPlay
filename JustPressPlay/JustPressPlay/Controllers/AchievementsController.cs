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
        public Boolean UserSubmission(int achievementID, string type, string text = null, HttpPostedFileBase image = null, string url = null)
        {
            if (String.IsNullOrWhiteSpace(type))
                return false;
            UnitOfWork work = new UnitOfWork();

            switch (type)
            {
                case "image":
                    //image stuff
                    if (image == null)
                        return false;
                    if (!Utilities.JPPImage.FileIsWebFriendlyImage(image.InputStream))
                        return false;                    
                    Utilities.JPPDirectory.CheckAndCreateUserDirectory(WebSecurity.CurrentUserId, Server);
                    String filepath = Utilities.JPPDirectory.CreateFilePath(Utilities.JPPDirectory.ImageTypes.ContentSubmission, WebSecurity.CurrentUserId);
                    //CHANGE IMAGE SIZE
                    Utilities.JPPImage.Save(Server, filepath, image.InputStream, 1000, 200, false);
                    work.AchievementRepository.UserSubmittedContentForImage(achievementID, WebSecurity.CurrentUserId, filepath, text);

                break;
                case "text":
                    //text stuff
                    if (String.IsNullOrWhiteSpace(text))
                        return false;
                    work.AchievementRepository.UserSubmittedContentForText(achievementID, WebSecurity.CurrentUserId, text);
                    break;
                case "url":
                    //url stuff
                        if(String.IsNullOrWhiteSpace(url))
                            return false;
                    work.AchievementRepository.UserSubmittedContentForURL(achievementID, WebSecurity.CurrentUserId, text, url);
                    break;
                default:
                    //Nope
                    return false;
            }

            return true;
        }


        //TODO: Write validation checks on this side
        [Authorize]
        [HttpPost]
        public ActionResult ManageAchievementStory(int instanceID, string storyText = null, HttpPostedFileBase storyImage = null)
        {
            UnitOfWork work = new UnitOfWork();
            if (!HttpContext.Request.IsAjaxRequest() || work.AchievementRepository.InstanceExists(instanceID) == null)
                return new HttpStatusCodeResult(406, "Invalid request"); // Invalid request

            try
            {
                var image = false;
                var text = false;

                // TODO: Change for editing, should init to null or existing image filepath
                String imagePath = null;
                if (storyImage != null)
                {
                    if (!Utilities.JPPImage.FileIsWebFriendlyImage(storyImage.InputStream))
                        return new HttpStatusCodeResult(406, "Invalid image type"); // Invalid image type

                    Utilities.JPPDirectory.CheckAndCreateUserDirectory(WebSecurity.CurrentUserId, Server);
                    String filepath = Utilities.JPPDirectory.CreateFilePath(Utilities.JPPDirectory.ImageTypes.UserStory, WebSecurity.CurrentUserId);
                    Utilities.JPPImage.Save(Server, filepath, storyImage.InputStream, 1000, 200, false);
                    work.AchievementRepository.UserAddAchievementStoryImage(instanceID, filepath);
                    imagePath = filepath;
                    image = true;
                }
                if (!String.IsNullOrWhiteSpace(storyText))
                {
                    work.AchievementRepository.UserAddAchievementStoryText(instanceID, storyText);
                    text = true;
                }

                if (!image && !text)
                    return new HttpStatusCodeResult(406, "Empty story form submission"); // Empty form submission

                StoryData storyResult = new StoryData()
                {
                    StoryText = storyText,
                    StoryImage = imagePath
                };
                
                return Json(storyResult);

            }
            catch(Exception e)
            {
                return new HttpStatusCodeResult(500, "Save story exception"); // Invalid request;
            }

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
