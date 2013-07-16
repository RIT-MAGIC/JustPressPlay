﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
			FriendsOnly = 1,

			/// <summary>
			/// Everyone with an account has access
			/// </summary>
			JustPressPlayOnly = 2,

			/// <summary>
			/// Information, achievements, etc are public
			/// </summary>
			Public = 3
		}
		#endregion
	}
}