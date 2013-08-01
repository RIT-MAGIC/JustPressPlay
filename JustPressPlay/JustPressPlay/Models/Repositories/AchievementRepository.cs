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

        public int GetAchievementType(int id)
        {
            return _dbContext.achievement_template.Find(id).type;
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
		public void AssignAchievement(int userID, int achievementID, int? assignedByID = null, bool cardGiven = false, bool checkForQuest = true, bool isRepeat = false)
		{
			// Get the achievement template
			achievement_template template = _dbContext.achievement_template.Find(achievementID);
			if( template == null )
				throw new ArgumentException("Invalid achievement ID");
            // Get the user
            user user = _dbContext.user.Find(userID);
            if (user == null)
                throw new ArgumentException("Invalid user ID");
            // Check if it was achieved and if it is repeatable
            // If repeatable, check the last unlock date
            
            else
            {
                if (_dbContext.achievement_instance.Any(ai => ai.achievement_id == achievementID && ai.user_id == userID))
                    throw new InvalidOperationException("This user already has this achievement");
            }

			// Create the new instance
			achievement_instance newInstance = new achievement_instance()
			{
				achieved_date = DateTime.Now,
				achievement_id = achievementID,
				assigned_by_id = assignedByID.HasValue ? assignedByID.Value : userID,
				card_given = cardGiven,
				card_given_date = cardGiven ? (Nullable<DateTime>)DateTime.Now : null,
				comments_disabled = false,
				has_user_content = false,
				has_user_story = false,
				points_create = isRepeat ? 0 :template.points_create,
                points_explore = isRepeat ? 0 : template.points_explore,
                points_learn = isRepeat ? 0 : template.points_learn,
                points_socialize = isRepeat ? 0 : template.points_socialize,
				user_content_id = null,
				user_id = userID,
				user_story_id = null
			};

			// Add the instance to the database
            _dbContext.achievement_instance.Add(newInstance);
            Save();

            if (checkForQuest)
                _unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(newInstance.achievement_id, newInstance.user_id);

            CheckRingSystemAchievements(userID);
		}

       

        /// <summary>
        /// Used for creating instances of the type "Scan", also will call CheckForThreshold
        /// if an Achievement is repeatable to check for Threshold unlocks
        /// </summary>
        /// <param name="userID">ID of the user to assign the achievement to</param>
        /// <param name="achievementID">The ID of the achievement to assign</param>
        /// <param name="assignedByID">ID of the User that assigns the achievement</param>
        /// <param name="cardGiven">Has a card been given for this achievement</param>
        public void AssignScanAchievement(int userID, int achievementID, int? assignedByID = null, bool cardGiven = false)
        {
            // Get the achievement template
            achievement_template template = _dbContext.achievement_template.Find(achievementID);
            bool isRepeat = false;

            if (template.is_repeatable)
            {
                achievement_instance lastInstance = _dbContext.achievement_instance.Where(ai => ai.achievement_id == achievementID && ai.user_id == userID).ToList().LastOrDefault();
                var today = DateTime.Now.Date;
                if (lastInstance != null)
                {
                    isRepeat = true;
                    var lastInstanceAchieveDate = lastInstance.achieved_date.Date;
                    if (today < lastInstanceAchieveDate.AddDays((double)template.repeat_delay_days))
                    {
                        var waitTime = (lastInstanceAchieveDate.AddDays((double)template.repeat_delay_days) - today).Days;
                        throw new InvalidOperationException("This user last received this achievement on " + lastInstanceAchieveDate.ToShortDateString() + ". They must wait " + waitTime + " day(s) until they can receive this achievement again");
                    }
                }
            }

            AssignAchievement(userID, achievementID, assignedByID, cardGiven, true, isRepeat);
            if (template.is_repeatable)
                CheckForThresholdUnlock(userID, achievementID);
        }

        /// <summary>
        /// Check to see if an scan achievement instance triggers a threshold achievement
        /// </summary>
        /// <param name="userID">ID of the user that was assigned the achievement</param>
        /// <param name="achievementID">ID of the repeatable achievement the user was assigned</param>
        private void CheckForThresholdUnlock(int userID, int achievementID)
        {
            // Get the list of all the specified user instances of achievements
            List<achievement_instance> instanceList = _dbContext.achievement_instance.Where(ai => ai.achievement_id == achievementID && ai.user_id == userID).ToList();
            // Get the list of all the threshold achievements for the specified achievement
            List<achievement_template> thresholdList = _dbContext.achievement_template.Where(at => at.type == (int)JPPConstants.AchievementTypes.Threshold && at.parent_id == achievementID).ToList();

            // This achievement has no threshold achievements
            if (thresholdList == null)
                return;

            // If the threshold achievement instance doesn't exist, assign the achievement to the user
            foreach (achievement_template threshold in thresholdList)
            {
                if (instanceList.Count >= threshold.threshold && !_dbContext.achievement_instance.Any(ai => ai.achievement_id == threshold.id && ai.user_id == userID))
                    AssignAchievement(userID, threshold.id);
            }

        }

        /// <summary>
        /// Assigns the specified achievement to all users in the system.
        /// </summary>
        /// <param name="achievementID">The ID of the achievement to assign</param>
        /// <param name="assignedByID">The ID of the User who assigned the achievement</param>
        public void AssignGlobalAchievement(int achievementID, int? assignedByID = null)
        {
            // Get the achievement template
            achievement_template template = _dbContext.achievement_template.Find(achievementID);
            if (template == null)
                throw new ArgumentException("Invalid achievement ID");

            bool partOfQuest = _dbContext.quest_achievement_step.Any(qas => qas.achievement_id == template.id);

            List<user> qualifiedUsers = _dbContext.user.Where(u => u.status == (int)JPPConstants.UserStatus.Active && u.is_player == true).ToList();

            // Loop through the user list and assign the achievement to each user.
            foreach (user user in qualifiedUsers)
            {
                AssignAchievement(user.id, achievementID, assignedByID, partOfQuest);
            }
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

        private void CheckRingSystemAchievements(int userID)
        {
            List<achievement_template> ringAchievementsList = _dbContext.achievement_template.Where(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Ring_x4 ||
                                                              at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Ring_x25 ||
                                                              at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Ring_x100 &&
                                                              at.state == (int)JPPConstants.AchievementQuestStates.Active).ToList();

            if(ringAchievementsList == null || ringAchievementsList.Count <= 0)
            {
                //None of the Ring Achievements have been made or are inactive so they can't be awarded
                return;
            }

            List<achievement_instance> userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == userID).ToList();

            int totalPointsCreate = userAchievements.Sum(ua => ua.points_create);
            int totalPointsExplore = userAchievements.Sum(ua => ua.points_explore);
            int totalPointsLearn = userAchievements.Sum(ua => ua.points_learn);
            int totalPointsSocialize = userAchievements.Sum(ua => ua.points_socialize);

            foreach (achievement_template ringAchievement in ringAchievementsList)
            {
                if(userAchievements.Any(ua => ua.achievement_id == ringAchievement.id))
                    continue;

                switch (ringAchievement.system_trigger_type)
                {
                    case((int)JPPConstants.SystemAchievementTypes.Ring_x4):
                        if (totalPointsCreate >= 4 && totalPointsExplore >= 4 && totalPointsLearn >= 4 && totalPointsSocialize >= 4)
                        {
                            AssignAchievement(userID, ringAchievement.id);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Ring_x25):
                        if (totalPointsCreate >= 25 && totalPointsExplore >= 25 && totalPointsLearn >= 25 && totalPointsSocialize >= 25)
                        {
                            AssignAchievement(userID, ringAchievement.id);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Ring_x100):
                        if (totalPointsCreate >= 100 && totalPointsExplore >= 100 && totalPointsLearn >= 100 && totalPointsSocialize >= 100)
                        {
                            AssignAchievement(userID, ringAchievement.id);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion

    }

}