using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Used for logging in to the site
	/// </summary>
	public class LoginViewModel
	{
		[Required]
		[Display(Name = "Username")]
		public String Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public String Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	/// <summary>
	/// Used for forgotten passwords.
	/// </summary>
	public class ForgotPasswordViewModel
	{
		[Required]
		[Display(Name = "Email")]
		public String Email { get; set; }
	}

	/// <summary>
	/// Used for resetting password
	/// </summary>
	public class ResetPasswordViewModel
	{
		[Required]
		public String Token { get; set; }

		[Required]
		[StringLength(255, ErrorMessage = "The {0} must contain at least {2} characters.", MinimumLength = 8)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public String Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public String ConfirmPassword { get; set; }
	}

	/// <summary>
	/// Allows users to self-register
	/// </summary>
	public class RegisterViewModel
	{
		[Required]
		[Display(Name = "Username", Description = "Used to log into the site")]
		public String Username { get; set; }

		[Required]
		[Display(Name = "Display name", Description = "Other players will see this along with your real name")]
		[StringLength(64, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 3)]
		public String DisplayName { get; set; }

		[Required]
		[Display(Name = "First name", Description = "Your first name")]
		public String FirstName { get; set; }

		[Display(Name = "Middle name", Description = "Your middle name")]
		public String MiddleName { get; set; }

		[Required]
		[Display(Name = "Last name", Description = "Your last name")]
		public String LastName { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email address", Description = "Your email address where the confirmation email will be sent")]
		public String Email { get; set; }
		
		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Confirm email address")]
		[Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
		public String ConfirmEmail { get; set; }

		[Required]
		[StringLength(255, ErrorMessage = "The {0} must contain at least {2} characters.", MinimumLength = 8)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public String Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public String ConfirmPassword { get; set; }
	}
}