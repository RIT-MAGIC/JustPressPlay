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
	/// Contains a list of users for editting
	/// </summary>
	public class EditUserListViewModel
	{
		/// <summary>
		/// The list of users for editting
		/// </summary>
		public List<EditUser> Users { get; set; }

		/// <summary>
		/// Holds data about users in the edit user page
		/// </summary>
		public class EditUser
		{
			public int ID { get; set; }
			public String Username { get; set; }
			public String RealName { get; set; }
		}

		/// <summary>
		/// Populates an EditUserListViewModel with the list of all users
		/// </summary>
		/// <param name="work">The Unit of Work to use for DB access</param>
		/// <returns>A list of all users</returns>
		public static EditUserListViewModel Populate(UnitOfWork work = null)
		{
			// Any unit of work?
			if (work == null)
				work = new UnitOfWork();

			// Get the user data
			var q = from u in work.EntityContext.user
					select new EditUser
					{
						ID = u.id,
						RealName = u.first_name + " " + u.middle_name + " " + u.last_name,
						Username = u.username
					};

			return new EditUserListViewModel()
			{
				Users = q.ToList()
			};

		}
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

		[Display(Name = "Six Word Bio", Description = "Optional")]
		public String SixWordBio1 { get; set; }

		[Display(Name = "Six Word Bio", Description = "Optional")]
		public String SixWordBio2 { get; set; }

		[Display(Name = "Six Word Bio", Description = "Optional")]
		public String SixWordBio3 { get; set; }

		[Display(Name = "Six Word Bio", Description = "Optional")]
		public String SixWordBio4 { get; set; }

		[Display(Name = "Six Word Bio", Description = "Optional")]
		public String SixWordBio5 { get; set; }

		[Display(Name = "Six Word Bio", Description = "Optional")]
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
			string[] sixWordBio = user.six_word_bio == null ? new string[0] : user.six_word_bio.Split(' ');

			// Make the new view model and return it
			return new EditUserViewModel()
			{
				ID = user.id,
				DisplayName = user.display_name,
				Email = user.email,
				IsPlayer = user.is_player,
				Roles = System.Web.Security.Roles.GetRolesForUser(user.username),
				FirstName = user.first_name,
				MiddleName = user.middle_name,
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

    //TODO: Need to add keyword support after that is implemented - for edit achievement as well
    public class AddAchievementViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public String Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public String Description { get; set; }

        [Required]
        public HttpPostedFileBase Icon { get; set; }

        public String IconFilePath { get; set; }

        [Required]
        [Display(Name = "Achievement Type")]
        public int Type { get; set; }

        [Required]
        [Display(Name = "Hidden Achievement")]
        public bool Hidden { get; set; }

        [Display(Name = "Parent Achievement")]
        public int? ParentID { get; set; }

        public List<achievement_template> ParentAchievements { get; set; }

        [Display(Name = "Threshold")]
        public int? Threshold { get; set; }

        [Required]
        [Display(Name = "Repeatable")]
        public bool IsRepeatable { get; set; }

        [Required]
        public int CreatorID { get; set; }

        [Display(Name = "User Submission Type")]
        public int? ContentType { get; set; }

        [Display(Name = "System Trigger")]
        public int? SystemTriggerType { get; set; }

        [Display(Name = "Repeat Delay (Days)")]
        public int? RepeatDelayDays { get; set; }

        [Required]
        [Display(Name = "Create")]
        public int PointsCreate { get; set; }

        [Required]
        [Display(Name = "Explore")]
        public int PointsExplore { get; set; }

        [Required]
        [Display(Name = "Learn")]
        public int PointsLearn { get; set; }

        [Required]
        [Display(Name = "Socialize")]
        public int PointsSocialize { get; set; }


        public List<user> PotentialCaretakersList { get; set; }
        public List<int> SelectedCaretakersList { get; set; }

        public List<String> RequirementsList { get; set; }

        public static AddAchievementViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            return new AddAchievementViewModel()
            {
                ParentAchievements = work.AchievementRepository.GetParentAchievements(),
                PotentialCaretakersList = work.UserRepository.GetAllCaretakers().ToList()
            };
        }

    }

    public class EditAchievementListViewModel
    {
        public List<EditAchievement> Achievements { get; set; }

        public class EditAchievement
        {
            public int ID { get; set; }
            public String Title { get; set; }
        }

        public static EditAchievementListViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            var e = from a in work.EntityContext.achievement_template
                    select new EditAchievement
                    {
                        ID = a.id,
                        Title = a.title
                    };
            return new EditAchievementListViewModel()
            {
                Achievements = e.ToList()
            };
        }
    }

    public class EditAchievementViewModel
    {
        [Required]
        [Display(Name = "Title")]
        public String Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public String Description { get; set; }

        
        public HttpPostedFileBase Icon { get; set; }
        public String IconFilePath { get; set; }

        [Required]
        [Display(Name = "Achievement Type")]
        public int Type { get; set; }

        [Required]
        [Display(Name = "Hidden Achievement")]
        public bool Hidden { get; set; }

        [Display(Name = "Parent Achievement")]
        public int? ParentID { get; set; }

        public List<achievement_template> ParentAchievements { get; set; }

        [Display(Name = "Threshold")]
        public int? Threshold { get; set; }

        [Required]
        [Display(Name = "Repeatable")]
        public bool IsRepeatable { get; set; }

        //TODO: Will set this up to use a list of enums later, but for testing will just use a textbox
        [Required]
        [Display(Name = "State")]
        public int State { get; set; }

        [Required]
        public int EditorID { get; set; }

        [Display(Name = "User Submission Type")]
        public int? ContentType { get; set; }

        [Display(Name = "System Trigger")]
        public int? SystemTriggerType { get; set; }

        [Display(Name = "Repeat Delay (Days)")]
        public int? RepeatDelayDays { get; set; }

        [Required]
        [Display(Name = "Create")]
        public int PointsCreate { get; set; }

        [Required]
        [Display(Name = "Explore")]
        public int PointsExplore { get; set; }

        [Required]
        [Display(Name = "Learn")]
        public int PointsLearn { get; set; }

        [Required]
        [Display(Name = "Socialize")]
        public int PointsSocialize { get; set; }


        public List<user> PotentialCaretakersList { get; set; }
        public List<int> SelectedCaretakersList { get; set; }

        public List<String> RequirementsList { get; set; }

        public static EditAchievementViewModel Populate(int id, UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            achievement_template currentAchievement = work.EntityContext.achievement_template.SingleOrDefault(at => at.id == id);

            List<achievement_caretaker> currentCaretakers = work.EntityContext.achievement_caretaker.Where(ac => ac.achievement_id == id).ToList();
            List<int> currentCaretakersIDs = new List<int>();
            foreach (achievement_caretaker caretaker in currentCaretakers)
                currentCaretakersIDs.Add(caretaker.caretaker_id);
            

            List<achievement_requirement> currentAchievementRequirements = work.EntityContext.achievement_requirement.Where(ar => ar.achievement_id == id).ToList();
            List<String> currentAchievementRequirementsText = new List<String>();
            for (int i = 0; i < 7; i++)
            {
                if (currentAchievementRequirements.Count > i)
                    currentAchievementRequirementsText.Add(currentAchievementRequirements[i].description);
                else
                    currentAchievementRequirementsText.Add("");
            }

            return new EditAchievementViewModel()
            {
                Title = currentAchievement.title,
                Description = currentAchievement.description,
                ContentType = currentAchievement.content_type,
                Hidden = currentAchievement.hidden,
                IconFilePath = currentAchievement.icon,
                IsRepeatable = currentAchievement.is_repeatable,
                ParentID = currentAchievement.parent_id,
                PointsCreate = currentAchievement.points_create,
                PointsExplore = currentAchievement.points_explore,
                PointsLearn = currentAchievement.points_learn,
                PointsSocialize = currentAchievement.points_socialize,
                RepeatDelayDays = currentAchievement.repeat_delay_days,
                RequirementsList = currentAchievementRequirementsText,
                SelectedCaretakersList = currentCaretakersIDs,
                State = currentAchievement.state,
                SystemTriggerType = currentAchievement.system_trigger_type,
                Threshold = currentAchievement.threshold,
                Type = currentAchievement.type,
                ParentAchievements = work.AchievementRepository.GetParentAchievements(),
                PotentialCaretakersList = work.UserRepository.GetAllCaretakers()
            };
        }

    }

	/// <summary>
	/// Used when assigning an individual achievement to a user or users
	/// </summary>
	public class AssignIndividualAchievementViewModel
	{
		[Required]
		[Display(Name = "User")]
		public int UserID { get; set; }

		[Required]
		[Display(Name = "Achievement")]
		public int AchievementID { get; set; }

		public List<user> Users { get; set; }
		public List<achievement_template> Achievements { get; set; }

		public static AssignIndividualAchievementViewModel Populate(UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			return new AssignIndividualAchievementViewModel()
			{
				Users = work.EntityContext.user.Where(u => u.is_player == true).ToList(),
				Achievements = work.EntityContext.achievement_template.ToList()
			};
		}
	}


}