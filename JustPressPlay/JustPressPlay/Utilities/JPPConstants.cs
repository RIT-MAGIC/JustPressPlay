using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

using System.Web.Mvc;
using System.Reflection;

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

				String[] rolesToRemove = currentRoles.Except(roles).ToArray();
				String[] rolesToAdd = roles.Except(currentRoles).ToArray();

				if( rolesToRemove.Length > 0 )
					System.Web.Security.Roles.RemoveUserFromRoles(username, rolesToRemove);

				if (rolesToAdd.Length > 0)
					System.Web.Security.Roles.AddUserToRoles(username, rolesToAdd);
			}
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

        #region Achievement Types
        public enum AchievementTypes
        {
            Scan,
            System,
            Threshold,
            UserSubmission
        }

        #endregion

        #region User Submission Types

        public enum UserSubmissionTypes
        {
            Image,
            Text,
            URL
        }

        #endregion

        #region System Achievement Types

        public enum SystemAvchievementTypes
        {
            Test,
            Testt,
            Testtt
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
	}
}