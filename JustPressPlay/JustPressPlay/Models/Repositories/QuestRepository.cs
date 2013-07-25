using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JustPressPlay.Utilities;
using System.Data.Entity;
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

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        
    }
}