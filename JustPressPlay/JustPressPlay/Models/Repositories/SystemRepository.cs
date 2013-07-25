using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;
using JustPressPlay.ViewModels;

namespace JustPressPlay.Models.Repositories
{
	public class SystemRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="dbContext">The context for DB communications</param>
		/// <param name="unitOfWork">The unit of work that created this repository</param>
		public SystemRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
			
		}


        public void AdminEditHighlights(ManageHighlightsViewModel model)
        {
            // TODO: Optimize?
            // Remove old featured items
            IEnumerable<quest_template> currentFeaturedQuests = _dbContext.quest_template.Where(m => m.featured);
            foreach (quest_template quest in currentFeaturedQuests)
                quest.featured = false;

            IEnumerable<achievement_template> currentFeaturedAchievements = _dbContext.achievement_template.Where(m => m.featured);
            foreach (achievement_template achievement in currentFeaturedAchievements)
                achievement.featured = false;

            // Set new featured items
            if (model.SelectedQuestsIDs != null)
            {
                IEnumerable<quest_template> newFeaturedQuests = _dbContext.quest_template.Where(m => model.SelectedQuestsIDs.Contains(m.id));
                foreach (quest_template quest in newFeaturedQuests)
                    quest.featured = true;
            }

            if (model.SelectedAchievementIDs != null)
            {
                IEnumerable<achievement_template> newFeaturedAchievements = _dbContext.achievement_template.Where(m => model.SelectedAchievementIDs.Contains(m.id));
                foreach (achievement_template achievement in newFeaturedAchievements)
                    achievement.featured = true;
            }

            Save();
        }

        public void AdminEditSiteSettings(ManageSiteSettingsViewModel model)
        {
            // TODO: Save changes in DB
            throw new NotImplementedException("Site settings not yet added to DB!");
        }

        public void AdminAddNewsItem(AddNewsItemViewModel model)
        {
            news newNewsItem = new news()
            {
                active = model.Active,
                body = model.Body,
                created_date = DateTime.Now,
                creator_id = model.CreatorID,
                image = model.ImageFilePath,
                title = model.Title
            };
            _dbContext.news.Add(newNewsItem);

            Save();
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}