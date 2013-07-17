using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Basic player information for their profile
	/// </summary>
	public class ProfileViewModel
	{
		public int ID { get; set; }
		public String DisplayName { get; set; }
		public String FirstName { get; set; }
		public String MiddleName { get; set; }
		public String LastName { get; set; }
		public String SixWordBio { get; set; }
		public String FullBio { get; set; }
		public String Image { get; set; }
		public int? PointsCreate { get; set; }
		public int? PointsExplore { get; set; }
		public int? PointsLearn { get; set; }
		public int? PointsSocialize { get; set; }

		/// <summary>
		/// Fills out a profile view model
		/// </summary>
		/// <param name="id">The id of the player</param>
		/// <param name="work">The unit of work to use. If null, one will be created.</param>
		/// <returns>A complete profile view model</returns>
		public static ProfileViewModel Populate(int id, UnitOfWork work = null)
		{
			if (id <= 0) return null;
			if (work == null) work = new UnitOfWork();

			// Grab the user
			var q = from u in work.EntityContext.user
					where u.id == id
					select new ProfileViewModel()
					{
						ID = u.id,
						DisplayName = u.display_name,
						FirstName = u.first_name,
						MiddleName = u.middle_name,
						LastName = u.last_name,
						SixWordBio = u.six_word_bio,
						FullBio = u.full_bio,
						Image = u.image,
						PointsCreate = (from ai in work.EntityContext.achievement_instance where ai.user_id == id select ai.points_create).Sum(),
						PointsExplore = (from ai in work.EntityContext.achievement_instance where ai.user_id == id select ai.points_explore).Sum(),
						PointsLearn = (from ai in work.EntityContext.achievement_instance where ai.user_id == id select ai.points_learn).Sum(),
						PointsSocialize = (from ai in work.EntityContext.achievement_instance where ai.user_id == id select ai.points_socialize).Sum()
					};

			// Convert and return
			return q.FirstOrDefault();
		}
	}

	/// <summary>
	/// Holds a list of players
	/// </summary>
	[DataContract]
	public class PlayersListViewModel
	{
		/// <summary>
		/// The list of players entries
		/// </summary>
		[DataMember]
		public List<PlayersListEntry> People;

		/// <summary>
		/// Holds a single entry for the players list
		/// </summary>
		[DataContract]
		public class PlayersListEntry
		{
			[DataMember]
			public int ID { get; set; }

			[DataMember]
			public String DisplayName { get; set; }

			[DataMember]
			public String FirstName { get; set; }

			[DataMember]
			public String MiddleName { get; set; }

			[DataMember]
			public String LastName { get; set; }

			[DataMember]
			public String Image { get; set; }
		}

		/// <summary>
		/// Populates a list of players
		/// </summary>
		/// <param name="work">The Unit of Work to use. If null, one will be created</param>
		/// <returns>A list of players</returns>
		public static PlayersListViewModel Populate(UnitOfWork work = null)
		{
			if (work == null) work = new UnitOfWork();

			var q = from p in work.EntityContext.user
					where p.is_player == true
					select new PlayersListEntry()
					{
						ID = p.id,
						DisplayName = p.display_name,
						FirstName = p.first_name,
						MiddleName = p.middle_name,
						LastName = p.last_name,
						Image = p.image
					};

			return new PlayersListViewModel()
			{
				People = q.ToList()
			};
		}
	}

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