using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;

using JustPressPlay.Utilities;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// View model for adding a new user to the site
	/// </summary>
	public class AddUserViewModel
	{
		[Required]
		[Display(Name = "Username")]
		public String Username { get; set; }

		[Required]
		[StringLength(255, ErrorMessage = "The {0} must contain at least {2} characters.", MinimumLength = 8)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public String Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public String ConfirmPassword { get; set; }

		[Required]
		[Display(Name = "Email")]
		public String Email { get; set; }

		[Required]
		[Display(Name = "Display Name")]
		[StringLength(64, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 3)]
		public String DisplayName { get; set; }

		[Required]
		[Display(Name = "Is Player", Description = "Is this user playing the game?")]
		public bool IsPlayer { get; set; }

		[Display(Name = "First Name", Description = "Optional")]
		public String FirstName { get; set; }

		[Display(Name = "Middle Name", Description = "Optional")]
		public String MiddleName { get; set; }

		[Display(Name = "Last Name", Description = "Optional")]
		public String LastName { get; set; }
	}

	/// <summary>
	/// View model for editing a user
	/// </summary>
	public class EditUserViewModel
	{
		[Required]
		public int ID { get; set; }
		
		[Required]
		[Display(Name = "Display Name")]
		[StringLength(64, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 3)]
		public String DisplayName { get; set; }

		[Required]
		[Display(Name = "Email")]
		public String Email { get; set; }

		[Required]
		[Display(Name = "Is Player", Description = "Is this user playing the game?")]
		public bool IsPlayer { get; set; }

		[Display(Name = "Roles", Description = "Optional roles for this user")]
		public String[] Roles { get; set; }

		[Display(Name = "First Name", Description = "Optional")]
		public String FirstName { get; set; }

		[Display(Name = "Middle Name", Description = "Optional")]
		public String MiddleName { get; set; }

		[Display(Name = "Last Name", Description = "Optional")]
		public String LastName { get; set; }

		[Display(Name = "Six Word Bio Word 1", Description = "Optional")]
		public String SixWordBio1 { get; set; }

		[Display(Name = "Six Word Bio Word 2", Description = "Optional")]
		public String SixWordBio2 { get; set; }

		[Display(Name = "Six Word Bio Word 3", Description = "Optional")]
		public String SixWordBio3 { get; set; }

		[Display(Name = "Six Word Bio Word 4", Description = "Optional")]
		public String SixWordBio4 { get; set; }

		[Display(Name = "Six Word Bio Word 5", Description = "Optional")]
		public String SixWordBio5 { get; set; }

		[Display(Name = "Six Word Bio Word 6", Description = "Optional")]
		public String SixWordBio6 { get; set; }

		[Display(Name = "Full Bio", Description = "Optional")]
		public String FullBio { get; set; }

		/// <summary>
		/// Populates an EditUserViewModel for the specified user
		/// </summary>
		/// <param name="id">The user's id</param>
		/// <param name="work">The Unit of Work for database access</param>
		/// <returns>A populated view model, or null</returns>
		public static EditUserViewModel Populate(int id, UnitOfWork work = null)
		{
			// Any unit of work?
			if (work == null)
				work = new UnitOfWork();

			// If the user doesn't exist, return null
			user user = work.UserRepository.GetUser(id);
			if (user == null)
				return null;

			// Split up the bio since it's stored as a single string
			string[] sixWordBio = user.six_word_bio.Split(' ');

			// Make the new view model and return it
			return new EditUserViewModel()
			{
				ID = user.id,
				DisplayName = user.display_name,
				Email = user.email,
				IsPlayer = user.is_player,
				Roles = System.Web.Security.Roles.GetRolesForUser(user.username),
				FirstName = user.first_name,
				MiddleName  =user.middle_name,
				LastName = user.last_name,
				SixWordBio1 = sixWordBio.Length > 0 ? sixWordBio[0] : "",
				SixWordBio2 = sixWordBio.Length > 1 ? sixWordBio[1] : "",
				SixWordBio3 = sixWordBio.Length > 2 ? sixWordBio[2] : "",
				SixWordBio4 = sixWordBio.Length > 3 ? sixWordBio[3] : "",
				SixWordBio5 = sixWordBio.Length > 4 ? sixWordBio[4] : "",
				SixWordBio6 = sixWordBio.Length > 5 ? sixWordBio[5] : "",
				FullBio = user.full_bio
			};
		}
	}
}