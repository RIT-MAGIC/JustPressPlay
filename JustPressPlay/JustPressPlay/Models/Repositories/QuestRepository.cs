using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using WebMatrix.WebData;

using JustPressPlay.Utilities;
using JustPressPlay.ViewModels;


namespace JustPressPlay.Models.Repositories
{
	public class QuestRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repository</param>
		public QuestRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{

		}

		#region Insert/Delete

		/// <summary>
		/// Add the quest_template to the database
		/// </summary>
		/// <param name="template">The quest_template created from the AddQuestViewModel</param>
		private void AddQuestTemplateToDatabase(quest_template template)
		{
			_dbContext.quest_template.Add(template);
		}

		/// <summary>
		/// Adds the collection of quest_achievement_step(s) to the database
		/// </summary>
		/// <param name="steps">Collection of quest_achievement_step objects created in AdminAddQuest or AdminEditQuest</param>
		private void AddAchievementStepsToDatabase(IEnumerable<quest_achievement_step> steps)
		{
			foreach (quest_achievement_step step in steps)
			{
				_dbContext.quest_achievement_step.Add(step);
			}
		}

		/// <summary>
		/// Creates a new quest_template and adds it to the database
		/// </summary>
		/// <param name="model">The AddQuestViewModel passed in from the controller</param>
		public void AdminAddQuest(AddQuestViewModel model)
		{
			quest_template newQuest = new quest_template
			{
				created_date = DateTime.Now,
				creator_id = model.CreatorID,
				description = model.Description,
				featured = false,
				icon = model.IconFilePath,
				last_modified_by_id = null,
				last_modified_date = null,
				posted_date = null,
				retire_date = null,
				state = (int)JPPConstants.AchievementQuestStates.Draft,
				threshold = model.Threshold,
				title = model.Title,
				user_generated = false,
			};

			List<quest_achievement_step> questAchievementSteps = new List<quest_achievement_step>();
			foreach (int i in model.SelectedAchievementsList)
			{
				quest_achievement_step q = new quest_achievement_step
				{
					achievement_id = i,
					quest_id = newQuest.id
				};
				questAchievementSteps.Add(q);
			}

			AddQuestTemplateToDatabase(newQuest);
			AddAchievementStepsToDatabase(questAchievementSteps);

			Save();
		}

		/// <summary>
		/// Gets the specified quest_template and updates it with any edits
		/// </summary>
		/// <param name="id">The id of the quest_template</param>
		/// <param name="model">The EditQuestViewModel passed in from the controller</param>
		public void AdminEditQuest(int id, EditQuestViewModel model)
		{
			quest_template currentQuest = _dbContext.quest_template.Find(id);

			// Title
			if (currentQuest.title != model.Title && !String.IsNullOrWhiteSpace(model.Title))
				currentQuest.title = model.Title;
			// Description
			if (currentQuest.description != model.Description && !String.IsNullOrWhiteSpace(model.Description))
				currentQuest.description = model.Description;
			// Icon
			if (currentQuest.icon != model.IconFilePath && !String.IsNullOrWhiteSpace(model.IconFilePath))
				currentQuest.icon = model.IconFilePath;
			// Posted Date
			if (currentQuest.state != model.State && model.State.Equals((int)JPPConstants.AchievementQuestStates.Active) && currentQuest.posted_date == null)
				currentQuest.posted_date = DateTime.Now;
			// Retire Date
			if (currentQuest.state != model.State && model.State.Equals((int)JPPConstants.AchievementQuestStates.Retired) && currentQuest.retire_date == null)
				currentQuest.retire_date = DateTime.Now;
			if (currentQuest.state != model.State && currentQuest.state.Equals((int)JPPConstants.AchievementQuestStates.Retired))
				currentQuest.retire_date = null;
			// State
			if (currentQuest.state != model.State)
				currentQuest.state = model.State;
			//Featured
			if (currentQuest.state != (int)JPPConstants.AchievementQuestStates.Active)
				currentQuest.featured = false;
			// Last Modified By
			currentQuest.last_modified_by_id = model.EditorID;
			// Last Modified Date
			currentQuest.last_modified_date = DateTime.Now;
			// Threshold
			if (currentQuest.threshold != model.Threshold)
				currentQuest.threshold = model.Threshold;

			// Replace achievement steps
			IEnumerable<quest_achievement_step> oldQuestAchievementSteps = _dbContext.quest_achievement_step.Where(q => q.quest_id == id);
			foreach (quest_achievement_step step in oldQuestAchievementSteps)
				_dbContext.quest_achievement_step.Remove(step);

			List<quest_achievement_step> newQuestAchievementSteps = new List<quest_achievement_step>();
			foreach (int i in model.SelectedAchievementsList)
			{
				quest_achievement_step q = new quest_achievement_step
				{
					achievement_id = i,
					quest_id = id
				};
				newQuestAchievementSteps.Add(q);
			}

			AddAchievementStepsToDatabase(newQuestAchievementSteps);

			Save();

			CheckAllUserQuestCompletion(currentQuest.id);
		}


        /// <summary>
        /// Checks all users for quest completion
        /// </summary>
        /// <param name="questID">The id of the quest to check</param>
        public void CheckAllUserQuestCompletion(int questID)
        {
            // Get the template
            quest_template template = _dbContext.quest_template.Find(questID);

			if (template == null || template.state != (int)JPPConstants.AchievementQuestStates.Active)
				return;

			// Get the achievements associated with this quest
			var steps = (from a in _dbContext.quest_achievement_step
						 where a.quest_id == questID
						 select a);
			int totalSteps = steps.Count();

			//TODO : DOUBLE CHECK WITH LIZ WHO VALID USERS ARE
			// Get the list of valid users who do not have the quest
			List<user> validUsers = (from p in _dbContext.user
									 where p.is_player == true && p.status != (int)JPPConstants.UserStatus.Deleted
									 select p).ToList();

			// Loop through all players and check for completion
			int completedCount = 0;
			foreach (user validUser in validUsers)
			{
				// Does this user have the quest already?
				bool hasQuest = _dbContext.quest_instance.Any(qi => qi.quest_id == template.id && qi.user_id == validUser.id);
				if (hasQuest)
					continue;


				// Get a count of achievement steps
				int instanceCount = (from a in _dbContext.achievement_instance
									 from s in steps
									 where a.user_id == validUser.id && a.achievement_id == s.achievement_id
									 select a).Count();

				int threshold = template.threshold != null ? (int)template.threshold : steps.Count();

				// Was this enough to trigger the quest?
				if (instanceCount >= threshold)
				{
					// Yes, so give the user the quest!
					CompleteQuest(template.id, validUser.id, false);
					completedCount++;
				}
			}

			// Any completed?
			if (completedCount > 0)
				Save();
		}


		/// <summary>
		/// Checks for completion of quests that have the specified achievement as a quest step
		/// </summary>
		/// <param name="achievementID">ID of the achievement</param>
		/// <param name="userID"></param>
		/// <param name="autoSave"></param>
		public void CheckAssociatedQuestCompletion(int achievementID, user userToCheck)
		{
            //TODO: OPTIMIZE THIS TO SPEED UP
			//Get a list of all the quests that have the passed in achievement as one of its steps
			List<quest_template> questTemplateList = (from t in _dbContext.quest_template
													  join qs in _dbContext.quest_achievement_step on t.id equals qs.quest_id
													  where qs.achievement_id == achievementID
													  select t).ToList();

			foreach (quest_template questTemplate in questTemplateList)
			{
                var steps = (from a in _dbContext.quest_achievement_step
                             where a.quest_id == questTemplate.id
                             select a);

				// Get a count of achievement instances
				int instanceCount = (from a in _dbContext.achievement_instance
									 from s in steps
									 where a.user_id == userToCheck.id && a.achievement_id == s.achievement_id
									 select a).Count();

                int threshold = questTemplate.threshold != null ? (int)questTemplate.threshold : steps.Count();

                // Check the current instance count against the threshold
                if (instanceCount >= threshold)
                    CompleteQuest(questTemplate.id, userToCheck.id);
                else
                {
                    quest_instance questInstance = _dbContext.quest_instance.SingleOrDefault(qi => qi.quest_id == questTemplate.id && qi.user_id == userToCheck.id);
                    if (questInstance != null)
                        RevokeQuest(questInstance);
                }
            }
        }

        private void RevokeQuest(quest_instance questInstance)
        {
            _dbContext.quest_instance.Remove(questInstance);
            Save();
        }

        private void CompleteQuest(int questID, int userID, bool autoSave = true)
        {

            user currentUserToCheck = _dbContext.user.Find(userID);
            if (currentUserToCheck.status != (int)JPPConstants.UserStatus.Active || !currentUserToCheck.is_player)
                return;
            //Check if the quest already exists
            quest_template questTemplate = _dbContext.quest_template.Find(questID);
            if (questTemplate.state == (int)JPPConstants.AchievementQuestStates.Retired || _dbContext.quest_instance.Any(qi => qi.quest_id == questTemplate.id && qi.user_id == userID))
                return;

			quest_instance newInstance = new quest_instance()
			{
				quest_id = questID,
				user_id = userID,
				completed_date = DateTime.Now,
				comments_disabled = true
			};

			_dbContext.quest_instance.Add(newInstance);

			if (autoSave)
				Save();
		}
		#endregion

		#region Query methods

		public IEnumerable<quest_tracking> GetTrackedQuestsForUser(int userID)
		{
			return _dbContext.quest_tracking.Where(q => q.user_id == userID);
		}

		public int GetQuestState(int id)
		{
			return _dbContext.quest_template.Find(id).state;
		}

		#endregion

		/// <summary>
		/// Tracks the specific quest for the current user
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>True if succesful, false otherwise</returns>
		public Boolean Track(int id)
		{
			if (!WebSecurity.IsAuthenticated)
				return false;

			// Get the quest, then the tracking (if it exists)
			quest_template quest = _dbContext.quest_template.Find(id);
			if (quest == null)
				return false;

			Boolean alreadyTracking = (from t in _dbContext.quest_tracking
									   where t.quest_id == quest.id && t.user_id == WebSecurity.CurrentUserId
									   select t).Any();
			if (alreadyTracking)
				return false;

			// Add a new tracking
			quest_tracking tracking = new quest_tracking()
			{
				quest_id = quest.id,
				user_id = WebSecurity.CurrentUserId
			};
			_dbContext.quest_tracking.Add(tracking);
			_dbContext.SaveChanges();
			return true;
		}

		/// <summary>
		/// Untracks the specific quest for the current user
		/// </summary>
		/// <param name="id">The id of the quest</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean Untrack(int id)
		{
			if (!WebSecurity.IsAuthenticated)
				return false;

			// Get the quest, then the tracking (if it exists)
			quest_template quest = _dbContext.quest_template.Find(id);
			if (quest == null)
				return false;

			// Get all (just incase)
			List<quest_tracking> tracking = (from t in _dbContext.quest_tracking
											 where t.quest_id == quest.id && t.user_id == WebSecurity.CurrentUserId
											 select t).ToList();
			if (tracking.Count == 0)
				return false;

			foreach (quest_tracking t in tracking)
				_dbContext.quest_tracking.Remove(t);
			_dbContext.SaveChanges();
			return true;
		}

		public void Save()
		{
			_dbContext.SaveChanges();
		}
	}
}