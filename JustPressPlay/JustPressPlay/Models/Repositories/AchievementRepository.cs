using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

using System.Data.Entity;

namespace JustPressPlay.Models.Repositories
{
    public class AchievementRepository : Repository
    {
        //------------------------------------------------------------------------------------//
        //-------------------------------------Enums------------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Enums
        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Variables---------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Variables
        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Properties--------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Properties
        #endregion
        //------------------------------------------------------------------------------------//
        //-----------------------------------Constructors-------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Constructors
        /// <summary>
        /// Creates a new achievement repository
        /// </summary>
        /// <param name="unitOfWork">The unit of work that created this repository</param>
        public AchievementRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {

        }
        #endregion
        //------------------------------------------------------------------------------------//
        //---------------------------------Populate ViewModels--------------------------------//
        //------------------------------------------------------------------------------------//
        #region Populate ViewModels
        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Query Methods-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Query Methods

        public List<achievement_template> GetParentAchievements()
        {
            return _dbContext.achievement_template.Where(a => a.is_repeatable == true).ToList();
        }

        public int GetAchievementState(int id)
        {
            return _dbContext.achievement_template.Find(id).state;
        }

        public achievement_instance InstanceExists(int instanceID)
        {
            return _dbContext.achievement_instance.Find(instanceID);
        }

        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Insert/Delete-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Insert/Delete

        private void AddAchievementToDatabase(achievement_template achievementTemplate)
        {
            //add the achievement
            if (achievementTemplate != null)
                _dbContext.achievement_template.Add(achievementTemplate);            
        }

        private void AddRequirementsToDatabase(List<achievement_requirement> requirementsList)
        {
            //add all the requirements
            foreach (achievement_requirement requirement in requirementsList)
                _dbContext.achievement_requirement.Add(requirement);
        }

        private void AddCaretakersToDatabase(List<achievement_caretaker> caretakersList)
        {
            //add any caretakers if need be
            if (caretakersList != null)
                foreach (achievement_caretaker caretaker in caretakersList)
                    _dbContext.achievement_caretaker.Add(caretaker);
        }


        //-----Admin Insert/Delete-----//
        #region Admin Insert/Delete

        public void AdminAddAchievement(AddAchievementViewModel model)
        {
            //Create the new achievement template from the model
            achievement_template newAchievement = new achievement_template()
            {
                title = model.Title,
                description = model.Description,
                icon = model.IconFilePath,
                type = model.Type,
                featured = false,
                hidden = model.Hidden,
                is_repeatable = model.IsRepeatable,
                state = (int)JPPConstants.AchievementQuestStates.Draft,
                parent_id = model.ParentID,
                threshold = model.Threshold,
                creator_id = model.CreatorID,
                created_date = DateTime.Now,
                posted_date = null,
                retire_date = null,
                modified_date = null,
                last_modified_by_id = null,
                content_type = model.ContentType,
                system_trigger_type = model.SystemTriggerType,
                repeat_delay_days = model.RepeatDelayDays,
                points_create = model.PointsCreate,
                points_explore = model.PointsExplore,
                points_learn = model.PointsLearn,
                points_socialize = model.PointsSocialize

            };

            //Create all the requirements for the achievement to be added to the database
            List<achievement_requirement> requirementsList = new List<achievement_requirement>();
            for (int i = 0; i < model.RequirementsList.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(model.RequirementsList[i]))
                    requirementsList.Add(new achievement_requirement() { achievement_id = newAchievement.id, description = model.RequirementsList[i] });
            }


            //Create all the caretakers for the achievement to be added to the database
            List<achievement_caretaker> caretakersList = new List<achievement_caretaker>();
            if (model.SelectedCaretakersList != null)
            {
                for (int j = 0; j < model.SelectedCaretakersList.Count; j++)
                {
                    caretakersList.Add(new achievement_caretaker() { achievement_id = newAchievement.id, caretaker_id = model.SelectedCaretakersList[j] });
                }
            }

            AddAchievementToDatabase(newAchievement);
            AddRequirementsToDatabase(requirementsList);
            AddCaretakersToDatabase(caretakersList);

            Save();
        }

        public void AdminEditAchievement(int id, EditAchievementViewModel model)
        {
            achievement_template currentAchievement = _dbContext.achievement_template.Find(id);
           
            //Create all the requirements for the achievement to be added to the database
            List<achievement_requirement> requirementsList = new List<achievement_requirement>();
            for (int i = 0; i < model.RequirementsList.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(model.RequirementsList[i]))
                    requirementsList.Add(new achievement_requirement() { achievement_id = currentAchievement.id, description = model.RequirementsList[i] });
            }

            //Get the old list of requirements and remove them from the database to prevent duplicate entries
            List<achievement_requirement> oldRequirements = _dbContext.achievement_requirement.Where(ar => ar.achievement_id == currentAchievement.id).ToList();
            foreach (achievement_requirement requirement in oldRequirements)
                _dbContext.achievement_requirement.Remove(requirement);


            //Create all the caretakers for the achievement to be added to the database
            List<achievement_caretaker> caretakersList = new List<achievement_caretaker>();
            if (model.SelectedCaretakersList != null)
                for (int j = 0; j < model.SelectedCaretakersList.Count; j++)
                    caretakersList.Add(new achievement_caretaker() { achievement_id = currentAchievement.id, caretaker_id = model.SelectedCaretakersList[j] });

            //Get the old list of caretakers and remove them from the database to prevent duplicate entries
            List<achievement_caretaker> oldCaretakers = _dbContext.achievement_caretaker.Where(ac => ac.achievement_id == currentAchievement.id).ToList();
            foreach (achievement_caretaker caretaker in oldCaretakers)
                _dbContext.achievement_caretaker.Remove(caretaker);

            //Compare the current achievement values in the DB to the model, if they are different, set the current achievement values equal to the model values
            //ContentType
            if (model.ContentType != currentAchievement.content_type)
                currentAchievement.content_type = model.ContentType;
            //Description
            if (!String.IsNullOrWhiteSpace(model.Description) && !String.Equals(currentAchievement.description, model.Description))
                currentAchievement.description = model.Description;
            //Hidden
            if (currentAchievement.hidden != model.Hidden)
                currentAchievement.hidden = model.Hidden;
            //Icon
            if (!String.IsNullOrWhiteSpace(model.IconFilePath) && !String.Equals(currentAchievement.icon, model.IconFilePath))
                currentAchievement.icon = model.IconFilePath;
            //IsRepeatable
            if(currentAchievement.is_repeatable != model.IsRepeatable)
                currentAchievement.is_repeatable = model.IsRepeatable;
            //Last Modified By (userID and DateTime)
            currentAchievement.last_modified_by_id = model.EditorID;
            currentAchievement.modified_date = DateTime.Now;
            //Parent Achievement ID
            if(currentAchievement.parent_id != model.ParentID)
                currentAchievement.parent_id = model.ParentID;
            //Points Create
            if(currentAchievement.points_create != model.PointsCreate)
                currentAchievement.points_create = model.PointsCreate;
            //Points Explore
            if(currentAchievement.points_explore != model.PointsExplore)
                currentAchievement.points_explore = model.PointsExplore;
            //Points Learn
            if(currentAchievement.points_learn != model.PointsLearn)
                currentAchievement.points_learn = model.PointsLearn;
            //Points Socialize
            if(currentAchievement.points_socialize != model.PointsSocialize)
                currentAchievement.points_socialize = model.PointsSocialize;
            //Posted Date
            if(currentAchievement.state != model.State && model.State == (int)JPPConstants.AchievementQuestStates.Active && currentAchievement.posted_date == null)
                currentAchievement.posted_date = DateTime.Now;
            //Repeat Delay Days
            if(currentAchievement.repeat_delay_days != model.RepeatDelayDays)
                currentAchievement.repeat_delay_days = model.RepeatDelayDays;
            //Retire Date
            if (currentAchievement.state != model.State && model.State == (int)JPPConstants.AchievementQuestStates.Retired && currentAchievement.retire_date == null)
                currentAchievement.retire_date = DateTime.Now;
            if (currentAchievement.state != model.State && currentAchievement.state == (int)JPPConstants.AchievementQuestStates.Retired)
                currentAchievement.retire_date = null;
            //Achievement State
            if(currentAchievement.state != model.State)
                currentAchievement.state = model.State;
            //Featured
            if (currentAchievement.state != (int)JPPConstants.AchievementQuestStates.Active)
                currentAchievement.featured = false;
            //System Trigger Type
            if(currentAchievement.system_trigger_type != model.SystemTriggerType)
                currentAchievement.system_trigger_type = model.SystemTriggerType;
            //Threshold
            if(currentAchievement.threshold != model.Threshold)
                currentAchievement.threshold = model.Threshold;
            //Title
            if(!String.IsNullOrWhiteSpace(model.Title) && !String.Equals(currentAchievement.title, model.Title))
                currentAchievement.title = model.Title;
            //Type
            if(currentAchievement.type != model.Type)
                currentAchievement.type = model.Type;

            AddRequirementsToDatabase(requirementsList);
            AddCaretakersToDatabase(caretakersList);

            Save();
        }

		/// <summary>
		/// Assigns an achievement
		/// TODO: Put in lots more error checking!
		/// </summary>
		/// <param name="userID">The id of the user getting the achievement</param>
		/// <param name="achievementID">The id of the achievement template</param>
		/// <param name="assignedByID">The id of the user assigning the achievement</param>
		/// <param name="cardGiven">Was the card given to the user?</param>
		public void AssignAchievement(int userID, int achievementID, int? assignedByID = null, bool cardGiven = false)
		{
			// Get the achievement template
			achievement_template template = _dbContext.achievement_template.Find(achievementID);
			if( template == null )
				throw new ArgumentException("Invalid achievement ID");

			// Create the new instance
			achievement_instance newInstance = new achievement_instance()
			{
				achieved_date = DateTime.Now,
				achievement_id = achievementID,
				assigned_by_id = assignedByID.HasValue ? assignedByID.Value : userID,
				card_given = cardGiven,
				card_given_date = cardGiven ? (Nullable<DateTime>)DateTime.Now : null,
				comments_disabled = false,
				has_user_content = false,	// TODO: Make this work!
				has_user_story = false,
				points_create = template.points_create,
				points_explore = template.points_explore,
				points_learn = template.points_learn,
				points_socialize = template.points_socialize,
				user_content_id = null,		// TODO: Make this work!
				user_id = userID,
				user_story_id = null
			};

			// Add the instance to the database
			_dbContext.achievement_instance.Add(newInstance);
			_dbContext.SaveChanges();
		}

        public void AwardCard(achievement_instance instance)
        {
            if(!instance.card_given)
                instance.card_given_date = DateTime.Now;
            instance.card_given = true;
            
            Save();
        }

        public void RevokeCard(achievement_instance instance)
        {
            if (instance.card_given)
                instance.card_given_date = null;
            instance.card_given = false;

            Save();
        }

        #endregion
        //-----User Insert/Delete------//
        #region User Insert/Delete
        #endregion

        #endregion
        //------------------------------------------------------------------------------------//
        //-------------------------------------Persistence------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Persistence
        public void Save()
        {
            _dbContext.SaveChanges();
        }
        #endregion
        //------------------------------------------------------------------------------------//
        //-----------------------------------Helper Methods-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Helper Methods
        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------JSON Methods------------------------------------//
        //------------------------------------------------------------------------------------//
        #region JSON Methods
        #endregion
        //------------------------------------------------------------------------------------//
        //---------------------------------System Achievements--------------------------------//
        //------------------------------------------------------------------------------------//
        #region System Achievements
        #endregion

    }

}