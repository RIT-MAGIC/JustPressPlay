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
							privacy_settings = (int)JPPConstants.PrivacySettings.FriendsOnly,
							has_agreed_to_tos = false,
							creator_id = WebSecurity.CurrentUserId,
							communication_settings = (int)JPPConstants.CommunicationSettings.All,
							notification_settings = 0
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

        //TODO: (BEN) CHECK SYSTEM ACHIEVEMENTS TO PREVENT REDUNDANCIES (Make sure there is only one of each type)
        #region Add/Edit Achievements
        //TODO: ONLY SCANS CAN BE REPEATABLE
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

            //Create a new Unit of Work
            UnitOfWork work = new UnitOfWork();

            #region Modify Model based on achievement type

            //Only scans get caretakers| Only thresholds have a threshold number and parent
            //Only user submissions have content types | Only system achievements have system trigger types
            //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1

            JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
            model.IsRepeatable = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) || achievementType.Equals(JPPConstants.AchievementTypes.System) || achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? false : true;
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

            //Check to make sure the model is valid and the image uploaded is an actual image
            if (ModelState.IsValid)
            {
                //Make Sure the Directories Exist
                Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                //Create the file path and save the image
                model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.AchievementIcon);
                Utilities.JPPImage.Save(Server, model.IconFilePath, model.Icon.InputStream, 109, true);          

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

            #region Modify Model based on achievement type

            //Only scans get caretakers| Only thresholds have a threshold number and parent
            //Only user submissions have content types | Only system achievements have system trigger types
            //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1

            JPPConstants.AchievementTypes achievementType = (JPPConstants.AchievementTypes)model.Type;
            model.IsRepeatable = achievementType.Equals(JPPConstants.AchievementTypes.Threshold) || achievementType.Equals(JPPConstants.AchievementTypes.System) || achievementType.Equals(JPPConstants.AchievementTypes.UserSubmission) ? false : true;
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
                int achievementType = work.AchievementRepository.GetAchievementType(model.AchievementID);
				// Attempt to assign the achievement
				try
				{
                    switch (achievementType)
                    {
                        case (int)JPPConstants.AchievementTypes.Scan:
                            work.AchievementRepository.AssignScanAchievement(model.UserID, model.AchievementID, WebSecurity.CurrentUserId, DateTime.Now);
                            break;
                        case (int)JPPConstants.AchievementTypes.System:
                            work.AchievementRepository.AssignAchievement(model.UserID, model.AchievementID, WebSecurity.CurrentUserId);
                            break;
                        case (int)JPPConstants.AchievementTypes.Threshold:
                            work.AchievementRepository.AssignAchievement(model.UserID, model.AchievementID, WebSecurity.CurrentUserId);
                            break;
                        default:
                            break;
                          
                    }
					return RedirectToAction("Index");
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

        #endregion

        #region Add/Edit Quests

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
            //Add the current logged in user to the model (They are the ones creating it)
            model.CreatorID = WebSecurity.CurrentUserId;

            //Make sure the quest has associated achievements
            if (model.SelectedAchievementsList == null || model.SelectedAchievementsList.Count <= 0)
                ModelState.AddModelError(String.Empty, "No Achievements were selected for this quest");

            //Make sure the Threshold value doesn't exceed the number of selected achievements
			if (model.SelectedAchievementsList == null || model.Threshold > model.SelectedAchievementsList.Count)
                ModelState.AddModelError("Threshold", "The Threshold value was greater than the number of achievements selected for this quest.");

            model.Threshold = model.Threshold == null || model.Threshold <= 0 ? model.SelectedAchievementsList.Count : model.Threshold;

            //Make sure the Icon image is actually of type .jpg/.gif/.png
            if (model.Icon != null)
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

                //Add the Quest
                work.QuestRepository.AdminAddQuest(model);

                return RedirectToAction("Index");
            }

            //ModelState was invalid, refrech the Achievements list to prevent NullRefrenceException
            AddQuestViewModel refreshModel = AddQuestViewModel.Populate();
            model.AchievementsList = refreshModel.AchievementsList;

            return View(model);

        }

        /// <summary>
        /// Gives a list of current quests to edit
        /// </summary>
        /// <returns>GET: Admin/EditQuestList</returns>
        [Authorize(Roles = JPPConstants.Roles.EditQuests + "," + JPPConstants.Roles.FullAdmin)]
        public ActionResult EditQuestList()
        {
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

            //Make sure the Icon image is actually of type .jpg/.gif/.png
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

                //Add the edits to the database
                work.QuestRepository.AdminEditQuest(id, model);

                return RedirectToAction("Index");
            }
            //ModelState was invalid, refresh the AchievementsList to prevent NullReferenceException
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
                if (model.SiteLogo != null)
                {
                    Utilities.JPPDirectory.CheckAndCreateSiteContentDirectory(Server);
                    model.SiteLogoFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.SiteContent);
                    Utilities.JPPImage.Save(Server, model.SiteLogoFilePath, model.SiteLogo.InputStream, JPPConstants.SiteLogoMaxSideSize, false);
                }

                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorNavBar, model.NavBarColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorCreate, model.CreateColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorExplore, model.ExploreColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorLearn, model.LearnColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorSocialize, model.SocializeColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.ColorQuest, model.QuestColor);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SchoolName, model.OrganizationName);
                if (model.SiteLogoFilePath != null) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SchoolLogo, model.SiteLogoFilePath);
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.MaxPointsPerAchievement, model.MaximumPointsPerAchievement.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.CardDistributionEnabled, model.EnableCardDistribution.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.SelfRegistrationEnabled, model.AllowSelfRegistration.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.UserGeneratedQuestsEnabled, model.AllowUserGeneratedQuests.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.CommentsEnabled, model.AllowComments.ToString());
                JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled, model.EnableFacebookIntegration.ToString());
                if (!string.IsNullOrWhiteSpace(model.FacebookAppId)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppId, model.FacebookAppId);
                if (!string.IsNullOrWhiteSpace(model.FacebookAppSecret)) JPPConstants.SiteSettings.SetValue(JPPConstants.SiteSettings.FacebookAppSecret, model.FacebookAppSecret);

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
                if (model.Image != null)
                {
                    model.ImageFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.News);
                    Utilities.JPPImage.Save(Server, model.ImageFilePath, model.Image.InputStream, JPPConstants.NewsItemImageMaxSideSize, true);
                }

                UnitOfWork work = new UnitOfWork();
                work.SystemRepository.AdminAddNewsItem(model);
                return RedirectToAction("Index");
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
                if (model.Image != null)
                {
                    model.ImageFilePath = Utilities.JPPDirectory.CreateFilePath(JPPDirectory.ImageTypes.News);
                    Utilities.JPPImage.Save(Server, model.ImageFilePath, model.Image.InputStream, JPPConstants.NewsItemImageMaxSideSize, true);
                }

                UnitOfWork work = new UnitOfWork();
                work.SystemRepository.AdminEditNewsItem(id, model);
                return RedirectToAction("Index");
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
            work.AchievementRepository.RevokeAchievement(id);
            return RedirectToAction("Index");
        }

        public String TestValidate()
        {
           return Request.UserAgent;
        }
    }
        
}
