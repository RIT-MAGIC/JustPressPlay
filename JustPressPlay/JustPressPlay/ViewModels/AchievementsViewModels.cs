using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.WebData;
using System.Runtime.Serialization;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// A list of achievements
	/// </summary>
	[DataContract]
	public class AchievementsListViewModel
	{
		[DataContract]
		public class BasicAchievementInfo
		{
			[DataMember]
			public int ID { get; set; }

			[DataMember]
			public String Title { get; set; }

			[DataMember]
			public String Image { get; set; }

			[DataMember]
			public int PointsCreate { get; set; }

			[DataMember]
			public int PointsExplore { get; set; }

			[DataMember]
			public int PointsLearn { get; set; }

			[DataMember]
			public int PointsSocialize { get; set; }
		}

		[DataMember]
		public List<BasicAchievementInfo> Achievements { get; set; }

		[DataMember]
		public int Total { get; set; }

		/// <summary>
		/// Populates a view model with a list of achievements
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="achievementsEarned">True for earned achievements, false for unearned, null for all. UserID required.</param>
		/// <param name="inactiveAchievements">Should inactive achievements be returned? Default is false.</param>
		/// <param name="createPoints">Require create points?</param>
		/// <param name="explorePoints">Require explore points?</param>
		/// <param name="learnPoints">Require learn points?</param>
		/// <param name="socializePoints">Require socialize points?</param>
		/// <param name="search">A string for searching</param>
		/// <param name="work">The unit of work for DB access. If null, one will be created.</param>
		/// <returns>A populated view model with a list of achievements</returns>
		public static AchievementsListViewModel Populate(
			int? userID = null,
			int? questID = null,
			bool? achievementsEarned = null,
			bool inactiveAchievements = false,
			bool? createPoints = null,
			bool? explorePoints = null,
			bool? learnPoints = null,
			bool? socializePoints = null,
			int? start = null,
			int? count = null,
			String search = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Set up the filter query
			var q = from a in work.EntityContext.achievement_template
					select a;

			// User related filtering?
			if (userID != null && achievementsEarned != null && WebSecurity.IsAuthenticated)
			{
				// Earned achievements
				if (achievementsEarned.Value == true)
				{
					q = from a in q
						join i in work.EntityContext.achievement_instance
						on a.id equals i.achievement_id
						where i.user_id == userID.Value
						select a;
				}
				else // Unearned achievements
				{
					// TODO: Fix
				}
			}

			// Include inactive?
			if (inactiveAchievements)
			{
				q = from a in q
					where a.state == (int)JPPConstants.AchievementQuestStates.Active || a.state == (int)JPPConstants.AchievementQuestStates.Inactive
					select a;
			}
			else
			{
				// Only active
				q = from a in q
					where a.state == (int)JPPConstants.AchievementQuestStates.Active
					select a;
			}

			// Filter points
			if (createPoints != null && createPoints.Value == true) q = from a in q where a.points_create > 0 select a;
			if (explorePoints != null && explorePoints.Value == true) q = from a in q where a.points_explore > 0 select a;
			if (learnPoints != null && learnPoints.Value == true) q = from a in q where a.points_learn > 0 select a;
			if (socializePoints != null && socializePoints.Value == true) q = from a in q where a.points_socialize > 0 select a;

			// TODO: Handle search keywords
			// ...

			// Do filtering on titles and descriptions
			if (search != null)
			{
				q = from a in q
					where a.title.Contains(search) || a.description.Contains(search)
					select a;
			}

			// Order by the achievement titles
			q = q.OrderBy(a => a.title);

			// Grab the total before limits
			int total = q.Distinct().ToList().Count();

			// Start at a specific index?
			if (start != null && start.Value > 0)
			{
				q = q.Skip(start.Value);
			}

			// Keep only a specific amount?
			if (count != null)
			{
				q = q.Take(count.Value);
			}

			// All done
			return new AchievementsListViewModel()
			{
				Achievements = (from a in q
								select new BasicAchievementInfo()
								{
									ID = a.id,
									Image = a.icon,
									Title = a.title,
									PointsCreate = a.points_create,
									PointsExplore = a.points_explore,
									PointsLearn = a.points_learn,
									PointsSocialize = a.points_socialize
								}).Distinct().ToList(),
				Total = total
			};
		}
	}

	/// <summary>
	/// A single achievement
	/// </summary>
	[DataContract]
	public class AchievementViewModel
	{
		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public String Title { get; set; }

		[DataMember]
		public String Image { get; set; }

		[DataMember]
		public String Description { get; set; }

		[DataMember]
		public IEnumerable<String> Requirements { get; set; }

		[DataMember]
		public IEnumerable<AssociatedQuest> Quests { get; set; }

		[DataMember]
		public int PointsCreate { get; set; }

		[DataMember]
		public int PointsExplore { get; set; }

		[DataMember]
		public int PointsLearn { get; set; }

		[DataMember]
		public int PointsSocialize { get; set; }

		[DataMember]
		public int State { get; set; }

		[DataMember]
		public int? SubmissionType { get; set; }

		[DataMember]
		public bool CurrentUserHasEarned { get; set; }

		[DataMember]
		public DateTime? CurrentUserEarnedDate { get; set; }

		[DataContract]
		public class AssociatedQuest
		{
			[DataMember]
			public int ID;

			[DataMember]
			public String Title;

			[DataMember]
			public Boolean UserGenerated;
		}

		/// <summary>
		/// Populates an achievement view model with information about a single achievement
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <param name="userID">The id of the user for achievement progress info</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>Info about a single achievement</returns>
		public static AchievementViewModel Populate(int id, UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			bool currentUserEarned = false;
			DateTime? currentUserEarnedDate = null;
			if (WebSecurity.IsAuthenticated)
			{
				achievement_instance instance = (from ai in work.EntityContext.achievement_instance
												 where ai.achievement_id == id && ai.user_id == WebSecurity.CurrentUserId
												 select ai).FirstOrDefault();
				if (instance != null)
				{
					currentUserEarnedDate = instance.achieved_date;
					currentUserEarned = true;
				}
			}

			// Get basic achievement info
			return (from a in work.EntityContext.achievement_template
					where a.id == id
					select new AchievementViewModel()
					{
						Quests = from step in a.quest_achievement_step
								 where step.achievement_id == id
								 select new AssociatedQuest()
								 {
									 ID = step.quest_id,
									 Title = step.quest_template.title,
									 UserGenerated = step.quest_template.user_generated
								 },
						ID = a.id,
						Description = a.description,
						Image = a.icon,
						Requirements = from r in a.achievement_requirement select r.description,
						State = a.state,
						SubmissionType = a.content_type,
						Title = a.title,
						PointsCreate = a.points_create,
						PointsExplore = a.points_explore,
						PointsLearn = a.points_learn,
						PointsSocialize = a.points_socialize,
						CurrentUserEarnedDate = currentUserEarnedDate,
						CurrentUserHasEarned = currentUserEarned
					}).FirstOrDefault();
		}

	}
}