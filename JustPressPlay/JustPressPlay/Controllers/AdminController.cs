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
		/// Allows an admin to edit a user
		/// </summary>
		/// <returns>GET: /Admin/EditUser/{id}</returns>
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUser(int id = 0)
		{
			if (id <= 0)
				return RedirectToAction("Index");

			EditUserViewModel model = EditUserViewModel.Populate(id);
			return View(model);
		}

		/// <summary>
		/// Post-back for editting a user
		/// </summary>
		/// <param name="user">The user being edited</param>
		/// <returns></returns>
		[Authorize(Roles = JPPConstants.Roles.EditUsers + "," + JPPConstants.Roles.FullAdmin)]
		public ActionResult EditUser(EditUserViewModel model)
		{
			// Valid?
			if (ModelState.IsValid)
			{
				// Put the data back into the database
				UnitOfWork work = new UnitOfWork();
				user user = work.UserRepository.GetUser(model.ID);
				if( user != null )
				{
					user.display_name = model.DisplayName;
					user.email = model.Email;
					user.is_player = model.IsPlayer;
					user.first_name = model.FirstName;
					user.middle_name = model.MiddleName;
					user.last_name = model.LastName;
					user.six_word_bio =
						model.SixWordBio1.Replace(" ", "") + " " +
						model.SixWordBio2.Replace(" ", "") + " " +
						model.SixWordBio3.Replace(" ", "") + " " +
						model.SixWordBio4.Replace(" ", "") + " " +
						model.SixWordBio5.Replace(" ", "") + " " +
						model.SixWordBio6.Replace(" ", "");
					user.full_bio = model.FullBio;

					// Save the changes, then add the user to the roles
					work.SaveChanges();
					// TODO: Fix this to account for the removal of roles
					//Roles.AddUserToRoles(user.username, model.Roles);
				}


			}

			// Problem, redisplay
			return View(model);
		}
    }
}
