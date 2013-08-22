using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.Utilities;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// A list of quests
	/// </summary>
	[DataContract]
	public class QuestsListViewModel
	{
		[DataContract]
		public class BasicQuestInfo
		{
			[DataMember]
			public int ID { get; set; }

			[DataMember]
			public String Title { get; set; }

			[DataMember]
			public String Image { get; set; }
		}

		[DataMember]
		public List<BasicQuestInfo> Quests { get; set; }

		[DataMember]
		public int Total { get; set; }

		/// <summary>
		/// Populates a view model with a list of quests
		/// </summary>
		/// <param name="userID">The id of a user for user-related searches</param>
		/// <param name="completedQuests">Only include completed quests?</param>
		/// <param name="partiallyCompletedQuests">Only include partially completed quests?</param>
		/// <param name="incompleteQuests">Only include fully incomplete quests?</param>
		/// <param name="inactiveQuests">Include inactive quests?</param>
		/// <param name="trackedQuests">Show only tracked quests?</param>
		/// <param name="userGeneratedQuests">Include user generated quests?</param>
		/// <param name="search">A string for searching</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created.</param>
		/// <returns>A populated view model with a list of quests</returns>
		public static QuestsListViewModel Populate(
			int? userID = null,
			bool completedQuests = false,
			bool partiallyCompletedQuests = false,
			bool incompleteQuests = false,
			bool inactiveQuests = false,
			bool trackedQuests = false,
			bool userGeneratedQuests = false,
			int? start = null,
			int? count = null,
			String search = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Set up the filter query
			var query = from q in work.EntityContext.quest_template
						select q;

			// Keep user-gen quests out unless specifically requested - 
			// TODO: Verify this behavior
			if (!userGeneratedQuests)
			{
				query = from q in query
						where q.user_generated == false
						select q;
			}

			// User stuff required authentication
			if (WebSecurity.IsAuthenticated)
			{

				// Tracking?
				if (trackedQuests)
				{
					query = from q in query
							join t in work.EntityContext.quest_tracking
							on q.id equals t.quest_id
							where t.user_id == WebSecurity.CurrentUserId
							select q;
				}

				// Just include completed quests?  Look for quest instance
				if (completedQuests)
				{
					int userForCompleted = userID == null ? WebSecurity.CurrentUserId : userID.Value;

					query = from q in query
							join qi in work.EntityContext.quest_instance
							on q.id equals qi.quest_id
							where qi.user_id == userForCompleted
							select q;
				}

				// Progress-related?
				if (partiallyCompletedQuests || incompleteQuests)
				{
					// Create the query for progress
					var progressQ = from q in query
									join step in work.EntityContext.quest_achievement_step
									on q.id equals step.quest_id
									join ai in work.EntityContext.achievement_instance
									on step.achievement_id equals ai.achievement_id
									select q;

					// Quests where the achieved count is zero
					if (incompleteQuests)
					{
						query = from q in query
								where progressQ.Count() == 0
								select q;
					}

					// Quests where at least some of the threshold has been met,
					// but not the whole thing!
					if (partiallyCompletedQuests)
					{
						query = from q in query
								let c = progressQ.Count()
								where c > 0 && c < q.threshold
								select q;
						// TODO: Update current quests to use 
					}
				}
			}

			// Include inactive?
			if (inactiveQuests)
			{
				query = from q in query
						where q.state == (int)JPPConstants.AchievementQuestStates.Active || q.state == (int)JPPConstants.AchievementQuestStates.Inactive
						select q;
			}
			else
			{
				// Only active
				query = from q in query
						where q.state == (int)JPPConstants.AchievementQuestStates.Active
						select q;
			}


			// TODO: Handle search keywords
			// ...

			// Do filtering on titles and descriptions
			if (search != null)
			{
				query = from q in query
						where q.title.Contains(search) || q.description.Contains(search)
						select q;
			}

			// Order by the title
			query = query.OrderBy(q => q.title);

			// Get the total before limits
			int total = query.Count();

			// Start at a specific index?
			if (start != null && start.Value > 0)
			{
				query = query.Skip(start.Value);
			}

			// Keep only a specific amount?
			if (count != null)
			{
				query = query.Take(count.Value);
			}

			return new QuestsListViewModel()
			{
				Quests = (from q in query
						  select new BasicQuestInfo()
						  {
							  ID = q.id,
							  Image = q.icon,
							  Title = q.title
						  }).ToList(),
				Total = total
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

		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public String Title { get; set; }

		[DataMember]
		public String Image { get; set; }

		[DataMember]
		public String Description { get; set; }

		[DataMember]
		public IEnumerable<AssociatedAchievement> Achievements { get; set; }

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
		public bool Tracking { get; set; }

		[DataMember]
		public bool CurrentUserHasEarned { get; set; }

		[DataMember]
		public DateTime? CurrentUserEarnedDate { get; set; }

		[DataContract]
		public class AssociatedAchievement
		{
			[DataMember]
			public int ID;

			[DataMember]
			public String Title;

			[DataMember]
			public String Image;
		}

		/// <summary>
		/// Populates an quest view model with information about a single quest
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <param name="work">The Unit of Work for DB access.  If null, one will be created</param>
		/// <returns>Info about a single quest</returns>
		public static QuestViewModel Populate(int id, UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			bool currentUserEarned = false;
			DateTime? currentUserEarnedDate = null;
			if (WebSecurity.IsAuthenticated)
			{
				quest_instance instance = (from qi in work.EntityContext.quest_instance
										   where qi.quest_id == id && qi.user_id == WebSecurity.CurrentUserId
										   select qi).FirstOrDefault();
				if (instance != null)
				{
					currentUserEarnedDate = instance.completed_date;
					currentUserEarned = true;
				}
			}

			// Base query
			return (from qt in work.EntityContext.quest_template
					where qt.id == id
					select new QuestViewModel()
					{
						ID = qt.id,
						Title = qt.title,
						Image = qt.icon,
						Description = qt.description,
						Threshold = qt.threshold == null ? 0 : qt.threshold.Value,
						CreationDate = qt.created_date,
						UserCreated = qt.user_generated,
						Tracking = WebSecurity.IsAuthenticated ?
									(from t in work.EntityContext.quest_tracking where t.user_id == WebSecurity.CurrentUserId && t.quest_id == id select t).Any() :
									false,
						Author = new QuestAuthor()
								  {
									  ID = qt.creator.id,
									  DisplayName = qt.creator.display_name,
									  Image = qt.creator.image,
									  IsPlayer = qt.creator.is_player
								  },
						State = qt.state,
						Achievements = from step in qt.quest_achievement_step
									   select new AssociatedAchievement()
									   {
										   ID = step.achievement_id,
										   Image = step.achievement_template.icon,
										   Title = step.achievement_template.title
									   },
						CurrentUserEarnedDate = currentUserEarnedDate,
						CurrentUserHasEarned = currentUserEarned
					}).FirstOrDefault();
		}
	}
}