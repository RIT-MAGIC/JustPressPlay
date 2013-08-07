using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// A list of achievements
	/// </summary>
	[DataContract]
	public class AchievementsListViewModel
	{
		[DataMember]
		public List<AchievementViewModel> Achievements { get; set; }

		/// <summary>
		/// Populates a view model with a list of achievements
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="achievementsEarned">Should earned achievements be returned? Requires the userID parameter. Default is true.</param>
		/// <param name="achievementsNotEarned">Should not-yet-earned achievements be returned? Requires the userID parameter. Default is true.</param>
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
			bool? achievementsNotEarned = null,
			bool? inactiveAchievements = null,
			bool? createPoints = null,
			bool? explorePoints = null,
			bool? learnPoints = null,
			bool? socializePoints = null,
			String search = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Grab the base query
			IEnumerable<AchievementViewModel> q = AchievementViewModel.GetPopulateQuery(null, userID, questID, work);

			// Set up the filter query
			var final = from a in q
						select a;

			// User related filtering?
			if (userID != null)
			{
				// The default, unfiltered option contains all achievements
				bool showEarned = achievementsEarned == null ? true : achievementsEarned.Value;
				bool showUnearned = achievementsNotEarned == null ? true : achievementsNotEarned.Value;

				// Handle other cases
				if (!showEarned && !showUnearned)
				{
					// This results in nothing being shown
					return new AchievementsListViewModel();
				}
				else if (showEarned && !showUnearned)
				{
					final = from a in final
							where a.AchievedCount > 0
							select a;
				}
				else if (!showEarned && showUnearned)
				{
					final = from a in final
							where a.AchievedCount == 0
							select a;
				}
			}

			// TODO: Handle inactive/active states
			// ...

			// Filter points
			if (createPoints != null && createPoints.Value == true) final = from a in final where a.PointsCreate > 0 select a;
			if (explorePoints != null && explorePoints.Value == true) final = from a in final where a.PointsExplore > 0 select a;
			if (learnPoints != null && learnPoints.Value == true) final = from a in final where a.PointsLearn > 0 select a;
			if (socializePoints != null && socializePoints.Value == true) final = from a in final where a.PointsSocialize > 0 select a;

			// TODO: Handle search keywords
			// ...

			// Do filtering on titles and descriptions
			if (search != null)
			{
				final = from a in final
						where a.Title.Contains(search) || a.Description.Contains(search)
						select a;
			}

			// All done
			return new AchievementsListViewModel()
			{
				Achievements = final.ToList()
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
		public List<String> Requirements { get; set; }

		[DataMember]
		public List<AssociatedQuest> AssociatedQuests { get; set; }

		[DataMember]
		public int PointsCreate { get; set; }

		[DataMember]
		public int PointsExplore { get; set; }

		[DataMember]
		public int PointsLearn { get; set; }

		[DataMember]
		public int PointsSocialize { get; set; }

		[DataMember]
		public int AchievedCount { get; set; }

		[DataMember]
		public List<DateTime> AchievedDates { get; set; }

		[DataMember]
		public int State { get; set; }

        [DataMember]
        public int? SubmissionType { get; set; }

		[DataContract]
		public class AssociatedQuest
		{
			[DataMember]
			public int ID;

			[DataMember]
			public String Title;

			[DataMember]
			public String Image;
		}

		/// <summary>
		/// Returns the query used for getting achievement view model information.
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <param name="userID">The id of the user for achievement progress info</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>A query with information about achievements</returns>
		public static IEnumerable<AchievementViewModel> GetPopulateQuery(int? id = null, int? userID = null, int? questID = null, UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Base query
			var q = from a in work.EntityContext.achievement_template
					select a;

			// Check for quest
			if (questID != null)
			{
				q = from a in q
					join step in work.EntityContext.quest_achievement_step
					on a.id equals step.achievement_id
					where step.quest_id == questID.Value
					select a;
			}

			// Specific achievement?
			if( id != null )
			{
				q = from a in q
					where a.id == id.Value
					select a;
			}

			// Build final query with the sub-queries (need to mark q as AsEnumerable here for this to work)
			var final = from a in q.AsEnumerable()
						select new AchievementViewModel()
						{
							ID = a.id,
							Title = a.title,
							Image = a.icon,
							Description = a.description,
							Requirements = (from r in work.EntityContext.achievement_requirement
											where r.achievement_id == a.id
											select r.description).ToList(),
							AssociatedQuests = (from s in work.EntityContext.quest_achievement_step
												where s.achievement_id == a.id
												select new AssociatedQuest()
												{
													ID = s.quest_id,
													Title = s.quest_template.title,
													Image = s.quest_template.icon
												}).ToList(),
							AchievedCount = userID == null ? -1 : (from ai in work.EntityContext.achievement_instance
																   where ai.achievement_id == a.id && ai.user_id == userID.Value
																   select ai).Count(),
							AchievedDates = userID == null ? null : (from ai in work.EntityContext.achievement_instance
																	 where ai.achievement_id == a.id && ai.user_id == userID.Value
																	 select ai.achieved_date).ToList(),
							State = a.state,
							PointsCreate = a.points_create,
							PointsExplore = a.points_explore,
							PointsLearn = a.points_learn,
							PointsSocialize = a.points_socialize,
                            SubmissionType = a.content_type
						};

			return final;
		}

		/// <summary>
		/// Populates an achievement view model with information about a single achievement
		/// </summary>
		/// <param name="id">The id of the achievement</param>
		/// <param name="userID">The id of the user for achievement progress info</param>
		/// <param name="questID">Use this to return only achievements related to a particular quest.</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>Info about a single achievement</returns>
		public static AchievementViewModel Populate(int id, int? userID = null, int? questID = null, UnitOfWork work = null)
		{
			return GetPopulateQuery(id, userID, questID, work).FirstOrDefault();
		}

	}
}