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
    public class PlayersController : Controller
    {
        /// <summary>
        /// The list of all plages
        /// </summary>
        /// <returns>GET: /Players</returns>
        public ActionResult Index()
        {
			PlayersListViewModel model = PlayersListViewModel.Populate();
            return View(model);
        }

		/// <summary>
		/// An individual player's profile.
		/// If this is your own profile, it will be editable.
		/// The routing table hides this action name
		/// </summary>
		/// <param name="id">The player's id</param>
		/// <returns>GET: /Players/{id}</returns>
		public new ActionResult Profile(int id)
		{
			ProfileViewModel model = ProfileViewModel.Populate(id);
			if (model == null)
				return RedirectToAction("Index");
				// TODO: Throw a 404

			return View(model);
		}

		/// <summary>
		/// The login page.
		/// TODO: Determine if this will be replaced by a pop-up login box
		/// </summary>
		/// <returns>GET: /Players/Login</returns>
		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		/// <summary>
		/// The login page post-back.
		/// TODO: Make this return to the previous url (once the pop-up box is in)
		/// </summary>
		/// <param name="model">The login information</param>
		/// <param name="returnUrl">The url for redirection</param>
		/// <returns>POST: /Players/Login</returns>
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid &&
				WebSecurity.Login(model.Username, model.Password, model.RememberMe))
			{
				if (Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				else
				{
					return RedirectToAction("Index", "Home");
				}
			}

			// Failed to log in
			ModelState.AddModelError("", "The username or password is incorrect.");
			return View(model);
		}

		/// <summary>
		/// Logs a user out
		/// </summary>
		/// <returns>POST: /Players/Logout</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Logout()
		{
			WebSecurity.Logout();
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// The self-registration page for new users
		/// </summary>
		/// <returns>GET: /Players/Register</returns>
		[AllowAnonymous]
		public ActionResult Register()
		{
			ViewBag.EmailSent = false;
			return View();
		}

		/// <summary>
		/// The post-back for new user registration
		/// </summary>
		/// <param name="model">The registration information</param>
		/// <returns>POST: /Players/Register</returns>
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Double check password and email confirmations
				bool confirmError = false;
				if (model.Email != model.ConfirmEmail)
				{
					ModelState.AddModelError("", "The email and confirmation email do not match.");
					confirmError = true;
				}
				if (model.Password != model.ConfirmPassword)
				{
					ModelState.AddModelError("", "The password and confirmation password do not match.");
					confirmError = true;
				}

				if (!confirmError)
				{
					try
					{
						// Attempt to create the user
						String confirmationToken = WebSecurity.CreateUserAndAccount(
							model.Username,
							model.Password,
							new
							{
								first_name = model.FirstName,
								middle_name = model.MiddleName,
								last_name = model.LastName,
								is_player = false,
								created_date = DateTime.Now,
								status = 1, // TODO: Create status code constants and update this
								first_login = true,
								email = model.Email,
								last_login_date = DateTime.Now,
								display_name = model.DisplayName,
								privacy_settings = 2, // TODO: Create privacy setting constants and update this
								has_agreed_to_tos = false,
								modified_date = DateTime.Now
							},
							true); // The user account must be confirmed

						// Send the confirmation email
						String confirmLink = "http://" + Request.Url.Authority + "/Players/Confirm?token=" + confirmationToken;
						Email.Send(model.Email,
							"Just Press Play Registration Confirmation",
							"Hello " + model.FirstName + ",\n\n" +
							"Here is your registration confirmation link:\n\n" +
							"<a href='" + confirmLink + "'>" + confirmLink + "</a>",
							true
						);

						// All done
						ViewBag.EmailSent = true;
						return View();
					}
					catch (MembershipCreateUserException e)
					{
						ModelState.AddModelError("", GetMembershipCreationErrorMessage(e.StatusCode));
					}
					catch (Exception e)
					{
						ModelState.AddModelError("", e.Message);
					}
				}
			}

			// Getting this far means an error has occurred, so redisplay the page
			ViewBag.EmailSent = false;
			return View(model);
		}

		/// <summary>
		/// Confirms the user's registration
		/// </summary>
		/// <param name="token">The confirmation token</param>
		/// <returns>GET: /Players/Confirm?token=...</returns>
		[AllowAnonymous]
		public ActionResult Confirm(String token)
		{
			if (String.IsNullOrWhiteSpace(token))
				return View();

			// Attempt to validate
			if (WebSecurity.ConfirmAccount(token))
			{
				ViewBag.Confirmed = true;
			}
			else
			{
				ViewBag.InvalidToken = true;
			}

			return View();
		}

		/// <summary>
		/// Allows a user to reset their password by
		/// emailing them a reset password token link
		/// </summary>
		/// <returns>GET: /Players/ForgotPassword</returns>
		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			ViewBag.EmailSent = false;
			return View();
		}

		/// <summary>
		/// Post-back for forgotten passwords
		/// </summary>
		/// <param name="model">The model with the username</param>
		/// <returns>POST: /Players/ForgotPassword</returns>
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Get the user's email
				UnitOfWork db = new UnitOfWork();
				user theUser = db.EntityContext.user.FirstOrDefault(u => u.email == model.Email);
				if (theUser != null)
				{
					// Attempt to create a password reset token
					String passwordToken = WebSecurity.GeneratePasswordResetToken(theUser.username);
					String link = "http://" + Request.Url.Authority + "/Players/ResetPassword?token=" + passwordToken;

					// Email to the user
					Email.Send(
						theUser.email,
						"Just Press Play - Password Reset Request",
						"Hello " + theUser.username + ",<br/><br/>\n\nHere is your password reset link.  It is valid for 24 hours.<br/><br/>\n\n" +
						"<a href='" + link + "'>" + link + "</a><br/><br/>\n\n" + 
						"If you did not request this password reset, you can ignore this email.",
						true);

					// All done
					ViewBag.EmailSent = true;
					return View();
				}
			}

			// Something was wrong
			ModelState.AddModelError("", "The email you provided is invalid.");
			ViewBag.EmailSent = false;
			return View(model);
		}

		/// <summary>
		/// Allows a user to reset their password if they have a valid token
		/// </summary>
		/// <param name="token">The password reset token</param>
		/// <returns>GET: /Players/ResetPassword?token=...</returns>
		[AllowAnonymous]
		public ActionResult ResetPassword(String token)
		{
			// Send the token to the page
			ResetPasswordViewModel model = new ResetPasswordViewModel();
			model.Token = token;
			ViewBag.PasswordReset = false;
			return View(model);
		}

		/// <summary>
		/// Post-back for resetting a user's password
		/// </summary>
		/// <param name="model">The model with the new password and token</param>
		/// <returns>POST: /Players/ResetPassword</returns>
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult ResetPassword(ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Do the password match?
				if (model.Password != model.ConfirmPassword)
				{
					// No, so error and hit the view at the bottom of the method
					ModelState.AddModelError("", "Password and confirmation do not match.");
				}
				else
				{
					// Yes, they match
					bool passwordReset = WebSecurity.ResetPassword(model.Token, model.Password);
					if (passwordReset)
					{
						// Success
						ViewBag.PasswordReset = true;
						return View();
					}
					else
					{
						// Problem with the token
						ModelState.AddModelError("", "The token you have provided is invalid or has expired.");
					}
				}
			}

			// Problem
			ViewBag.PasswordReset = false;
			return View(model);
		}

		/// <summary>
		/// Allows the logged in user to add a friend
		/// </summary>
		/// <param name="id">The id of the user to friend</param>
		/// <returns>POST: /Players/AddFriend</returns>
		[HttpPost]
		public Boolean AddFriend(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.UserRepository.AddFriend(id);
		}

		/// <summary>
		/// Allows the logged in user to accept a friend request
		/// </summary>
		/// <param name="id">The id of the user whose request should be accepted</param>
		/// <returns>POST: /Players/AcceptFriendRequest</returns>
		[HttpPost]
		public Boolean AcceptFriendRequest(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.UserRepository.AcceptFriendRequest(id);
		}

		/// <summary>
		/// Allows the logged in user to decline a friend request
		/// </summary>
		/// <param name="id">The id of the user whose request should be declined</param>
		/// <returns>POST: /Players/DeclineFriendRequest</returns>
		[HttpPost]
		public Boolean DeclineFriendRequest(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.UserRepository.DeclineFriendRequest(id);
		}

		/// <summary>
		/// Allows the logged in user to ignore a friend request
		/// </summary>
		/// <param name="id">The id of the user whose request should be ignored</param>
		/// <returns>POST: /Players/IgnoreFriendRequest</returns>
		[HttpPost]
		public Boolean IgnoreFriendRequest(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.UserRepository.IgnoreFriendRequest(id);
		}

		/// <summary>
		/// Allows the logged in user to remove a friend
		/// </summary>
		/// <param name="id">The id of the user whose friendship should be removed</param>
		/// <returns>POST: /Players/RemoveFriend</returns>
		[HttpPost]
		public Boolean RemoveFriend(int id)
		{
			UnitOfWork work = new UnitOfWork();
			return work.UserRepository.RemoveFriend(id);
		}

		#region Helper Methods
		/// <summary>
		/// Returns an error message corresponding to a specific membership
		/// creation status error code.
		/// 
		/// This is based on the method "ErrorCodeToString" from the default
		/// AccountController.cs in new ASP MVC 4 projects
		/// </summary>
		/// <param name="statusCode">The error code</param>
		/// <returns>An error message</returns>
		private String GetMembershipCreationErrorMessage(MembershipCreateStatus statusCode)
		{
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch (statusCode)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "User name already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
		#endregion
	}
}
