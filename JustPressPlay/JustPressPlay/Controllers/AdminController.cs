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

using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

namespace JustPressPlay.Controllers
{
	[Authorize]
    public class AdminController : Controller
    {
        /// <summary>
        /// Admin home page
        /// </summary>
        /// <returns>GET: /Admin</returns>
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                if (!String.IsNullOrWhiteSpace(TempData["Message"].ToString()))
                    ViewBag.Message = TempData["Message"].ToString();
            }
            return View();
        }

        #region Add/Edit Users

        /// <summary>
		/// Adds a user to the site
		/// </summary>
		/// <returns>GET: /Admin/AddUser</returns>
		[Authorize(Roles = JPPConstants.Roles.CreateUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult AddUser()
		{
			return View();
		}

		/// <summary>
		/// Post-back for adding a user to the site
		/// </summary>
		/// <param name="model">Information about the new user</param>
		/// <returns>POST: /Admin/Adduser</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = JPPConstants.Roles.CreateUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult AddUser(AddUserViewModel model)
		{
            //return model.Image.InputStream.ToString();
			if (ModelState.IsValid)
			{
				try
				{
					WebSecurity.CreateUserAndAccount(
						model.Username,
						model.Password,
						new
						{
							first_name = model.FirstName,
							middle_name = model.MiddleName,
							last_name = model.LastName,
							is_player = model.IsPlayer,
							created_date = DateTime.Now,
							status = (int)JPPConstants.UserStatus.Active,
							first_login = true,
							email = model.Email,
							last_login_date = DateTime.Now,
							display_name = model.DisplayName,
							privacy_settings = (int)JPPConstants.PrivacySettings.FriendsOnly,
							has_agreed_to_tos = false,
							creator_id = WebSecurity.CurrentUserId,
							communication_settings = (int)JPPConstants.CommunicationSettings.All,
							notification_settings = 0,
						}, 
						false);

                    TempData["Message"] = "User " + model.Username + " successfully created.";

                    UnitOfWork work = new UnitOfWork();
                    user user = work.UserRepository.GetUser(model.Username);
                    try
                    {
                        Utilities.JPPDirectory.CheckAndCreateUserDirectory(user.id, Server);

                        String qrString = Request.Url.GetLeftPart(UriPartial.Authority) + "/Players/" + user.id;
                        //Create the file path and save the image
                        String qrfilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.UserQRCode, user.id);
                        String qrfileMinusPath = qrfilePath.Replace("~/Content/Images/Users/" + user.id.ToString() + "/UserQRCodes/", "");
                        //"/Users/" + userID.ToString() + "/ProfilePictures/" + fileName + ".png";
                        if (JPPImage.SavePlayerQRCodes(qrfilePath, qrfileMinusPath, qrString))
                        {
                            user.qr_image = qrfilePath;
                            work.SaveChanges();
                        }

                        if (model.Image != null && user != null)
                        {
                            Utilities.JPPDirectory.CheckAndCreateUserDirectory(user.id, Server);

                            //Create the file path and save the image
                            String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.ProfilePicture, user.id);
                            String fileMinusPath = filePath.Replace("~/Content/Images/Users/" + user.id.ToString() + "/ProfilePictures/", "");
                            //"/Users/" + userID.ToString() + "/ProfilePictures/" + fileName + ".png";
                            if (JPPImage.SavePlayerImages(filePath, fileMinusPath, model.Image.InputStream))
                            {
                                user.image = filePath;
                                work.SaveChanges();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TempData["Message"] = TempData["Message"].ToString() +" However, there was an error uploading the profile picture: " + e.Message;
                    }

					//Success
					return RedirectToAction("Index");
				}
				catch (Exception e)
				{
					// Problem!
					ModelState.AddModelError("", "There was a problem adding the user: " + e.Message);
				}
			}

			// Something went wrong, redisplay
			return View(model);
		}

		/// <summary>
		/// Shows a list of users to be editted
		/// </summary>
		/// <returns>GET: /Admin/EditUserList</returns>
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.ModerateAchievementsAndStories+ "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUserList()
		{
            if (TempData["Message"] != null)
            {
                if (!String.IsNullOrWhiteSpace(TempData["Message"].ToString()))
                    ViewBag.Message = TempData["Message"].ToString();
            }
			UserListViewModel model = UserListViewModel.Populate();
            foreach (var u in model.Users)
            {
                u.LastLoginString = u.LastLogin.ToShortDateString();
            }
			return View(model);
		}

		/// <summary>
		/// Allows an admin to edit a user
		/// </summary>
		/// <returns>GET: /Admin/EditUser/{id}</returns>
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUser(int id = 0)
		{
			if (id <= 0)
				return RedirectToAction("EditUserList");

			EditUserViewModel model = EditUserViewModel.Populate(id);
			return View(model);
		}

		/// <summary>
		/// Post-back for editting a user
		/// </summary>
		/// <param name="user">The user being edited</param>
		/// <returns>POST: /Admin/EditUser/{id}</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUser(EditUserViewModel model)
		{
            
                UnitOfWork work = new UnitOfWork();
                user user = work.UserRepository.GetUser(model.ID);

                //Commented Out to make Dev easier
                if (model.Roles == null || !model.Roles.Contains(JPPConstants.Roles.FullAdmin))
                {
                    if (user.username.ToLower().Equals(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminUsername).ToLower()))
                        ModelState.AddModelError(String.Empty, "This user is required to be a Full Admin");
                }

                // Valid?
                if (ModelState.IsValid)
                {
                   // try
                    //{
                        /*if (model.Image != null)
                        {
                            Utilities.JPPDirectory.CheckAndCreateUserDirectory(model.ID, Server);

                            //Create the file path and save the image
                            String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.ProfilePicture, model.ID);
                            String fileMinusPath = filePath.Replace("~/Content/Images/Users/" + model.ID.ToString() + "/ProfilePictures/", "");
                            //"/Users/" + userID.ToString() + "/ProfilePictures/" + fileName + ".png";
                            if (JPPImage.SavePlayerImages(filePath, fileMinusPath, model.Image.InputStream))
                            {
                                if (user != null)
                                {
                                    user.image = filePath;
                                }
                            }
                        }*/
                        List<LoggerModel> loggerList = new List<LoggerModel>();
                        // Put the data back into the database
                        if (user != null)
                        {
                            //Check Display Name
                            if (!String.Equals(model.DisplayName, user.display_name))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.DisplayNameEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.display_name,
                                    Value2 = model.DisplayName
                                });
                                //Change the DB entry
                                user.display_name = model.DisplayName;
                            }

                            //Check Email
                            if (!String.Equals(model.Email, user.email))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.EmailEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.email,
                                    Value2 = model.Email
                                });
                                //Change the DB entry
                                user.email = model.Email;
                            }
                            
                            //Check IsPlayer
                            if (model.IsPlayer != user.is_player)
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.IsPlayerEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.is_player.ToString(),
                                    Value2 = model.IsPlayer.ToString()
                                });
                                //Change the DB entry
                                user.is_player = model.IsPlayer;
                            }
                            //Check First Name
                            if (!String.Equals(model.FirstName, user.first_name))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.FirstNameEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.first_name,
                                    Value2 = model.FirstName
                                });
                                //Change the DB entry
                                user.first_name = model.FirstName;
                            }

                            //Check Middle Name
                            if (!String.Equals(model.MiddleName, user.middle_name))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.MiddleNameEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.middle_name,
                                    Value2 = model.MiddleName
                                });
                                //Change the DB entry
                                user.middle_name = model.MiddleName;
                            }

                            //Check Last Name
                            if (!String.Equals(model.LastName, user.last_name))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.LastNameEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.last_name,
                                    Value2 = model.LastName
                                });
                                //Change the DB entry
                                user.last_name = model.LastName;
                            }
                            //Check the six word bio
                            String modelSixWordBio = model.SixWordBio1 == null ? "" : model.SixWordBio1.Replace(" ", "") + " ";
                            modelSixWordBio += model.SixWordBio2 == null ? "" : model.SixWordBio2.Replace(" ", "") + " ";
                            modelSixWordBio += model.SixWordBio3 == null ? "" : model.SixWordBio3.Replace(" ", "") + " ";
                            modelSixWordBio += model.SixWordBio4 == null ? "" : model.SixWordBio4.Replace(" ", "") + " ";
                            modelSixWordBio += model.SixWordBio5 == null ? "" : model.SixWordBio5.Replace(" ", "") + " ";
                            modelSixWordBio += model.SixWordBio6 == null ? "" : model.SixWordBio6.Replace(" ", "");
                            if (!String.Equals(user.six_word_bio, modelSixWordBio))
                            {
                                //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.SixWordBioEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.six_word_bio,
                                    Value2 = modelSixWordBio
                                });
                                //Change the DB entry
                                user.six_word_bio = modelSixWordBio;
                            }

                            if(!String.Equals(model.FullBio, user.full_bio))
                            {
                                 //Add it to the list to log first to get the old value
                                loggerList.Add(new LoggerModel()
                                {
                                    Action = Logger.EditProfileContentLogType.FullBioEdit.ToString(),
                                    UserID = WebSecurity.CurrentUserId,
                                    IPAddress = Request.UserHostAddress,
                                    TimeStamp = DateTime.Now,
                                    IDType1 = Logger.LogIDType.User.ToString(),
                                    ID1 = user.id,
                                    Value1 = user.full_bio,
                                    Value2 = model.FullBio
                                });
                                //Change the DB entry
                                user.full_bio = model.FullBio;
                            }
                           
                            user.modified_date = DateTime.Now;

                            Logger.LogMultipleEntries(loggerList, work.EntityContext);
                            // Save the changes, then add the user to the roles
                            work.SaveChanges();
                            JPPConstants.Roles.UpdateUserRoles(user.username, model.Roles);

                            // Success
                            TempData["Message"] = "Successfully saved changes to " + user.first_name + " " + user.last_name;
                            return RedirectToAction("EditUserList");
                        }
                        else
                        {
                            ModelState.AddModelError("", "The specified user could not be found");
                        }
                   // }
                   // catch (Exception e)
                  //  {
                   //     ModelState.AddModelError("", e.Message);
                    //}
                }            

            model.Roles = Roles.GetRolesForUser(user.username);
            model.ImageURL = user.image;
			// Problem, redisplay
			return View(model);
		}

        #endregion

        #region Add/Edit Achievements
        /// <summary>
        /// Adds an achievement to the database
        /// </summary>
        /// <returns>GET: /admin/addachievement</returns>
        [Authorize(Roles = JPPConstants.Roles.CreateAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddAchievement()
        {
            AddAchievementViewModel model = AddAchievementViewModel.Populate();
            return View(model);
        }

        /// <summary>
        /// Post-back for adding an achievement
        /// </summary>
        /// <param name="model">The achievement to be created</param>
        /// <returns>POST: /admin.addachievement</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.CreateAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddAchievement(AddAchievementViewModel model)
        {
            //Add the Logged In User(Creator) ID to the Model
            model.CreatorID = WebSecurity.CurrentUserId;

            //Create a new Unit of Work
            UnitOfWork work = new UnitOfWork();

            #region Modify Model based on achievement type

            //Only scans get caretakers| Only thresholds have a threshold number and parent
            //Only user submissions have content types | Only system achievements have system trigger types
            //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1

            JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
            model.IsRepeatable = !achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? false : model.IsRepeatable;
            model.SelectedCaretakersList = achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? model.SelectedCaretakersList : null;
            model.Threshold = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.Threshold : null;
            model.ParentID = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.ParentID : null;
            model.ContentType = achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? model.ContentType : null;
            model.SystemTriggerType = achievementType.Equals(JPPConstants.AchievementTypes.System) ? model.SystemTriggerType : null;
            model.RepeatDelayDays = model.RepeatDelayDays >= 1 ? model.RepeatDelayDays : 1;
            model.RepeatDelayDays = model.IsRepeatable ? model.RepeatDelayDays : null;

            #endregion

            if (model.Type == (int)JPPConstants.AchievementTypes.System && work.AchievementRepository.SystemAchievementExists((int)model.SystemTriggerType))
                ModelState.AddModelError(String.Empty, "There is already a system achievement of that type");
            if (model.Icon == null && model.UploadedIcon == null)
                ModelState.AddModelError(String.Empty, "An icon must be selected for this achievement");
            if (model.Title != null)
                if (work.AchievementRepository.AchievementTitleExists(model.Title))
                    ModelState.AddModelError("", "An achievement with that title already exists");
            //Check to make sure the model is valid and the image uploaded is an actual image
            if (ModelState.IsValid)
            {
                try
                {
                    if(model.UploadedIcon != null)
                    {
                        String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.NewIconUpload);
                        model.Icon = filePath.Replace("~/Content/Images/Icons/", "");
                        model.Icon = model.Icon.Replace(".png", "");
                        JPPImage.Save(Server, filePath, model.UploadedIcon.InputStream, 400, 400, true);
                    }
                    //Make Sure the Directories Exist
                    Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);

                    //Create the file path and save the image
                    model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.AchievementIcon);
                    if (JPPImage.SaveAchievementIcons(model.IconFilePath, model.Icon, model.PointsCreate, model.PointsExplore, model.PointsLearn, model.PointsSocialize))
                    {
                        //Add the Achievement to the Database
                        work.AchievementRepository.AdminAddAchievement(model);

                        //Return to the Admin index page
                        TempData["Message"] = "Achievement: " + model.Title + " successfully created and is in draft mode awaiting approval.";
                        return RedirectToAction("EditAchievementList");
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Achievement Creation Failed: " + e.Message);
                }
            }

            //ModelState was not valid, refresh the ViewModel
            AddAchievementViewModel refreshModel = AddAchievementViewModel.Populate();
            model.PotentialCaretakersList = refreshModel.PotentialCaretakersList;
            model.ParentAchievements = refreshModel.ParentAchievements;
            model.IconList = refreshModel.IconList;

            //Return the user to the AddAchievement view with the current model
            return View(model);
        }

		//public String ICONTEST()
		//{
		//	JPPImage.SaveAchievementIcons("~/Content/Images/Achievements/Testing32435.png", "acorn", 4, 4, 4, 4);
		//	JPPImage.SaveAchievementIcons("~/Content/Images/Achievements/Testing3243546456.png", "Penguins", 0, 0, 0, 0);
		//	JPPImage.SaveAchievementIcons("~/Content/Images/Achievements/Testing3243456.png", "Penguins", 4, 4, 0, 0);
		//	return "OK";
		//}

        /// <summary>
        /// Gets the list of achievements to edit
        /// </summary>
        /// <returns>GET: /admin/editachievementlist</returns>
        [Authorize(Roles = JPPConstants.Roles.EditAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditAchievementList()
        {
            if (TempData["Message"] != null)
            {
                if (!String.IsNullOrWhiteSpace(TempData["Message"].ToString()))
                    ViewBag.Message = TempData["Message"].ToString();
            }
            EditAchievementListViewModel model = EditAchievementListViewModel.Populate();
            foreach (var m in model.Achievements)
            {
                m.State = ((JPPConstants.AchievementQuestStates)m.NumState).ToString();
                m.Type = ((JPPConstants.AchievementTypes)m.NumType).ToString();
                m.DateCreatedString = m.DateCreated.ToShortDateString();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult DiscardAchievementDraft(int id)
        {
            UnitOfWork work = new UnitOfWork();
            var message = work.AchievementRepository.DiscardAchievementDraft(id);
            if (!String.IsNullOrWhiteSpace(message))
            {
                TempData["Message"] = message;
                return RedirectToAction("EditAchievementList");
            }
            
            return RedirectToAction("EditAchievement", new { id = id });
        }

        /// <summary>
        /// Allows authorized users to edit achievements
        /// </summary>
        /// <param name="id">the id of the achievement to edit</param>
        /// <returns>GET: /admin/editachievement/{id}</returns>
        [Authorize(Roles = JPPConstants.Roles.EditAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditAchievement(int id)
        {
            EditAchievementViewModel model = EditAchievementViewModel.Populate(id);
            return View(model);
        }

        /// <summary>
        /// Post-back for EditAchievement
        /// </summary>
        /// <param name="id">the id of the achievement to edit</param>
        /// <param name="model">the model with which to update the achievement</param>
        /// <returns>POST: /admin/editachievement/{id}</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.EditAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditAchievement(int id, EditAchievementViewModel model)
        {

            
            //Add the Logged In User(Creator) ID to the Model
            model.EditorID = WebSecurity.CurrentUserId;
            ViewBag.ModelStateBeforeCode = ModelState.IsValid;
            //Create a new Unit of Work
            UnitOfWork work = new UnitOfWork();

            JPPConstants.AchievementQuestStates currentAchievementState = (JPPConstants.AchievementQuestStates)work.AchievementRepository.GetAchievementState(id);
            JPPConstants.AchievementQuestStates modelAchievementState = (JPPConstants.AchievementQuestStates)model.State;

            if (!currentAchievementState.Equals(JPPConstants.AchievementQuestStates.Draft) && modelAchievementState.Equals(JPPConstants.AchievementQuestStates.Draft))
                ModelState.AddModelError(String.Empty, "This Achievement was already moved out of draft mode, it cannot be moved back");

            #region Modify Model based on achievement type

            //Only scans get caretakers| Only thresholds have a threshold number and parent
            //Only user submissions have content types | Only system achievements have system trigger types
            //Only scans are repeatable | Only repeatable achievements have a delay, which must be at least 1

            JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
            model.IsRepeatable = !achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? false : model.IsRepeatable;
            model.SelectedCaretakersList = achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? model.SelectedCaretakersList : null;
            model.Threshold = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.Threshold : null;
            model.ParentID = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.ParentID : null;
            model.ContentType = achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? model.ContentType : null;
            model.SystemTriggerType = achievementType.Equals(JPPConstants.AchievementTypes.System) ? model.SystemTriggerType : null;
            model.RepeatDelayDays = model.RepeatDelayDays >= 1 ? model.RepeatDelayDays : 1;
            model.RepeatDelayDays = model.IsRepeatable ? model.RepeatDelayDays : null;

            #endregion

            if (model.Type == (int)JPPConstants.AchievementTypes.System && work.AchievementRepository.SystemAchievementExists((int)model.SystemTriggerType) && id != work.AchievementRepository.GetSystemAchievementID((int)model.SystemTriggerType))
                ModelState.AddModelError(String.Empty, "There is already a system achievement of that type");
            if (model.Icon == null && model.UploadedIcon == null)
                ModelState.AddModelError(String.Empty, "An icon must be selected for this achievement");
            if (model.Title != null)
                if (work.AchievementRepository.AchievementTitleExists(model.Title, id))
                    ModelState.AddModelError("", "An achievement with that title already exists");
            //Check to make sure the model is valid
            if (ModelState.IsValid)
            {
                
                try
                {

                    if (model.UploadedIcon != null)
                    {
                        String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.NewIconUpload);
                        model.Icon = filePath.Replace("~/Content/Images/Icons/", "");
                        model.Icon = model.Icon.Replace(".png", "");
                        JPPImage.Save(Server, filePath, model.UploadedIcon.InputStream, 400, 400, true);
                    }

                    Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                    achievement_template template = work.EntityContext.achievement_template.Find(id);
                    model.IconFilePath = template == null ?
                        Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.AchievementIcon) :
                        template.icon;
                    if (model.IconFilePath.Contains(".jpg"))
                        model.IconFilePath = model.IconFilePath.Replace(".jpg", "");
                    if (!model.IconFilePath.Contains(".png"))
                        model.IconFilePath += ".png";
                   if(JPPImage.SaveAchievementIcons(model.IconFilePath, model.Icon, model.PointsCreate, model.PointsExplore, model.PointsLearn, model.PointsSocialize))
                    {
                        //Add the Achievement to the Database
                        work.AchievementRepository.AdminEditAchievement(id, model);

                        //Return to the Admin index page
                        TempData["Message"] = "The changes to " + model.Title + " were successfully saved.";
                        return RedirectToAction("EditAchievementList");
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError("", "Achievement Editing Failed: " + e.Message);
                }
            }

            //Modelstate was not valid, refresh the ViewModel
            AddAchievementViewModel refreshModel = AddAchievementViewModel.Populate();
            model.PotentialCaretakersList = refreshModel.PotentialCaretakersList;
            model.ParentAchievements = refreshModel.ParentAchievements;
            model.IconList = refreshModel.IconList;
            //Return the user to the EditAchievement view with the current model
            return View(model);
        }

        [Authorize(Roles = JPPConstants.Roles.AssignGlobalAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AssignGlobalAchievement()
        {
            AssignGlobalAchievementViewModel model = AssignGlobalAchievementViewModel.Populate(); 
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = JPPConstants.Roles.AssignGlobalAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AssignGlobalAchievement(AssignGlobalAchievementViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UnitOfWork work = new UnitOfWork();
                    work.AchievementRepository.AssignGlobalAchievement(model.AchievementID, model.StartRange, model.EndRange, WebSecurity.CurrentUserId);
                    TempData["Message"] = "Global Achievement Successfully Awarded!";                    
                    return RedirectToAction("Index");
                }
                catch
                {
                }
            }

            AssignGlobalAchievementViewModel refreshModel = AssignGlobalAchievementViewModel.Populate();
            model.Achievements = refreshModel.Achievements;
            return View(model);
        }

        public ActionResult SendAnnouncement()
        {
            return View();
        }

		/// <summary>
		/// Allows an admin to assign individual (non-global) achievements to users
		/// </summary>
		/// <returns>GET: /Admin/AssignIndividualAchievement</returns>
		[Authorize(Roles = JPPConstants.Roles.AssignIndividualAchievements + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult AssignIndividualAchievement()
		{
            if (TempData["Message"] != null)
            {
                if (!String.IsNullOrWhiteSpace(TempData["Message"].ToString()))
                    ViewBag.Message = TempData["Message"].ToString();
            }
			AssignIndividualAchievementViewModel model = AssignIndividualAchievementViewModel.Populate();
			return View(model);
		}

		/// <summary>
		/// Post-back for assigning individual achievements to users
		/// </summary>
		/// <param name="model">The selected achievement and user</param>
		/// <returns>POST: /Admin/AssignIndividualAchievement</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = JPPConstants.Roles.AssignIndividualAchievements + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult AssignIndividualAchievement(AssignIndividualAchievementViewModel model)
		{
			if (ModelState.IsValid)
			{
				UnitOfWork work = new UnitOfWork();
                int achievementType = work.AchievementRepository.GetAchievementType(model.AchievementID);
				// Attempt to assign the achievement
                try
                {
                    if(achievementType != (int)JPPConstants.AchievementTypes.UserSubmission)
                        TempData["Message"] = work.AchievementRepository.AssignAchievement(model.UserID, model.AchievementID, WebSecurity.CurrentUserId).ToString();
                    return RedirectToAction("AssignIndividualAchievement");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
			}

            AssignIndividualAchievementViewModel refreshModel = AssignIndividualAchievementViewModel.Populate();
            model.Users = refreshModel.Users;
            model.Achievements = refreshModel.Achievements;

			// Problem, return the model
			return View(model);
		}

        [Authorize(Roles = JPPConstants.Roles.ModerateAchievementsAndStories + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditUserAchievementsList(int id)
        {
            EditUserAchievementsListViewModel model = EditUserAchievementsListViewModel.Populate(id);
            foreach (var a in model.Achievements)
            {
                a.AchievementType = ((JPPConstants.AchievementTypes)a.NumAchievementType).ToString();
                a.DateAchievedString = a.DateAchieved.ToShortDateString();
            }
            return View(model);
        }

        public ActionResult EditUserAchievement(int id)
        {
            EditUserAchievementViewModel model = EditUserAchievementViewModel.Populate(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUserAchievement()
        {
            return View();
        }

        public ActionResult PendingUserSubmissionsList()
        {
            PendingUserSubmissionsListViewModel model = PendingUserSubmissionsListViewModel.Populate();
            return View(model);
        }

        public ActionResult ViewPendingSubmission(int id)
        {
            ViewPendingSubmissionViewModel model = ViewPendingSubmissionViewModel.Populate(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult ViewPendingSubmission(int id, ViewPendingSubmissionViewModel model)
        {
            if(!model.Approved && String.IsNullOrWhiteSpace(model.Reason))
                ModelState.AddModelError(String.Empty, "A reason must be provided to deny this submission");

            if (ModelState.IsValid)
            {
                UnitOfWork work = new UnitOfWork();
                if (model.Approved)
                {
                    work.AchievementRepository.HandleContentSubmission(id, JPPConstants.HandleUserContent.Approve);
                    return RedirectToAction("PendingUserSubmissionsList");
                }
                else
                {
                    work.AchievementRepository.HandleContentSubmission(id, JPPConstants.HandleUserContent.Deny, model.Reason);
                    return RedirectToAction("PendingUserSubmissionsList");
                }
            }
            ViewPendingSubmissionViewModel refresh = ViewPendingSubmissionViewModel.Populate(id);
            refresh.Reason = model.Reason;
            refresh.Approved = model.Approved;
            return View(refresh);
        }


        #endregion

        #region Create/Edit/Approve Quests

        /*----------------------------------Create----------------------------------------------*/
        /// <summary>
        /// The GET action for adding a quest
        /// </summary>
        /// <returns>GET: /Admin/AddQuest</returns>
        [Authorize(Roles = JPPConstants.Roles.CreateQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddQuest()
        {
            //Create the AddQuestViewModel and populate it
            AddQuestViewModel model = AddQuestViewModel.Populate();
            return View(model);
        }

        /// <summary>
        /// The POST action for adding a quest
        /// </summary>
        /// <param name="model">The AddQuestViewModel that gets posted back from the view</param>
        /// <returns>POST: /Admin/AddQuest</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.CreateQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddQuest(AddQuestViewModel model)
        {
            //Create a new Unit of Work
			UnitOfWork work = new UnitOfWork();
            //Add the current logged in user to the model (They are the ones creating it)
            model.CreatorID = WebSecurity.CurrentUserId;
            model.UserGenerated = false;
            //Make sure the quest has associated achievements
            if (model.SelectedAchievementsList == null || model.SelectedAchievementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No Achievements were selected for this quest");

            //Make sure the Threshold value doesn't exceed the number of selected achievements
			if (model.SelectedAchievementsList == null || model.Threshold > model.SelectedAchievementsList.Count)
                ModelState.AddModelError("Threshold", "The Threshold value was greater than the number of achievements selected for this quest.");

            model.Threshold = model.Threshold == null || model.Threshold <= 0 ? model.SelectedAchievementsList.Count : model.Threshold;
            if (model.Icon == null && model.UploadedIcon == null)
                ModelState.AddModelError(String.Empty, "An icon must be selected for this achievement");
            if (model.Title != null)
                if (work.QuestRepository.QuestTitleExists(model.Title))
                    ModelState.AddModelError("", "A quest with that title already exists");
            if (ModelState.IsValid)
            {

                if (model.UploadedIcon != null)
                {
                    String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.NewIconUpload);
                    model.Icon = filePath.Replace("~/Content/Images/Icons/", "");
                    model.Icon = model.Icon.Replace(".png", "");
                    JPPImage.Save(Server, filePath, model.UploadedIcon.InputStream, 400, 400, true);
                }
                //Make Sure the Directories Exist
                Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);

				//Create the file path and save the image
				model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.QuestIcon);
				if (JPPImage.SaveQuestIcons(model.IconFilePath, model.Icon, false))
				{
					

					//Add the Quest
					work.QuestRepository.AddQuest(model);

                    TempData["Message"] = "Quest: " + model.Title + " successfully created and is in draft mode awaiting approval.";
					return RedirectToAction("EditQuestList");
				}
				
            }

            //ModelState was invalid, refrech the Achievements list to prevent NullRefrenceException
            AddQuestViewModel refreshModel = AddQuestViewModel.Populate();
            model.AchievementsList = refreshModel.AchievementsList;
            model.IconList = refreshModel.IconList;

            return View(model);

        }
        /*---------------------------------------------------------------------------------------*/

        /*------------------------------------Edit-----------------------------------------------*/
        /// <summary>
        /// Gives a list of current quests to edit
        /// </summary>
        /// <returns>GET: Admin/EditQuestList</returns>
        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuestList()
        {
            if (TempData["Message"] != null)
            {
                if (!String.IsNullOrWhiteSpace(TempData["Message"].ToString()))
                    ViewBag.Message = TempData["Message"].ToString();
            }
            //Create the EditQuestViewModel and populate it
            EditQuestListViewModel model = EditQuestListViewModel.Populate();
            return View(model);
        }

        /// <summary>
        /// The GET action for editing a quest
        /// </summary>
        /// <param name="id">The ID of the quest_template</param>
        /// <returns>GET: Admin/EditQuest{id}</returns>
        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuest(int id)
        {
            //Create the EditQuestViewModel and populate it
            EditQuestViewModel model = EditQuestViewModel.Populate(id);
            return View(model);
        }

        /// <summary>
        /// The POST action for editing a quest
        /// </summary>
        /// <param name="id">The ID of the quest_template</param>
        /// <param name="model">The EditQuestViewModel posted from the view</param>
        /// <returns>POST: Admin/EditQuest/{id}</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuest(int id, EditQuestViewModel model)
        {
            //Create a new Unit of Work
            UnitOfWork work = new UnitOfWork();
            model.EditorID = WebSecurity.CurrentUserId;

            //Check to make sure the Quest isn't being put back into draft mode
            JPPConstants.AchievementQuestStates currentQuestState = (JPPConstants.AchievementQuestStates)work.QuestRepository.GetQuestState(id);
            JPPConstants.AchievementQuestStates modelQuestState = (JPPConstants.AchievementQuestStates)model.State;
            if (!currentQuestState.Equals(JPPConstants.AchievementQuestStates.Draft) && modelQuestState.Equals(JPPConstants.AchievementQuestStates.Draft))
                ModelState.AddModelError(String.Empty, "This Achievement was already moved out of draft mode, it cannot be moved back");

            //Make sure the quest has associated achievements
            if (model.SelectedAchievementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No Achievements were selected for this quest");

            //Make sure the Threshold value doesn't exceed the number of selected achievements
            if (model.Threshold > model.SelectedAchievementsList.Count)
                ModelState.AddModelError("Threshold", "The Threshold value was greater than the number of achievements selected for this quest.");

            model.Threshold = model.Threshold == null || model.Threshold <= 0 ? model.SelectedAchievementsList.Count : model.Threshold;
            if (model.Icon == null && model.UploadedIcon == null)
            ModelState.AddModelError(String.Empty, "An icon must be selected for this achievement");

            if (model.Title != null)
                if (work.QuestRepository.QuestTitleExists(model.Title, id))
                    ModelState.AddModelError("", "A quest with that title already exists");

            if (ModelState.IsValid)
            {
                if (model.UploadedIcon != null)
                {
                    String filePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.NewIconUpload);
                    model.Icon = filePath.Replace("~/Content/Images/Icons/", "");
                    model.Icon = model.Icon.Replace(".png", "");
                    JPPImage.Save(Server, filePath, model.UploadedIcon.InputStream, 400, 400, true);
                }

				quest_template template = work.EntityContext.quest_template.Find(id);
				model.IconFilePath = template == null ?
					Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.QuestIcon) :
					template.icon;
				if (JPPImage.SaveQuestIcons(model.IconFilePath, model.Icon, false))
				{
					//Add the Quest to the Database
					work.QuestRepository.AdminEditQuest(id, model);

					//Return to the Admin index page
                    TempData["Message"] = "Changes to " + model.Title + " were successfully saved.";
					return RedirectToAction("EditQuestList");
				}
            }
            //ModelState was invalid, refresh the AchievementsList to prevent NullReferenceException
            AddQuestViewModel refreshModel = AddQuestViewModel.Populate();
            model.AchievementsList = refreshModel.AchievementsList;
            model.IconList = refreshModel.IconList;
            return View(model);

        }
        /*----------------------------------------------------------------------------------------*/


        public ActionResult PendingUserQuestsList()
        {
            PendingUserQuestsListViewModel model = PendingUserQuestsListViewModel.Populate();
            return View(model);
        }

        public ActionResult ManageUserQuestSubmission(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult ManageUserQuestSubmission(int? id, JPPConstants.HandleUserContent handleContent, string reason = null)
        {
            switch (handleContent)
            {
                case JPPConstants.HandleUserContent.Approve:
                    if (id == null)
                    {
                        ModelState.AddModelError(String.Empty, "Achievement of ID:" + id + " does not exist in the current version.");
                        break;
                    }
                    ApproveQuest((int)id);
                    return RedirectToAction("PendingUserQuestsList");
                case JPPConstants.HandleUserContent.Deny:
                    if (id == null)
                    {
                        ModelState.AddModelError(String.Empty, "Achievement of ID:" + id + " does not exist in the current version.");
                        break;
                    }
                    if (String.IsNullOrWhiteSpace(reason))
                    {
                        ModelState.AddModelError(String.Empty, "A reason needs to be provided for denying this quest.");
                        break;
                    }
                    DenyUserQuest((int)id, reason);
                    return RedirectToAction("PendingUserQuestsList");
                default:
                    break;
            }

            return View();
        }

        public void ApproveQuest(int questID)
        {
            UnitOfWork work = new UnitOfWork();
            work.QuestRepository.ApproveUserQuest(questID);
        }

        public void DenyUserQuest(int questID, string reason)
        {
            UnitOfWork work = new UnitOfWork();
            work.QuestRepository.DenyUserQuest(questID, reason);
        }

        #endregion

        #region Communications

        /// <summary>
        /// Manage which quests and achievements are featured on the home page
        /// </summary>
        /// <returns>GET: /Admin/ManageHighlights</returns>
        [HttpGet]
        [Authorize(Roles = JPPConstants.Roles.HandleHighlightedAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageHighlights()
        {
            ManageHighlightsViewModel model = ManageHighlightsViewModel.Populate();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = JPPConstants.Roles.HandleHighlightedAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageHighlights(ManageHighlightsViewModel model)
        {
            // TODO: implement editing
            if (model.SelectedAchievementIDs == null && model.SelectedQuestsIDs == null)
                ModelState.AddModelError(String.Empty, "At least one achievement or quest must be selected to be featured!");

            if (ModelState.IsValid)
            {
                UnitOfWork work = new UnitOfWork();
                work.SystemRepository.AdminEditHighlights(model);
                return RedirectToAction("Index");
            }

            ManageHighlightsViewModel refreshModel = ManageHighlightsViewModel.Populate();
            model.AllAchievementsList = refreshModel.AllAchievementsList;
            model.AllQuestsList = refreshModel.AllQuestsList;

            return View(model);
        }

        #endregion

        #region Site Config

        /// <summary>
        /// Configure global site settings (e.g. organization name, logo, site colors, etc)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = JPPConstants.Roles.ManageSiteSettings + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageSiteSettings()
        {
            ManageSiteSettingsViewModel model = ManageSiteSettingsViewModel.Populate();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.ManageSiteSettings + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageSiteSettings(ManageSiteSettingsViewModel model)
        {
            if (model.SiteLogo != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.SiteLogo.InputStream))
                    ModelState.AddModelError("Icon", "Image must be of type .jpg, .gif, or .png");

            if (ModelState.IsValid)
            {
                model.CreateColor = "#" + model.CreateColor;
                model.ExploreColor = "#" + model.ExploreColor;
                model.LearnColor = "#" + model.LearnColor;
                model.SocializeColor = "#" + model.SocializeColor;
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
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.DevPasswordEnabled, model.DevPasswordEnabled.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.DevPassword, model.DevPassword.ToString());

                if (!string.IsNullOrWhiteSpace(model.FacebookAppId)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppId, model.FacebookAppId);
                if (!string.IsNullOrWhiteSpace(model.FacebookAppSecret)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppSecret, model.FacebookAppSecret);
                if (!string.IsNullOrWhiteSpace(model.FacebookAppNamespace)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppNamespace, model.FacebookAppNamespace);

                TempData["Message"] = "Site settings have been successfully saved.";
                return RedirectToAction("Index"); // TODO: show success?
            }

            return View(model);
        }

        #endregion

        #region Distribute Cards

        /// <summary>
        /// Gets a list of all the users
        /// </summary>
        /// <returns>GET: Admin/ManageUserCardsList</returns>
        [Authorize(Roles = JPPConstants.Roles.DistributeCards + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageUserCardsList()
        {
            UserListViewModel model = UserListViewModel.Populate();
            return View(model);
        }

        /// <summary>
        /// Gets a list of all achievement_instance(s) associated with the userID
        /// </summary>
        /// <param name="id">The userID of the selected player</param>
        /// <returns>GET: Admin/ManageUserCards/{id}</returns>
        [Authorize(Roles = JPPConstants.Roles.DistributeCards + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult ManageUserCards(int id)
        {
            ManageUserCardsViewModel model = ManageUserCardsViewModel.Populate(id);
            return View(model);
        }

        /// <summary>
        /// Awards a card for the selected achievement_instance
        /// </summary>
        /// <param name="id">The id of the achievement_instance</param>
        /// <returns>GET: Admin/ManageUserCards/{id}</returns>
        [Authorize(Roles = JPPConstants.Roles.DistributeCards + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AwardCard(int id)
        {
            if (Request.UrlReferrer != null)
            {
                UnitOfWork work = new UnitOfWork();
                achievement_instance instance = work.AchievementRepository.InstanceExists(id);
                if (instance != null)
                    work.AchievementRepository.AwardCard(instance);
            }
            //Redirect back to the ManageUserCards for the currently selected user
            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// Revokes the card for the selected achievement_instance
        /// </summary>
        /// <param name="id">the ID of the achievement_instance</param>
        /// <returns>GET: Admin/ManageUserCards/{id}</returns>
        [Authorize(Roles = JPPConstants.Roles.DistributeCards + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult RevokeCard(int id)
        {
            if (Request.UrlReferrer != null)
            {
                UnitOfWork work = new UnitOfWork();
                achievement_instance instance = work.AchievementRepository.InstanceExists(id);
                if (instance != null)
                    work.AchievementRepository.RevokeCard(instance);
            }
            //Redirect back to the ManageUserCards for the currently selected user
            return Redirect(Request.UrlReferrer.ToString());
        }

        #endregion

        #region News Items
        [HttpGet]
        [Authorize(Roles = JPPConstants.Roles.CreateEditNews + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddNewsItem()
        {
            AddNewsItemViewModel model = new AddNewsItemViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = JPPConstants.Roles.CreateEditNews + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddNewsItem(AddNewsItemViewModel model)
        {
            model.CreatorID = WebSecurity.CurrentUserId;

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Image != null)
                    {
                        Utilities.JPPDirectory.CheckAndCreateNewsDirectory(Server);
                        model.ImageFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.News);
                        Utilities.JPPImage.Save(Server, model.ImageFilePath, model.Image.InputStream, JPPConstants.Images.NewsImageMaxSize, 200, true);
                    }

                    UnitOfWork work = new UnitOfWork();
                    work.SystemRepository.AdminAddNewsItem(model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "News Item Failed to Add: " + e.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = JPPConstants.Roles.CreateEditNews + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditNewsItem(int id)
        {
            EditNewsItemViewModel model = EditNewsItemViewModel.Populate(id);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = JPPConstants.Roles.CreateEditNews + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditNewsItem(int id, EditNewsItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Image != null)
                    {
                        Utilities.JPPDirectory.CheckAndCreateNewsDirectory(Server);
                        model.ImageFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.News);
                        Utilities.JPPImage.Save(Server, model.ImageFilePath, model.Image.InputStream, JPPConstants.Images.NewsImageMaxSize, 0, true);
                    }

                    UnitOfWork work = new UnitOfWork();
                    work.SystemRepository.AdminEditNewsItem(id, model);
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", "Edit Failed to Save: " + e.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = JPPConstants.Roles.CreateEditNews + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditNewsItemList()
        {
            EditNewsItemListViewModel model = EditNewsItemListViewModel.Populate();
            return View(model);
        }

        #endregion

        public ActionResult RevokeAchievement(int id)
        {
            UnitOfWork work = new UnitOfWork();
            //TODO: FIX PARAMETERS 
            work.AchievementRepository.RevokeAchievement(id,"reason",true,1);
            return RedirectToAction("Index");
        }

        public String DeleteUser(String username)
        {
            try
            {
                if (Roles.GetRolesForUser(username).Count() > 0)
                {
                    Roles.RemoveUserFromRoles(username, Roles.GetRolesForUser(username));
                }
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(username);
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(username, true);
                return "Success";

            }
            catch(Exception e)
            {
                return e.Message;
            }

            
        }
        [AllowAnonymous]
        public ActionResult FixQRCodes()
        {
            UnitOfWork work = new UnitOfWork();
            List<user> users = work.UserRepository.GetAllUsers();

            foreach (user user in users)
            {
                Utilities.JPPDirectory.CheckAndCreateUserDirectory(user.id, Server);
                String qrString = Request.Url.GetLeftPart(UriPartial.Authority) + "/Players/" + user.id;
                //Create the file path and save the image
                String qrfilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.UserQRCode, user.id);
                String qrfileMinusPath = qrfilePath.Replace("~/Content/Images/Users/" + user.id.ToString() + "/UserQRCodes/", "");
                //"/Users/" + userID.ToString() + "/ProfilePictures/" + fileName + ".png";
                if (JPPImage.SavePlayerQRCodes(qrfilePath, qrfileMinusPath, qrString))
                {
                    user.qr_image = qrfilePath;
                }
            }
            work.SaveChanges();

            return RedirectToAction("Index");
        }

        public String TestValidate()
        {
           return Request.UserAgent;
        }
    }
        
}
