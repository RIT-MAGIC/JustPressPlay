using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// A list of quests
	/// </summary>
	[DataContract]
	public class QuestsListViewModel
	{
		[DataMember]
		public List<QuestViewModel> Quests { get; set; }

		/// <summary>
		/// Populates a view model with a list of quests
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="completedQuests">Include completed quests?  Defaults to true.</param>
		/// <param name="partiallyCompletedQuests">Include partially completed quests?  Defaults to true.</param>
		/// <param name="inactiveQuests">Include inactive quests?</param>
		/// <param name="userGeneratedQuests">Include user generated quests?</param>
		/// <param name="search">A string for searching</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created.</param>
		/// <returns>A populated view model with a list of quests</returns>
		public static QuestsListViewModel Populate(
			int? userID = null,
			bool? completedQuests = null,
			bool? partiallyCompletedQuests = null,
			bool? inactiveQuests = null,
			bool? userGeneratedQuests = null,
			String search = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Get the initial query
			IEnumerable<QuestViewModel> q = QuestViewModel.GetPopulateQuery(null, userID, work);

			// Set up the filter query
			var final = from a in q
						select a;

			// User related filtering?
			if (userID != null)
			{
				// The default, unfiltered option contains all quests
				bool completed = completedQuests == null ? true : completedQuests.Value;
				bool partial = partiallyCompletedQuests == null ? true : partiallyCompletedQuests.Value;

				// Exclude completed quests?
				if (!completed)
				{
					final = from a in q
							where a.Progress != null && a.Progress.CompletionDate == null
							select a;
				}

				// Exclude partially completed quests?
				if (!partial)
				{
					final = from a in q
							where a.Progress != null &&
								a.Progress.AchievementsEarned != null &&
								a.Progress.AchievementsEarned.Earnings != null &&
								a.Progress.AchievementsEarned.Earnings.Count > 0
							select a;
				}
			}

			// TODO: Handle inactive/active states
			// ...

			// TODO: Handle search keywords
			// ...

			// Do filtering on titles and descriptions
			if (search != null)
			{
				final = from a in final
						where a.Title.Contains(search) || a.Description.Contains(search)
						select a;
			}

			return new QuestsListViewModel()
			{
				Quests = final.ToList()
			};
		}
	}

	/// <summary>
	/// A single quest
	/// </summary>
	[DataContract]
	public class QuestViewModel
	{
		/// <summary>
		/// Represents the user who created the quest
		/// </summary>
		[DataContract]
		public class QuestAuthor
		{
			[DataMember]
			public int ID { get; set; }

			[DataMember]
			public String DisplayName { get; set; }

			[DataMember]
			public String Image { get; set; }

			[DataMember]
			public bool IsPlayer { get; set; }
		}

		/// <summary>
		/// Contains information about a user's progress on a quest, including earnings
		/// </summary>
		[DataContract]
		public class QuestProgress
		{
			[DataMember]
			public int UserID { get; set; }

			[DataMember]
			public bool Tracking { get; set; }

			[DataMember]
			public DateTime? CompletionDate { get; set; }

			[DataMember]
			public EarningsViewModel AchievementsEarned { get; set; }

			[DataMember]
			public AchievementsListViewModel AchievementsNotEarned { get; set; }
		}

		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public String Title { get; set; }

		[DataMember]
		public String Image { get; set; }

		[DataMember]
		public String Description { get; set; }

		[DataMember]
		public List<AchievementViewModel> Achievements { get; set; }

		[DataMember]
		public int Threshold { get; set; }

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public Boolean UserCreated { get; set; }

		[DataMember]
		public QuestAuthor Author { get; set; }

		[DataMember]
		public int State { get; set; }

		[DataMember]
		public QuestProgress Progress { get; set; }

		/// <summary>
		/// Returns the query used for getting quest view model information.
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <param name="userID">The id of the user for achievement progress info</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>A query with information about quests</returns>
		public static IEnumerable<QuestViewModel> GetPopulateQuery(int? id = null, int? userID = null, UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Base query
			var q = from qt in work.EntityContext.quest_template
					select qt;

			// Specific achievement?
			if (id != null)
			{
				q = from qt in q
					where qt.id == id.Value
					select qt;
			}

			// Build final query with the sub-queries (need to mark q as AsEnumerable here for this to work)
			var final = from qt in q.AsEnumerable()
						select new QuestViewModel()
						{
							ID = qt.id,
							Title = qt.title,
							Image = qt.icon,
							Description = qt.description,
							Threshold = qt.threshold == null ? 0 : qt.threshold.Value,
							Achievements = AchievementsListViewModel.Populate(null, qt.id, null, null, null, null, null, null, null, null, work).Achievements,
							CreationDate = qt.created_date,
							UserCreated = qt.user_generated,
							Author = (from u in work.EntityContext.user
									  where u.id == qt.creator_id
									  select new QuestAuthor()
									  {
										  ID = u.id,
										  DisplayName = u.display_name,
										  Image = u.image,
										  IsPlayer = u.is_player
									  }).FirstOrDefault(),
							State = qt.state,
							Progress = userID == null ? null : new QuestProgress()
							{
								UserID = userID.Value,
								Tracking = (from t in work.EntityContext.quest_tracking where t.quest_id == qt.id && t.user_id == userID.Value select t).Any(),
								CompletionDate = (from qi in work.EntityContext.quest_instance
												  where qi.quest_id == qt.id && qi.user_id == userID.Value
												  select (DateTime?)qi.completed_date).FirstOrDefault(),
								AchievementsEarned = EarningsViewModel.Populate(userID.Value, null, qt.id),
								AchievementsNotEarned = AchievementsListViewModel.Populate(userID.Value, qt.id, false, true)
							}
						};

			return final;
		}

		/// <summary>
		/// Populates an quest view model with information about a single quest
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <param name="userID">The id of the user for quest progress info</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>Info about a single quest</returns>
		public static QuestViewModel Populate(int id, int? userID = null, UnitOfWork work = null)
		{
			return GetPopulateQuery(id, userID, work).FirstOrDefault();
		}
	}
}