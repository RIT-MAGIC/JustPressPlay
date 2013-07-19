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
			EditUserListViewModel model = EditUserListViewModel.Populate();
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

        public ActionResult AddAchievement()
        {
            AddAchievementViewModel model = AddAchievementViewModel.Populate();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddAchievement(AddAchievementViewModel model)
        {
            //Add the Logged In User(Creator) ID to the Model
            model.CreatorID = WebSecurity.CurrentUserId;

            //Make sure the requirements list isn't empty
            int i = 0;
            foreach (string requirement in model.RequirementsList)
            {
                if (!String.IsNullOrWhiteSpace(requirement))
                    i++;
            }

            //Check to make sure the model is valid and the image uploaded is an actual image
            if (ModelState.IsValid && Utilities.JPPImage.FileIsWebFriendlyImage(model.Icon.InputStream) && i > 0)
            {
                //Make Sure the Directories Exist
                Utilities.JPPDirectory.CheckAndCreateAchievementAndQuestDirectory(Server);
                //Create the file path and save the image
                model.IconFilePath = Utilities.JPPDirectory.CreateFilePath(Server, JPPDirectory.ImageTypes.AchievementIcon);
                Utilities.JPPImage.Save(model.IconFilePath, model.Icon.InputStream, 109, true);

                //Create a new Unit of Work
                UnitOfWork work = new UnitOfWork();

                //Check the achievement type and modify the model accordingly
                //Only scans get caretakers| Only thresholds have a threshold number and parent
                //Only user submissions have content types | Only system achievements have system trigger types
                //Thresholds can't be repeatable | Only repeatable achievements have a delay, which must be at least 1
                #region Modify Model based on achievement type

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

                return RedirectToAction("Index");
            }
            else
            {
                AddAchievementViewModel refreshModel = AddAchievementViewModel.Populate();
                model.PotentialCaretakersList = refreshModel.PotentialCaretakersList;
                model.ParentAchievements = refreshModel.ParentAchievements;                
            }

            return View(model);
        }
    }
}
