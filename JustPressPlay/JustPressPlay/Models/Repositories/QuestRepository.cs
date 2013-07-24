using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        private void AddQuestTemplateToDatabase(quest_template template)
        {
            _dbContext.quest_template.Add(template);
        }

        private void AddAchievementStepsToDatabase(IEnumerable<quest_achievement_step> steps)
        {
            foreach (quest_achievement_step step in steps)
            {
                _dbContext.quest_achievement_step.Add(step);
            }
        }

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
                state = 0, // TODO: Get state from enum once it's implemented
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

        #endregion

        #region Query methods

        public IEnumerable<quest_tracking> GetTrackedQuestsForUser(int userID)
        {
            return _dbContext.quest_tracking.Where(q => q.user_id == userID);
        }

        #endregion

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}