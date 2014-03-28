using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Drawing;

using System.Web.Mvc;
using System.Reflection;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.Utilities
{
	/// <summary>
	/// Contains site-wide constants
	/// </summary>
	public static class JPPConstants
	{
		#region Roles
		/// <summary>
		/// Contains all of the roles in Just Press Play.
		/// Roles in this class will be automatically added to the database
		/// during Application_Start() in Global.asax
		/// </summary>
		public static class Roles
		{
			public const String FullAdmin = "FullAdmin";
			public const String AssignIndividualAchievements = "AssignIndividualAchievements";
			public const String AssignGlobalAchievements = "AssignGlobalAchievements";
			public const String HandleUserSubmittedContent = "HandleUserSubmittedContent";
			public const String ModerateAchievementsAndStories = "ModerateAchievementsAndStories";
			public const String CreateEditNews = "CreateEditNews";
			public const String SendAnnouncements = "SendAnnouncements";
			public const String HandleHighlightedAchievements = "HandleHighlightedAchievements";
			public const String DistributeCards = "DistributeCards";
			public const String ViewReports = "ViewReports";
			public const String CreateUsers = "CreateUsers";
			public const String EditUsers = "EditUsers";
			public const String CreateAchievements = "CreateAchievements";
			public const String EditAchievements = "EditAchievements";
			public const String CreateQuests = "CreateQuests";
			public const String EditQuests = "EditQuests";
			public const String ApproveUserSubmittedQuests = "ApproveUserSubmittedQuests";
			public const String ManageSiteSettings = "ManageSiteSettings";

			/// <summary>
			/// Updates a user to ONLY be in the roles specified
			/// </summary>
			/// <param name="username">The username of the user</param>
			/// <param name="roles">The array of roles</param>
			public static void UpdateUserRoles(String username, String[] roles)
			{
				String[] currentRoles = System.Web.Security.Roles.GetRolesForUser(username);
                String[] rolesToRemove;
                String[] rolesToAdd;

                if(roles != null)
                    rolesToRemove = currentRoles.Except(roles).ToArray();
                else
                    rolesToRemove = currentRoles.ToArray();

                if (roles != null)
                    rolesToAdd = roles.Except(currentRoles).ToArray();
                else
                    rolesToAdd = new String[0];

				if( rolesToRemove.Length > 0 )
					System.Web.Security.Roles.RemoveUserFromRoles(username, rolesToRemove);

				if (rolesToAdd.Length > 0)
					System.Web.Security.Roles.AddUserToRoles(username, rolesToAdd);
			}
		}
		#endregion

        #region Valid Characters
        /// <summary>
        /// Contains the REGEX for valid text input
        /// </summary>
        public const String INPUT_VALID_TEXT_REGEX = "^(?!.*--)[A-Za-z0-9\\/.,;:@()&!%^$#*\"'_ \\+\\=\\?\\-]*$";

        /// <summary>
        /// Contains the REGEX for valid text area input - It allows new line characters!
        /// </summary>
        public const String INPUT_VALID_TEXT_AREA_REGEX = "^(?!.*--)[A-Za-z0-9\\/.,;:@()&!%^$#*\"'_ \\r\\n\\+\\=\\?\\-]*$";

        #endregion

        #region Site Settings
        /// <summary>
		/// Contains constants and methods for getting and setting
		/// various site settings
		/// </summary>
		public static class SiteSettings
		{
			public const String ColorNavBar = "ColorNavBar";
			public const String ColorCreate = "ColorCreate";
			public const String ColorExplore = "ColorExplore";
			public const String ColorLearn = "ColorLearn";
			public const String ColorSocialize = "ColorSocialize";
			public const String ColorQuest = "ColorQuest";
            public const String ColorUserQuest = "ColorUserQuest";
			public const String SchoolName = "SchoolName";
			public const String SchoolLogo = "SchoolLogo";
			public const String CardDistributionEnabled = "CardDistributionEnabled";
			public const String SelfRegistrationEnabled = "SelfRegistrationEnabled";
			public const String UserGeneratedQuestsEnabled = "UserGeneratedQuestsEnabled";
			public const String CommentsEnabled = "CommentsEnabled";
			public const String FacebookIntegrationEnabled = "FacebookIntegrationEnabled";
            public const String FacebookAppId = "FacebookAppId";
            public const String FacebookAppSecret = "FacebookAppSecret";
            public const String FacebookAppNamespace = "FacebookAppNamespace";
			public const String SiteInitialized = "SiteInitialized";
            public const String AdminAccountCreated = "AdminAccountCreated";
            public const String AdminUsername = "AdminUsername";
            public const String AdminEmail = "AdminEmail";
            public const String DeletedCommentText = "Comment deleted by ";
            public const String DevPassword = "DevPassword";
            public const String DevPasswordEnabled = "DevPasswordEnabled";

            private static Dictionary<String, String> DefaultValues = new Dictionary<string, string>()
            {
                { ColorNavBar, "#fdb302" },
                { ColorCreate, "#005f95" },
                { ColorExplore, "#8eb936" },
                { ColorLearn, "#ce3146" },
                { ColorSocialize, "#ffcb05" },
                { ColorQuest, "#AA35A5" },
                { ColorUserQuest, "#FF6B1C"},
                { SchoolName, "Just Press Play" },
                { SchoolLogo, "~/Content/Images/Jpp/logo.png" },
                { CardDistributionEnabled, false.ToString() },
                { SelfRegistrationEnabled, false.ToString() },
                { UserGeneratedQuestsEnabled, false.ToString() },
                { CommentsEnabled, true.ToString() },
                { FacebookIntegrationEnabled, false.ToString() }, // Must assume false because we don't have an app ID or app secret set
                { FacebookAppId, string.Empty },
                { FacebookAppSecret, string.Empty },
                { FacebookAppNamespace, string.Empty },
				{ SiteInitialized, false.ToString() },
                { AdminAccountCreated, false.ToString()},
                { AdminUsername, "" },
                { AdminEmail, "" },
                { DeletedCommentText, "[Deleted]" },
                { DevPassword, "Password"},
                { DevPasswordEnabled, false.ToString()}

            };

			/// <summary>
			/// Gets a setting's value from the database.  All values are string. 
			/// Returns a default value if the setting doesn't exist in the database, or null if neither exist.
			/// </summary>
			/// <param name="setting">The setting to get. Use pre-defined constants in SiteSettings class.</param>
			/// <returns>The value of the setting, or null</returns>
			public static String GetValue(String setting)
			{
				UnitOfWork work = new UnitOfWork();

				system_setting ss = (from s in work.EntityContext.system_setting
									 where s.key == setting
									 select s).FirstOrDefault();

                if (ss == null)
                {
                    // Return default
                    // TODO: Log default return?
                    return DefaultValues[setting];
                }

				// Return the actual stored value
				return ss.value;
			}

			/// <summary>
			/// Sets a site setting in the database.  All values are strings.
			/// </summary>
			/// <param name="setting">The setting to set. Use pre-defined constants in SiteSettings class.</param>
			/// <param name="value">The value to store in the database.</param>
			/// <returns>The value of the setting, or null</returns>
			public static void SetValue(String setting, String value)
			{
				UnitOfWork work = new UnitOfWork();

				system_setting ss = (from s in work.EntityContext.system_setting
									 where s.key == setting
									 select s).FirstOrDefault();

				// Making a new one, or updating?
				if (ss == null)
				{
					// Make a new one and add it
					ss = new system_setting()
					{
						key = setting,
						key_hash = 0,	// Unused for now
						value = value
					};
					work.EntityContext.system_setting.Add(ss);
				}
				else
				{
					ss.value = value;
				}

				work.SaveChanges();
			}
		}

		#endregion

        #region Featured Achievement/Quest/News Type
        public enum FeaturedControllerType
        {
            Achievements,
            Quests,
            News
        }

        public enum FeaturedActionType
        {
            IndividualAchievement,
            IndividualQuest,
            IndividualNews
        }
        #endregion

        #region User Status
        /// <summary>
		/// Contains possible user status options
		/// </summary>
		public enum UserStatus
		{
			/// <summary>
			/// Account is currently active
			/// </summary>
			Active = 1,

			/// <summary>
			/// Account is suspended
			/// </summary>
			Suspended = 2,

			/// <summary>
			/// Account has been deactivated, although the user remains
			/// in the system and can be reactivated
			/// </summary>
			Deactivated = 3,

			/// <summary>
			/// Account has been deleted and all user information
			/// has been purged
			/// </summary>
			Deleted = 4
		}
		#endregion

		#region Privacy Settings
		/// <summary>
		/// A user's current privacy setting
		/// </summary>
		public enum PrivacySettings
		{
			/// <summary>
			/// Only friends have access
			/// </summary>
			[Description("Friends Only")]
			FriendsOnly,

			/// <summary>
			/// Everyone with an account has access
			/// </summary>
			[Description("Just Press Play Only")]
			JustPressPlayOnly,

			/// <summary>
			/// Information, achievements, etc are public
			/// </summary>
			[Description("Public")]
			Public
		}
		#endregion

		#region Communication Settings
		/// <summary>
		/// A player's communication settings
		/// </summary>
		public enum CommunicationSettings
		{
			/// <summary>
			/// The user will get all communications
			/// </summary>
			All,

			/// <summary>
			/// The user will only get important (system) communications
			/// </summary>
			Important
		}

        /// <summary>
        /// Used to generate the notification text for Facebook
        /// </summary>
        /// <param name="achievementName">The name of the achievement earned</param>
        /// <returns>The text to appear in the notification</returns>
        public static string GetFacebookNotificationMessage(string achievementName)
        {
            return "You've earned the achievement " + achievementName + "!";
        }
		#endregion

		#region Achievement and Quest States
		/// <summary>
		/// States for achievements and quests. "Hidden" is not a state,
		/// as it can be applied in addition to any state.
		/// </summary>
		public enum AchievementQuestStates
		{
			/// <summary>
			/// It exists but is not yet available to users
			/// </summary>
			Draft,

			/// <summary>
			/// The achievement/quest can be attained
			/// </summary>
			Active,

			/// <summary>
			/// The achievement/quest is visible but not attainable
			/// </summary>
			Inactive,

			/// <summary>
			/// The achievement/quest is visible but can never be attained again
			/// </summary>
			Retired
		}
		#endregion

		#region Achievement Types
		/// <summary>
		/// Types of achievements
		/// </summary>
		public enum AchievementTypes
        {
			/// <summary>
			/// Achievements with caretakers, which can be given via the admin section
			/// or scanned via the mobile app
			/// </summary>
            Scan,

			/// <summary>
			/// Automatic achievements granted by the system
			/// </summary>
            System,

			/// <summary>
			/// Achievements that require a specific number of repeats
			/// of another repeatable achievement
			/// </summary>
            Threshold,

			/// <summary>
			/// Achievements that require the user to submit type of content
			/// to the site
			/// </summary>
            UserSubmission,

			/// <summary>
			/// Can only be assigned by an admin
			/// </summary>
			AdminAssigned
        }

        #endregion

        #region Achievement Points
        public static List<int> AchievementPoints()
        {
            List<int> pointsList = new List<int>();
            for (int i = 0; i <= 4; i++)
            {
                pointsList.Add(i);
            }
            return pointsList;
        }
        #endregion

        #region Comment Locations
        /// <summary>
		/// Possible comment locations
		/// </summary>
		public enum CommentLocation
		{
			Achievement,
			Quest
		}
		#endregion

        #region Handle User Content Actions

        public enum HandleUserContent
        {
            Approve,
            Deny
        }
        #endregion

        #region User Submission Types
        /// <summary>
		/// Types of user submission content
		/// </summary>
        public enum UserSubmissionTypes
        {
            Image = 1,
            Text = 2,
            URL = 3
        }

        #endregion

        #region System Achievement Types
        /// <summary>
        /// TODO: Finalize this list
        /// </summary>
        public enum SystemAchievementTypes
        {
            SixWordBio,
            ProfilePic,
            Friends_x1,
            Friends_x10,
            Friends_x25,
            OneK,
            TenK,
            Ring_x4,
            Ring_x25,
            Ring_x100,
            PublicProfile,
            FacebookLink
        }

        #endregion

        #region AssignAchievementResult
        /// <summary>
        /// Results of Assigning an achievement
        /// </summary>
        public enum AssignAchievementResult
        {
            Success,
            SuccessNoCard,
            SuccessYesCard,
            SuccessRepetition,
            SuccessThresholdTriggered,

            FailureInvalidAchievement,
            FailureInvalidPlayer,
            FailureUnauthorizedPlayer,
            FailureInvalidAssigner,
            FailureUnauthorizedAssigner,
            FailureAlreadyAchieved,
            FailureRepetitionDelay,
            FailureInvalidUserContent,
            FailureOther
        }

        #endregion

        #region Constants Static Helper Methods
        /// <summary>
		/// Converts an enum to a List of SelectListItems, which will contain Description attributes
		/// if found, otherwise it will contain the names of the enum fields
		/// </summary>
		/// <typeparam name="T">The type to convert (must be an enum)</typeparam>
		/// <param name="enumObj">The enum object</param>
		/// <returns>A list of SelectListItems based on the enum</returns>
		public static List<SelectListItem> SelectListFromEnum<T>() where T : struct, IConvertible
		{
			// Make sure it's an enum
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enum");
			}

			// Get the field members
			Type type = typeof(T);
			FieldInfo[] infos = type.GetFields(BindingFlags.Public | BindingFlags.Static);

			// Loop and look for attributes
			List<SelectListItem> list = new List<SelectListItem>();
			for (int i = 0; i < infos.Length; i++)
			{
				DescriptionAttribute attr = infos[i].GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
				String desc = attr == null ? infos[i].Name : attr.Description;
				list.Add(new SelectListItem()
				{
					Text = desc,
					Value = infos[i].GetRawConstantValue().ToString()
				});
			}

			return list;
		}
		#endregion

        #region Image Related

		public static class Images
		{
			public const int SizeSmall = 40;
			public const int SizeMedium = 200;
			public const int SizeLarge = 400;
			public const int UserUploadMinSize = 150;
			public const int UserUploadMaxSize = 1000;
			public const int SiteLogoMaxSize = 200; // TODO: Get real value
			public const int NewsImageMaxSize = 400;
			public const String IconPath = "~/Content/Images/Icons/";
			public const float QuadBorderWidthPercent = 0.025f;
			public const float QuadBorderOffsetPercent = 0.045f;
			public static Color QuadCreateOnColor = ColorTranslator.FromHtml("#005f95");
			public static Color QuadLearnOnColor = ColorTranslator.FromHtml("#ce3146");
			public static Color QuadExploreOnColor = ColorTranslator.FromHtml("#8eb936");
			public static Color QuadSocializeOnColor = ColorTranslator.FromHtml("#ffcb05");
			public static Color QuadCreateOffColor = ColorTranslator.FromHtml("#c2c2c2");
            public static Color QuadLearnOffColor = ColorTranslator.FromHtml("#c2c2c2");
            public static Color QuadExploreOffColor = ColorTranslator.FromHtml("#c2c2c2");
            public static Color QuadSocializeOffColor = ColorTranslator.FromHtml("#c2c2c2");
            //public static Color QuadCreateOffColor = ColorTranslator.FromHtml("#738b98");
            //public static Color QuadLearnOffColor = ColorTranslator.FromHtml("#a67f84");
            //public static Color QuadExploreOffColor = ColorTranslator.FromHtml("#96a180");
            //public static Color QuadSocializeOffColor = ColorTranslator.FromHtml("#b3a674");
			public static Color QuestSystemColor = ColorTranslator.FromHtml("#AA35A5");
			public static Color QuestCommunityColor = ColorTranslator.FromHtml("#FF6B1C");
		}

        #endregion

        #region SendGridStuff

        public const String SendGridUserName = "azure_29980584273dd43e9cb01b1e76beb9c2@azure.com";
        public const String SendGridPassword = "jmg6hyyj";

        #endregion

        #region DevPassword
        public const string devPassword = "IFoughtTheLaw";
        #endregion
    }
}