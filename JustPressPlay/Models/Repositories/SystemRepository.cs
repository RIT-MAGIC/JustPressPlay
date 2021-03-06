﻿/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
		/// <param name="autoSave">Should this method auto save the unit of work?</param>
		/// <param name="url">The url this notification should redirect to</param>
		/// <param name="icon">The icon</param>
		public void AddNotification(int destinationID, int sourceID, String message, String icon, String url, bool autoSave = true)
		{
			_unitOfWork.EntityContext.notification.Add(new notification()
			{
				date = DateTime.Now,
				destination_id = destinationID,
				icon = icon,
				message = message,
				source_id = sourceID,
				url = url
			});

			if (autoSave)
				_unitOfWork.SaveChanges();
		}

        public Boolean IgnoreNotification(int id, int userID)
        {
            var notification = _dbContext.notification.Find(id);
            if (notification == null)
                return false;
            if (userID != notification.destination_id)
                return false;

            notification.@new = false;
            Save();

            return true;
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

        public List<JustPressPlay.Utilities.JPPNewsFeed> GetNewsForFeed()
        {
            List<news> dbNewsList = _dbContext.news.Where(n => n.active == true).ToList();
            List<JustPressPlay.Utilities.JPPNewsFeed> newsList = new List<Utilities.JPPNewsFeed>();

            foreach(news n in dbNewsList)
            {
                newsList.Add(new Utilities.JPPNewsFeed()
                {
                    Controller = JustPressPlay.Utilities.JPPConstants.FeaturedControllerType.News.ToString(),
                    Action = JustPressPlay.Utilities.JPPConstants.FeaturedActionType.IndividualNews.ToString(),
                    ID = n.id,
                    Icon = n.image,
                    Title = n.title,
                    Text = n.body
                });

            }

            return newsList;
            
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

        //TODO: SET CONSTANT FOR EXPIRE DATE
        public external_token GenerateAuthorizationToken(string username, string IPAddress)
        {
            /*var exisitingToken = _dbContext.external_token.SingleOrDefault(et => et.id == _unitOfWork.UserRepository.GetUser(username).id && et.source.Equals(IPAddress));
            if (exisitingToken != null)
                _dbContext.external_token.Remove(exisitingToken);*/

            external_token newToken = new external_token()
            {
                user_id = _unitOfWork.UserRepository.GetUser(username).id,
                source = IPAddress,
                created_date = DateTime.Now,
                expiration_date = DateTime.Now.AddMonths(3),
                token = Guid.NewGuid().ToString(),
                refresh_token = Guid.NewGuid().ToString()
            };

            _dbContext.external_token.Add(newToken);
            Save();

            return newToken;
        }

        public external_token GetAuthorizationToken(string token)
        {
            return _dbContext.external_token.SingleOrDefault(et => et.token.Equals(token));            
        }

        public external_token GetAuthorizationTokenByRefresh(string refreshToken)
        {
            return _dbContext.external_token.SingleOrDefault(et => et.refresh_token.Equals(refreshToken));    
        }

        public bool ExpireAuthorizationToken(string token)
        {
            external_token tokenToRemove = _dbContext.external_token.SingleOrDefault(et => et.token.Equals(token));

            if (tokenToRemove == null)
                return false;

            tokenToRemove.expiration_date = DateTime.Now;
            Save();
            return true;
        }

        public bool RemoveAuthorizationToken(string refreshToken)
        {
            external_token tokenToRemove = _dbContext.external_token.SingleOrDefault(et => et.refresh_token.Equals(refreshToken));

            if (tokenToRemove == null)
                return false;

            _dbContext.external_token.Remove(tokenToRemove);
            Save();
            return true;
        }

        public external_token RefreshAuthorizationToken(string token, string refreshToken)
        {
            external_token tokenToRefresh = _dbContext.external_token.SingleOrDefault(et => et.token.Equals(token) && et.refresh_token.Equals(refreshToken));

            if (tokenToRefresh == null)
                return null;

            tokenToRefresh.token = Guid.NewGuid().ToString();
            tokenToRefresh.refresh_token = Guid.NewGuid().ToString();
            tokenToRefresh.created_date = DateTime.Now;
            tokenToRefresh.expiration_date = DateTime.Now.AddMonths(3);
            Save();

            return tokenToRefresh;
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

    }
}