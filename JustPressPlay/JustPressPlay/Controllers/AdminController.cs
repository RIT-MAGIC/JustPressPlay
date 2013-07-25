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
        const int SiteLogoMaxWidth = 200; // TODO: Pick actual value

        /// <summary>
        /// Admin home page
        /// </summary>
        /// <returns>GET: /Admin</returns>
        public ActionResult Index()
        {
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
							privacy_settings = (int)JPPConstants.PrivacySettings.JustPressPlayOnly,
							has_agreed_to_tos = false,
							creator_id = WebSecurity.CurrentUserId
						}, 
						false);

					ViewBag.Message = "User " + model.Username + " successfully created.";
					return View();
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
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUserList()
		{
			UserListViewModel model = UserListViewModel.Populate();
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
			// Valid?
			if (ModelState.IsValid)
			{
                //TODO: ADD PROFILE IMAGE STUFF
				// Put the data back into the database
				UnitOfWork work = new UnitOfWork();
				user user = work.UserRepository.GetUser(model.ID);
				if (user != null)
				{
					user.display_name = model.DisplayName;
					user.email = model.Email;
					user.is_player = model.IsPlayer;
					user.first_name = model.FirstName;
					user.middle_name = model.MiddleName;
					user.last_name = model.LastName;
					user.six_word_bio =
						model.SixWordBio1 == null ? "" : model.SixWordBio1.Replace(" ", "") + " " +
						model.SixWordBio2 == null ? "" : model.SixWordBio2.Replace(" ", "") + " " +
						model.SixWordBio3 == null ? "" : model.SixWordBio3.Replace(" ", "") + " " +
						model.SixWordBio4 == null ? "" : model.SixWordBio4.Replace(" ", "") + " " +
						model.SixWordBio5 == null ? "" : model.SixWordBio5.Replace(" ", "") + " " +
						model.SixWordBio6 == null ? "" : model.SixWordBio6.Replace(" ", "");
					user.full_bio = model.FullBio;
					user.modified_date = DateTime.Now;
					
					// Save the changes, then add the user to the roles
					work.SaveChanges();
					JPPConstants.Roles.UpdateUserRoles(user.username, model.Roles);

					// Success
					return RedirectToAction("EditUserList");
				}
				else
				{
					ModelState.AddModelError("", "The specified user could not be found");
				}
			}

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

            //Make sure the requirements list isn't empty
            model.RequirementsList = model.RequirementsList.Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            if (model.RequirementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No requirements were specified for this achievement");

            //Check if there is an image upload and if there is, make sure it's actually an image
            if (model.Icon != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.Icon.InputStream))
                    ModelState.AddModelError("Icon", "File not of type .jpg,.gif, or .png");

            //Check to make sure the model is valid and the image uploaded is an actual image
            if (ModelState.IsValid)
            {
                //Make Sure the Directories Exist
                Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                //Create the file path and save the image
                model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.AchievementIcon);
                Utilities.JPPImage.Save(Server, model.IconFilePath, model.Icon.InputStream, 109, true);

                //Create a new Unit of Work
                UnitOfWork work = new UnitOfWork();

                #region Modify Model based on achievement type

                //Only scans get caretakers| Only thresholds have a threshold number and parent
                //Only user submissions have content types | Only system achievements have system trigger types
                //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1

                JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
                model.IsRepeatable = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? false : true;
                model.SelectedCaretakersList = achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? model.SelectedCaretakersList : null;
                model.Threshold = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.Threshold : null;
                model.ParentID = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.ParentID : null;
                model.ContentType = achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? model.ContentType : null;
                model.SystemTriggerType = achievementType.Equals(JPPConstants.AchievementTypes.System) ? model.SystemTriggerType : null;
                model.RepeatDelayDays = model.RepeatDelayDays >= 1 ? model.RepeatDelayDays : 1;
                model.RepeatDelayDays = model.IsRepeatable ? model.RepeatDelayDays : null;

                #endregion

                //Add the Achievement to the Database
                work.AchievementRepository.AdminAddAchievement(model);

                //Return to the Admin index page
                return RedirectToAction("Index");
            }

            //ModelState was not valid, refresh the ViewModel
            AddAchievementViewModel refreshModel = AddAchievementViewModel.Populate();
            model.PotentialCaretakersList = refreshModel.PotentialCaretakersList;
            model.ParentAchievements = refreshModel.ParentAchievements;
            for (int i = 0; i < 7; i++)
                model.RequirementsList.Add("");

            //Return the user to the AddAchievement view with the current model
            return View(model);
        }

        /// <summary>
        /// Gets the list of achievements to edit
        /// </summary>
        /// <returns>GET: /admin/editachievementlist</returns>
        [Authorize(Roles = JPPConstants.Roles.EditAchievements + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditAchievementList()
        {
            EditAchievementListViewModel model = EditAchievementListViewModel.Populate();
            return View(model);
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

            //Create a new Unit of Work
            UnitOfWork work = new UnitOfWork();

            JPPConstants.AchievementQuestStates currentAchievementState = (JPPConstants.AchievementQuestStates)work.AchievementRepository.GetAchievementState(id);
            JPPConstants.AchievementQuestStates modelAchievementState = (JPPConstants.AchievementQuestStates)model.State;

            if (!currentAchievementState.Equals(JPPConstants.AchievementQuestStates.Draft) && modelAchievementState.Equals(JPPConstants.AchievementQuestStates.Draft))
                ModelState.AddModelError(String.Empty, "This Achievement was already moved out of draft mode, it cannot be moved back");

            //Make sure the requirements list isn't empty
            model.RequirementsList = model.RequirementsList.Where(s => !String.IsNullOrWhiteSpace(s)).ToList();
            if (model.RequirementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No requirements were specified for this achievement");

            //Check if there is an image upload and if there is, make sure it's actually an image
            if (model.Icon != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.Icon.InputStream))
                    ModelState.AddModelError("Icon", "File not of type .jpg,.gif, or .png");


            //Check to make sure the model is valid
            if (ModelState.IsValid)
            {
                if (model.Icon != null)
                {
                    //Make Sure the Directories Exist
                    Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                    //Create the file path and save the image
                    model.IconFilePath = Utilities.JPPDirectory.CreateFilePath( JPPDirectory.ImageTypes.AchievementIcon);
                    Utilities.JPPImage.Save(Server, model.IconFilePath, model.Icon.InputStream, 109, true);
                }

               
                              
                #region Modify Model based on achievement type

                //Only scans get caretakers| Only thresholds have a threshold number and parent
                //Only user submissions have content types | Only system achievements have system trigger types
                //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1

                JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
                model.IsRepeatable = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? false : model.IsRepeatable;
                model.SelectedCaretakersList = achievementType.Equals(JPPConstants.AchievementTypes.Scan) ? model.SelectedCaretakersList : null;
                model.Threshold = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.Threshold : null;
                model.ParentID = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) ? model.ParentID : null;
                model.ContentType = achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? model.ContentType : null;
                model.SystemTriggerType = achievementType.Equals(JPPConstants.AchievementTypes.System) ? model.SystemTriggerType : null;
                model.RepeatDelayDays = model.RepeatDelayDays >= 1 ? model.RepeatDelayDays : 1;
                model.RepeatDelayDays = model.IsRepeatable ? model.RepeatDelayDays : null;

                #endregion

                //Add the Achievement to the Database
                work.AchievementRepository.AdminEditAchievement(id, model);

                //Return to the Admin index page
                return RedirectToAction("Index");
            }

            //Modelstate was not valid, refresh the ViewModel
            AddAchievementViewModel refreshModel = AddAchievementViewModel.Populate();
            model.PotentialCaretakersList = refreshModel.PotentialCaretakersList;
            model.ParentAchievements = refreshModel.ParentAchievements;
            for (int i = 0; i < 7; i++)
                model.RequirementsList.Add("");

            //Return the user to the EditAchievement view with the current model
            return View(model);
        }

		/// <summary>
		/// Allows an admin to assign individual (non-global) achievements to users
		/// </summary>
		/// <returns>GET: /Admin/AssignIndividualAchievement</returns>
		[Authorize(Roles = JPPConstants.Roles.AssignIndividualAchievements + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult AssignIndividualAchievement()
		{
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

				// Attempt to assign the achievement
				try
				{
					work.AchievementRepository.AssignAchievement(model.UserID, model.AchievementID, WebSecurity.CurrentUserId);
					return RedirectToAction("Index");
				}
				catch (Exception e)
				{
					ModelState.AddModelError("", e.Message);
				}
			}

			// Problem, return the model
			return View(model);
		}

        #endregion

        #region Add/Edit Quests
        //TODO: ADD FILTERS AND COMMENTS [Ben]
        public ActionResult AddQuest()
        {
            AddQuestViewModel model = AddQuestViewModel.Populate();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = JPPConstants.Roles.CreateQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult AddQuest(AddQuestViewModel model)
        {
            model.CreatorID = WebSecurity.CurrentUserId;

            if (model.SelectedAchievementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No Achievements were selected for this quest");

            if (model.Threshold > model.SelectedAchievementsList.Count)
                ModelState.AddModelError("Threshold", "The Threshold value was greater than the number of achievements selected for this quest.");

            if (model != null)
                if(!Utilities.JPPImage.FileIsWebFriendlyImage(model.Icon.InputStream))
                    ModelState.AddModelError("Icon", "Image must be of type .jpg, .gif, or .png");

            if (ModelState.IsValid)
            {
                //Make Sure the Directories Exist
                Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                //Create the file path and save the image
                model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.QuestIcon);
                Utilities.JPPImage.Save(Server, model.IconFilePath, model.Icon.InputStream, 109, true);

                //Create a new Unit of Work
                UnitOfWork work = new UnitOfWork();

                work.QuestRepository.AdminAddQuest(model);

                return RedirectToAction("Index");
            }

            AddQuestViewModel refreshModel = AddQuestViewModel.Populate();
            model.AchievementsList = refreshModel.AchievementsList;

            return View(model);

        }

        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuestList()
        {
            EditQuestListViewModel model = EditQuestListViewModel.Populate();
            return View(model);
        }


        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuest(int id)
        {
            EditQuestViewModel model = EditQuestViewModel.Populate(id);
            return View(model);
        }

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

            if (model.SelectedAchievementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No Achievements were selected for this quest");

            if (model.Threshold > model.SelectedAchievementsList.Count)
                ModelState.AddModelError("Threshold", "The Threshold value was greater than the number of achievements selected for this quest.");

            if (model.Icon != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.Icon.InputStream))
                    ModelState.AddModelError("Icon", "Image must be of type .jpg, .gif, or .png");

            if (ModelState.IsValid)
            {
                if (model.Icon != null)
                {
                    //Make Sure the Directories Exist
                    Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                    //Create the file path and save the image
                    model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.QuestIcon);
                    Utilities.JPPImage.Save(Server, model.IconFilePath, model.Icon.InputStream, 109, true);
                }

                work.QuestRepository.AdminEditQuest(id, model);

                return RedirectToAction("Index");
            }

            AddQuestViewModel refreshModel = AddQuestViewModel.Populate();
            model.AchievementsList = refreshModel.AchievementsList;

            return View(model);

        }
        #endregion

        #region Communications

        /// <summary>
        /// Manage which quests and achievements are featured on the home page
        /// </summary>
        /// <returns>GET: /Admin/ManageHighlights</returns>
        [HttpGet]
        public ActionResult ManageHighlights()
        {
            ManageHighlightsViewModel model = ManageHighlightsViewModel.Populate();
            return View(model);
        }

        [HttpPost]
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
        public ActionResult ManageSiteSettings()
        {
            ManageSiteSettingsViewModel model = ManageSiteSettingsViewModel.Populate();
            return View(model);
        }

        [HttpPost]
        public ActionResult ManageSiteSettings(ManageSiteSettingsViewModel model)
        {
            if (model.SiteLogo != null)
                if (!Utilities.JPPImage.FileIsWebFriendlyImage(model.SiteLogo.InputStream))
                    ModelState.AddModelError("Icon", "Image must be of type .jpg, .gif, or .png");

            if (ModelState.IsValid)
            {
                // TODO: Apply changes in site settings
                ModelState.AddModelError(string.Empty, "Model valid! DB entry not yet implemented, sorry.");
                return View(model);

                // For reference: Actual handling; will throw exception until implemented
                //model.SiteLogoFilePath = Utilities.JPPDirectory.CreateFilePath(Server, JPPDirectory.ImageTypes.QuestIcon);
                //Utilities.JPPImage.Save(model.SiteLogoFilePath, model.SiteLogo.InputStream, SiteLogoMaxWidth, false);
                //UnitOfWork work = new UnitOfWork();
                //work.SystemRepository.AdminEditSiteSettings(model);
                //return RedirectToAction("Index");
            }

            return View(model);
        }

        #endregion

        public ActionResult ManageUserCardsList()
        {
            UserListViewModel model = UserListViewModel.Populate();
            return View(model);
        }

        public ActionResult ManageUserCards(int id)
        {
            ManageUserCardsViewModel model = ManageUserCardsViewModel.Populate(id);
            return View(model);
        }

        public ActionResult AwardCard(int id)
        {
            if (Request.UrlReferrer != null)
            {
                UnitOfWork work = new UnitOfWork();
                achievement_instance instance = work.AchievementRepository.InstanceExists(id);
                if (instance != null)
                    work.AchievementRepository.AwardCard(instance);
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult RevokeCard(int id)
        {
            if (Request.UrlReferrer != null)
            {
                UnitOfWork work = new UnitOfWork();
                achievement_instance instance = work.AchievementRepository.InstanceExists(id);
                if (instance != null)
                    work.AchievementRepository.RevokeCard(instance);
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        [HttpGet]
        public ActionResult AddNewsItem()
        {
            AddNewsItemViewModel model = new AddNewsItemViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddNewsItem(AddNewsItemViewModel model)
        {
            // TODO: add entry to db
            return View(model);
        }
    }
        
}
