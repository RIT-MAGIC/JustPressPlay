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

    #region Add/Edit User ViewModels

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
	public class UserListViewModel
	{
		/// <summary>
		/// The list of users for editting
		/// </summary>
		public List<User> Users { get; set; }

		/// <summary>
		/// Holds data about users in the edit user page
		/// </summary>
		public class User
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
		public static UserListViewModel Populate(UnitOfWork work = null)
		{
			// Any unit of work?
			if (work == null)
				work = new UnitOfWork();

			// Get the user data
			var q = from u in work.EntityContext.user
					select new User
					{
                        //TODO: FIX REALNAME (IT IS NULL IF MIDDLE NAME IS NULL
						ID = u.id,
						RealName = u.first_name + " " + u.middle_name + " " + u.last_name,
						Username = u.username
					};

			return new UserListViewModel()
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

    #endregion

    #region Add/Edit Achievement ViewModels
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
        public String Icon { get; set; }
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

		public List<String> IconList { get; set; }

        public static AddAchievementViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            return new AddAchievementViewModel()
            {
                ParentAchievements = work.AchievementRepository.GetParentAchievements(),
                PotentialCaretakersList = work.UserRepository.GetAllCaretakers().ToList(),
				IconList = JPPImage.GetIconFileNames()
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
            public String Icon { get; set; }
        }

        public static EditAchievementListViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            var e = from a in work.EntityContext.achievement_template
                    select new EditAchievement
                    {
                        ID = a.id,
                        Title = a.title,
                        Icon = a.icon
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
    #endregion

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
				Users = work.EntityContext.user.Where(u => u.is_player == true && u.status == (int)JPPConstants.UserStatus.Active).ToList(),
				Achievements = work.EntityContext.achievement_template.Where(at => at.type != (int)JPPConstants.AchievementTypes.UserSubmission).ToList()
			};
		}
	}

    #region Add/Edit Quest ViewModels
    //TODO: ADD KEYWORD SUPPORT and display names
    public class AddQuestViewModel
    {
        [Required]
        public int CreatorID { get; set; }
        [Required]
        public String Title { get; set; }
        [Required]
        public String Description { get; set; }
        public HttpPostedFileBase Icon { get; set; }
        public String IconFilePath { get; set; }
        public List<achievement_template> AchievementsList { get; set; }
        [Display(Name = "Achievements List")]
        public List<int> SelectedAchievementsList { get; set; }
        public int? Threshold { get; set; }
        public bool UserGenerated { get; set; }
        public static AddQuestViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            return new AddQuestViewModel()
            {
                AchievementsList = work.EntityContext.achievement_template.Where(at => at.state != (int)JPPConstants.AchievementQuestStates.Draft).ToList()
            };
        }
    }

    public class EditQuestViewModel
    {
        [Required]
        public int EditorID { get; set; }
        [Required]
        public String Title { get; set; }
        [Required]
        public String Description { get; set; }
        public HttpPostedFileBase Icon { get; set; }
        public String IconFilePath { get; set; }
        public List<achievement_template> AchievementsList { get; set; }
        [Display(Name = "Achievements List")]
        public List<int> SelectedAchievementsList { get; set; }
        public int? Threshold { get; set; }
        [Required]
        public int State { get; set; }


        public static EditQuestViewModel Populate(int id, UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            quest_template currentQuest = work.EntityContext.quest_template.Find(id);

            List<quest_achievement_step> currentQuestSteps = work.EntityContext.quest_achievement_step.Where(qac => qac.quest_id == currentQuest.id).ToList();
            List<int> currentQuestStepsIDs = new List<int>();
            foreach (quest_achievement_step questStep in currentQuestSteps)
                currentQuestStepsIDs.Add(questStep.achievement_id);

            return new EditQuestViewModel()
            {
                Title = currentQuest.title,
                Description = currentQuest.description,
                State = currentQuest.state,
                SelectedAchievementsList = currentQuestStepsIDs,
                Threshold= currentQuest.threshold,
                AchievementsList = work.EntityContext.achievement_template.Where(at => at.state != (int)JPPConstants.AchievementQuestStates.Draft).ToList()
            };
        }
    }

    public class EditQuestListViewModel
    {
        public List<EditQuest> Quests { get; set; }

        public class EditQuest
        {
            public int ID { get; set; }
            public String Title { get; set; }
        }

        public static EditQuestListViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            var e = from a in work.EntityContext.quest_template
                    select new EditQuest
                    {
                        ID = a.id,
                        Title = a.title
                    };
            return new EditQuestListViewModel()
            {
                Quests = e.ToList()
            };
        }
    }
#endregion

    public class ManageUserCardsViewModel
    {
        public List<AchievementCard> AchievementCardList { get; set; }

        public class AchievementCard
        {
            public String Title { get; set; }
            public int InstanceID { get; set; }
            public bool CardGiven { get; set; }
        }

        public static ManageUserCardsViewModel Populate(int id, UnitOfWork work = null)
        {
            if(work == null)
                work = new UnitOfWork();

            List<achievement_instance> selectedUserAchievements = work.EntityContext.achievement_instance.Where(ai => ai.user_id == id).ToList();
            List<AchievementCard> achievementCardList = new List<AchievementCard>();

            foreach (achievement_instance instance in selectedUserAchievements)
            {
                AchievementCard achievementCard = new AchievementCard
                {
                    Title = work.EntityContext.achievement_template.Find(instance.achievement_id).title,
                    InstanceID = instance.id,
                    CardGiven = instance.card_given
                };

                achievementCardList.Add(achievementCard);
            }


            return new ManageUserCardsViewModel
            {
                AchievementCardList = achievementCardList
            };
        }
    }

    public class ManageHighlightsViewModel
    {
        public List<achievement_template> AllAchievementsList { get; set; }

        [Display(Name = "Featured Achievements")]
        public List<int> SelectedAchievementIDs { get; set; }

        public List<quest_template> AllQuestsList { get; set; }

        [Display(Name = "Featured Quests")]
        public List<int> SelectedQuestsIDs { get; set; }

        public static ManageHighlightsViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            List<achievement_template> allAchievements = work.EntityContext.achievement_template.Where(at => at.state == (int)JPPConstants.AchievementQuestStates.Active).ToList();
            IEnumerable<achievement_template> selectedAchievements = work.EntityContext.achievement_template.Where(a => a.featured == true);
            List<int> selectedAchievementIDs = new List<int>();
            foreach (achievement_template achievement in selectedAchievements)
                selectedAchievementIDs.Add(achievement.id);

            List<quest_template> allQuests = work.EntityContext.quest_template.ToList();
            IEnumerable<quest_template> selectedQuests = work.EntityContext.quest_template.Where(q => q.featured == true);
            List<int> selectedQuestIDs = new List<int>();
            foreach (quest_template quest in selectedQuests)
                selectedQuestIDs.Add(quest.id);

            return new ManageHighlightsViewModel()
            {
                AllAchievementsList = allAchievements,
                AllQuestsList = allQuests,
                SelectedAchievementIDs = selectedAchievementIDs,
                SelectedQuestsIDs = selectedQuestIDs,
            };
        }
    }

    public class ManageSiteSettingsViewModel : IValidatableObject
    {
        // TODO: Implement bulk add user upload

        [Required]
        [Display(Name = "Nav bar color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string NavBarColor { get; set; }

        [Required]
        [Display(Name = "Create color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string CreateColor { get; set; }

        [Required]
        [Display(Name = "Explore color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string ExploreColor { get; set; }

        [Required]
        [Display(Name = "Learn color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string LearnColor { get; set; }

        [Required]
        [Display(Name = "Socialize color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string SocializeColor { get; set; }

        [Required]
        [Display(Name = "Quest color")]
        [RegularExpression(@"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex code")]
        public string QuestColor { get; set; }

        [Required]
        [Display(Name = "Organization name")]
        public String OrganizationName { get; set; }

        // TODO: Require logo?
        [Display(Name = "Site logo")]
        public HttpPostedFileBase SiteLogo { get; set; }
        public String SiteLogoFilePath { get; set; }

        [Required]
        [Display(Name = "Maximum points per achievement")]
        public int MaximumPointsPerAchievement { get; set; }

        [Required]
        [Display(Name = "Enable card distribution")]
        public bool EnableCardDistribution { get; set; }

        [Required]
        [Display(Name = "Allow self-registration")]
        public bool AllowSelfRegistration { get; set; }

        [Required]
        [Display(Name = "Allow user-generated quests")]
        public bool AllowUserGeneratedQuests { get; set; }

        [Required]
        [Display(Name = "Allow comments")]
        public bool AllowComments { get; set; }

        [Required]
        [Display(Name = "Enable Facebook integration")]
        public bool EnableFacebookIntegration { get; set; }

        [Display(Name = "Facebook App ID")]
        public string FacebookAppId { get; set; }

        [Display(Name = "Facebook App Secret")]
        public string FacebookAppSecret { get; set; }

        [Display(Name = "Facebook App Namespace")]
        public string FacebookAppNamespace { get; set; }

        public static ManageSiteSettingsViewModel Populate(UnitOfWork work = null)
        {
            return new ManageSiteSettingsViewModel()
            {
                NavBarColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorNavBar),
                CreateColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorCreate),
                ExploreColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorExplore),
                LearnColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorLearn),
                SocializeColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorSocialize),
                QuestColor = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.ColorQuest),
                OrganizationName = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolName),
                SiteLogoFilePath = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SchoolLogo),
                MaximumPointsPerAchievement = int.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.MaxPointsPerAchievement)),
                EnableCardDistribution = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CardDistributionEnabled)),
                AllowSelfRegistration = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SelfRegistrationEnabled)),
                AllowUserGeneratedQuests = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.UserGeneratedQuestsEnabled)),
                AllowComments = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CommentsEnabled)),
                EnableFacebookIntegration = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled)),
                FacebookAppId = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppId),
                FacebookAppSecret = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppSecret),
                FacebookAppNamespace = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppNamespace),
            };
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure Facebook is configured if enabled
            if (EnableFacebookIntegration)
            {
                if (String.IsNullOrWhiteSpace(FacebookAppId))
                    yield return new ValidationResult("Facebook App ID must be supplied if Facebook integration is turned on", new[] { "FacebookAppId" });

                if (String.IsNullOrWhiteSpace(FacebookAppSecret))
                    yield return new ValidationResult("Facebook App Secret must be supplied if Facebook integration is turned on", new[] { "FacebookAppSecret" });

                if (String.IsNullOrWhiteSpace(FacebookAppNamespace))
                    yield return new ValidationResult("Facebook App Namespace must be supplied if Facebook integration is turned on", new[] { "FacebookAppNamespace" });
            }
        }
    }

    public class AddNewsItemViewModel
    {
        [Required]
        public String Title { get; set; }

        [Required]
        public String Body { get; set; }

        public HttpPostedFileBase Image { get; set; }
        public String ImageFilePath { get; set; }

        public bool Active { get; set; }

        [Required]
        public int CreatorID { get; set; }
    }

    public class EditNewsItemViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public String Body { get; set; }

        public HttpPostedFileBase Image { get; set; }
        public String ImageFilePath { get; set; }

        public bool Active { get; set; }

        public static EditNewsItemViewModel Populate(int id, UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            news newsItem = work.EntityContext.news.Find(id);

            return new EditNewsItemViewModel()
            {
                Title = newsItem.title,
                Body = newsItem.body,
                ImageFilePath = newsItem.image,
                Active = newsItem.active
            };
        }
    }

    public class EditNewsItemListViewModel
    {
        public class NewsItemEntry
        {
            public string Title;
            public int Id;
        }

        public List<NewsItemEntry> NewsItems { get; set; }

        public static EditNewsItemListViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            var e = from n in work.EntityContext.news
                    select new NewsItemEntry
                    {
                        Id = n.id,
                        Title = n.title
                    };

            return new EditNewsItemListViewModel()
            {
                NewsItems = e.ToList()
            };
        }
    }

}