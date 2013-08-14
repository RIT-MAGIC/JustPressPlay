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

		/// <summary>
		/// Adds a notification to the system
		/// </summary>
		/// <param name="destinationID">The ID of the user who is getting notified</param>
		/// <param name="sourceID">The ID of the source user</param>
		/// <param name="message">The notification message</param>
		/// <param name="icon">The icon</param>
		public void AddNotification(int destinationID, int sourceID, String message, String icon)
		{
			_unitOfWork.EntityContext.notification.Add(new notification()
			{
				date = DateTime.Now,
				destination_id = destinationID,
				icon = icon,
				message = message,
				source_id = sourceID
			});
			_unitOfWork.SaveChanges();
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

        internal void AdminEditNewsItem(int id, EditNewsItemViewModel model)
        {
            news newsItem = _dbContext.news.Find(id);

            if (model.Title != null)
                newsItem.title = model.Title;

            if (model.Body != null)
                newsItem.body = model.Body;

            if (model.ImageFilePath != null)
                newsItem.image = model.ImageFilePath;

            newsItem.active = model.Active;

            Save();
        }

        //TODO: SET CONSTANT FOR EXPIRE DATE, CHECK USER ROLE
        public external_token GenerateAuthorizationToken(string username, string IPAddress)
        {

            external_token newToken = new external_token()
            {
                user_id = _unitOfWork.UserRepository.GetUser(username).id,
                source = IPAddress,
                created_date = DateTime.Now,
                expiration_date = DateTime.Now.AddMinutes(3),
                token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())

            };

            _dbContext.external_token.Add(newToken);
            Save();

            return newToken;
        }

        public external_token GetAuthorizationToken(string refresh)
        {
            return _dbContext.external_token.SingleOrDefault(et => et.token.Equals(refresh));            
        }

        public bool RemoveAuthorizationToken(string token)
        {
            external_token tokenToRemove = _dbContext.external_token.SingleOrDefault(et => et.token.Equals(token));

            if (tokenToRemove == null)
                return false;

            _dbContext.external_token.Remove(tokenToRemove);
            Save();
            return true;
        }

        public external_token RefreshAuthorizationToken(string token)
        {
            external_token tokenToRefresh = _dbContext.external_token.SingleOrDefault(et => et.token.Equals(token));

            if (tokenToRefresh == null)
                return null;

            String newToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            tokenToRefresh.token = newToken;
            tokenToRefresh.created_date = DateTime.Now;
            tokenToRefresh.expiration_date = DateTime.Now.AddMinutes(3);
            Save();

            return tokenToRefresh;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

    }
}