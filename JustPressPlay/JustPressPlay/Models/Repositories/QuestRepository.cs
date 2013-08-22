using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                keywords = "",
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
            List<LoggerModel> logChanges = new List<LoggerModel>();
			quest_template currentQuest = _dbContext.quest_template.Find(id);

            if (currentQuest == null)
                return;

            #region// Title
            if (currentQuest.title != model.Title && !String.IsNullOrWhiteSpace(model.Title))
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Quest: " + Logger.EditQuestLogType.Title.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentQuest.title,
                    Value2 = model.Title
                });
                currentQuest.title = model.Title;
            }
            #endregion

            #region// Description
            if (currentQuest.description != model.Description && !String.IsNullOrWhiteSpace(model.Description))
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Quest: " + Logger.EditQuestLogType.Description.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentQuest.description,
                    Value2 = model.Description
                });
                currentQuest.description = model.Description;
            }
            #endregion

            #region// Icon
            if (currentQuest.icon != model.IconFilePath && !String.IsNullOrWhiteSpace(model.IconFilePath))
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Quest: " + Logger.EditQuestLogType.Icon.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentQuest.icon,
                    Value2 = model.IconFilePath
                });
                currentQuest.icon = model.IconFilePath;
            }
            #endregion

            #region// Posted Date
            if (currentQuest.state != model.State && model.State.Equals((int)JPPConstants.AchievementQuestStates.Active) && currentQuest.posted_date == null)
				currentQuest.posted_date = DateTime.Now;
            #endregion

            #region// Retire Date
            if (currentQuest.state != model.State && model.State.Equals((int)JPPConstants.AchievementQuestStates.Retired) && currentQuest.retire_date == null)
				currentQuest.retire_date = DateTime.Now;
			if (currentQuest.state != model.State && currentQuest.state.Equals((int)JPPConstants.AchievementQuestStates.Retired))
				currentQuest.retire_date = null;
            #endregion

            #region// State
            if (currentQuest.state != model.State)
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Quest: " + Logger.EditQuestLogType.State.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentQuest.state.ToString(),
                    Value2 = model.State.ToString()
                });
                currentQuest.state = model.State;
            }
            #endregion

            #region//Featured
            if (currentQuest.state != (int)JPPConstants.AchievementQuestStates.Active)
				currentQuest.featured = false;
            #endregion

            #region// Last Modified By And Date Last Modified
            currentQuest.last_modified_by_id = model.EditorID;
			currentQuest.last_modified_date = DateTime.Now;
            #endregion

            #region// Threshold
            if (currentQuest.threshold != model.Threshold)
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Quest: " + Logger.EditQuestLogType.Threshold.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentQuest.threshold.ToString(),
                    Value2 = model.Threshold.ToString()
                });
                currentQuest.threshold = model.Threshold;
            }
            #endregion

            #region// Replace achievement steps
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

            #endregion

            if (logChanges.Count > 0)
                Logger.LogMultipleEntries(logChanges, _dbContext);

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


			// Get the list of valid users who do not have the quest
			List<user> validUsers = (from p in _dbContext.user
									 where p.is_player == true && p.status == (int)JPPConstants.UserStatus.Active
									 select p).ToList();

			// Loop through all players and check for completion
            bool needToSave = false;
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
					CompleteQuest(template.id, validUser, null, false);
                    needToSave = true;
				}
			}

			// Any completed?
			if (needToSave)
				Save();
		}


		/// <summary>
		/// Checks for completion of quests that have the specified achievement as a quest step
		/// </summary>
		/// <param name="achievementID">ID of the achievement</param>
		/// <param name="userID"></param>
		/// <param name="autoSave"></param>
		public void CheckAssociatedQuestCompletion(int achievementID, user userToCheck, List<achievement_instance> userAchievements, bool autoSave, bool achievementRevoked = false)
		{
			//Get a list of all the quests that have the passed in achievement as one of its steps
			List<quest_template> questTemplateList = (from t in _dbContext.quest_template
													  join qs in _dbContext.quest_achievement_step on t.id equals qs.quest_id
													  where qs.achievement_id == achievementID
													  select t).ToList();
            if (userAchievements == null)
            {
                userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == userToCheck.id).ToList();
            }

			foreach (quest_template questTemplate in questTemplateList)
			{
                var steps = (from a in _dbContext.quest_achievement_step
                             where a.quest_id == questTemplate.id
                             select a);

				// Get a count of achievement instances
				int instanceCount = (from a in userAchievements
									 from s in steps
									 where a.achievement_id == s.achievement_id
									 select a).Count();

                int threshold = questTemplate.threshold != null ? (int)questTemplate.threshold : steps.Count();

                // Check the current instance count against the threshold
                if (instanceCount >= threshold)
                    CompleteQuest(questTemplate.id, userToCheck, achievementID, autoSave);
                else
                {
                    //Only try and revoke if an achievement was revoked from the player. If the quest was updated by and admin
                    //and the player no longer meets the requirements, they still keep the quest.
                    if (achievementRevoked)
                    {
                        quest_instance questInstance = _dbContext.quest_instance.SingleOrDefault(qi => qi.quest_id == questTemplate.id && qi.user_id == userToCheck.id);
                        if (questInstance != null)
                            RevokeQuest(questInstance, achievementID, false);
                    }
                }
            }
        }

        private void RevokeQuest(quest_instance questInstance, int achievementID, bool autoSave)
        {
            #region Log Quest Revoke
            LoggerModel logQuestRevoke = new LoggerModel()
            {
                Action = Logger.QuestInstanceLogType.QuestRevoked.ToString(),
                UserID = questInstance.user_id,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = questInstance.quest_id,
                IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                Value1 = "Achievement:" + achievementID + " was revoked.",
            };
            Logger.LogSingleEntry(logQuestRevoke, _dbContext, false);
            #endregion

            _dbContext.quest_instance.Remove(questInstance);

            if(autoSave)
                Save();
        }

        private void CompleteQuest(int questID, user userToCheck, int? achievementID, bool autoSave)
        {
            if (userToCheck.status != (int)JPPConstants.UserStatus.Active || !userToCheck.is_player)
                return;

            //Check if the quest exists and is active, and if an instance already exists
            quest_template questTemplate = _dbContext.quest_template.Find(questID);
            if (questTemplate.state == (int)JPPConstants.AchievementQuestStates.Retired || _dbContext.quest_instance.Any(qi => qi.quest_id == questTemplate.id && qi.user_id == userToCheck.id))
                return;

			quest_instance newInstance = new quest_instance()
			{
				quest_id = questID,
				user_id = userToCheck.id,
				completed_date = DateTime.Now,
				comments_disabled = true
			};

			_dbContext.quest_instance.Add(newInstance);
			_unitOfWork.SystemRepository.AddNotification(
				userToCheck.id,
				userToCheck.id,
				"You completed the quest [" + questTemplate.title + "]",
				questTemplate.icon,
				new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
					"IndividualQuest",
					"Quests",
					new { id = questTemplate.id }
				) + "#" + userToCheck.id,
				false);

            LoggerModel logQuestUnlock = new LoggerModel()
            {
                Action = Logger.QuestInstanceLogType.QuestUnlocked.ToString(),
                UserID = userToCheck.id,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = questID,
                IDType1 = Logger.LogIDType.QuestTemplate.ToString(),
                ID2 = achievementID,
                IDType2 = Logger.LogIDType.AchievementTemplate.ToString(),
                Value1 = "ID2 represents the ID of the achievement that triggered the quest unlock"
            };

            Logger.LogSingleEntry(logQuestUnlock, _dbContext);

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