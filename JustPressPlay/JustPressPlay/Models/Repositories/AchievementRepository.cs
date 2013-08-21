using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace JustPressPlay.Models.Repositories
{
    public class AchievementRepository : Repository
    {
        //------------------------------------------------------------------------------------//
        //-------------------------------------Enums------------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Enums
        private enum AchievementInstanceResult
        {
            First,
            TooSoon,
            Repeat
        }
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

        public achievement_template GetTemplateById(int id)
        {
            return _dbContext.achievement_template.Find(id);
        }

        public bool DoesUserHaveAchievement(int userId, int achievementId)
        {
            return _dbContext.achievement_instance.Any(t => (t.user_id == userId && t.achievement_id == achievementId));
        }

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

        public Boolean SystemAchievementExists(int systemAchievementType)
        {
            return _dbContext.achievement_template.Any(at => at.system_trigger_type == systemAchievementType);
        }

        public int GetSystemAchievementID(int systemAchievementType)
        {
            return _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == systemAchievementType).id;
        }

        public List<achievement_template> GetAssignableAchievements(int userID, bool isFullAdmin)
        {
            if (isFullAdmin)
                return _dbContext.achievement_template.Where(at => at.type == (int)JPPConstants.AchievementTypes.Scan).ToList();

            List<achievement_caretaker> caretakerList = _dbContext.achievement_caretaker.Where(ac => ac.caretaker_id == userID).ToList();
            List<achievement_template> templateList = new List<achievement_template>();
            foreach (achievement_caretaker caretakerEntry in caretakerList)
            {
                if(caretakerEntry.achievement_template.type == (int)JPPConstants.AchievementTypes.Scan)
                    templateList.Add(caretakerEntry.achievement_template);
            }
            return templateList;
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
                points_socialize = model.PointsSocialize,
                keywords = ""

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


        //TODO: OPTIMIZE THE WAY ACHIEVEMENTS ARE ASSIGNED TO REDUCE DATABASE QUERIES AND SPEED UP THE OVERALL PROCESS
		/// <summary>
		/// Assigns an achievement
		/// TODO: Put in lots more error checking!
		/// </summary>
		/// <param name="userID">The id of the user getting the achievement</param>
		/// <param name="achievementID">The id of the achievement template</param>
		/// <param name="assignedByID">The id of the user assigning the achievement</param>
		/// <param name="cardGiven">Was the card given to the user?</param>
        public JPPConstants.AssignAchievementResult AssignAchievement(int userID, int achievementID, int? assignedByID = null, bool autoSave = true, DateTime? dateAssigned = null, bool cardGiven = false, bool isGlobal = false)
        {
            //Int value to keep track of whether a repeatable achievement can be awarded or not.
            AchievementInstanceResult instanceResult = AchievementInstanceResult.First;
            // Get the achievement template
            achievement_template template = _dbContext.achievement_template.Find(achievementID);
            // Get the user
            user user = _dbContext.user.Find(userID);

            //Get the assigner
            user assigner = _dbContext.user.Find(assignedByID);

            //Make sure the achievement can be assigned
            #region Validate Parameters
            //Make sure the achievement and user exist
            if (template == null)
                return JPPConstants.AssignAchievementResult.FailureInvalidAchievement;
            if (user == null)
                return JPPConstants.AssignAchievementResult.FailureInvalidPlayer;
            if (assigner == null && assignedByID != null)
                return JPPConstants.AssignAchievementResult.FailureInvalidAssigner;
            //Check to make sure the user is a player, and that they are not suspended,deactivate,deleted
            if (user.status != (int)JPPConstants.UserStatus.Active || !user.is_player)
                return JPPConstants.AssignAchievementResult.FailureUnauthorizedPlayer;
            // Check For Instances
            if (_dbContext.achievement_instance.Any(ai => ai.achievement_id == achievementID && ai.user_id == userID) && !template.is_repeatable)
                return JPPConstants.AssignAchievementResult.FailureAlreadyAchieved;
            //Check to see if the achievement is repeatable, and if so, has the user waited long enough since the last time they received it
            if (template.is_repeatable)
                instanceResult = CanAwardRepeatableAchievement(template, userID);
            if (instanceResult.Equals(AchievementInstanceResult.TooSoon))
                return JPPConstants.AssignAchievementResult.FailureRepetitionDelay;
            #endregion

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
                points_create = instanceResult.Equals(AchievementInstanceResult.Repeat) ? 0 : template.points_create,
                points_explore = instanceResult.Equals(AchievementInstanceResult.Repeat) ? 0 : template.points_explore,
                points_learn = instanceResult.Equals(AchievementInstanceResult.Repeat) ? 0 : template.points_learn,
                points_socialize = instanceResult.Equals(AchievementInstanceResult.Repeat) ? 0 : template.points_socialize,
                user_content_id = null,
                user_id = userID,
                user_story_id = null,
                globally_assigned = isGlobal,
            };
            // Add the instance to the database
            _dbContext.achievement_instance.Add(newInstance);
			_unitOfWork.SystemRepository.AddNotification(
				userID,
				newInstance.assigned_by_id,
				"You earned the achievement [" + template.title + "]",
				template.icon,
				new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
					"IndividualAchievement", 
					"Achievements", 
					new { id = template.id }
				) + "#" + userID, 
				false);

            JPPConstants.AssignAchievementResult result = JPPConstants.AssignAchievementResult.Success;

            if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CardDistributionEnabled)))

            {
                if (cardGiven)
                    result = JPPConstants.AssignAchievementResult.SuccessYesCard;
                else
                    result = JPPConstants.AssignAchievementResult.SuccessNoCard;
            }

            if (template.is_repeatable)
            {
                result = JPPConstants.AssignAchievementResult.SuccessRepetition;

                if (CheckForThresholdUnlock(achievementID, userID))
                    result = JPPConstants.AssignAchievementResult.SuccessThresholdTriggered;
            }

            if (!isGlobal)
            {
                List<achievement_instance> userAchievements = _dbContext.achievement_instance.Where(ai=>ai.user_id == userID).ToList().Union(_dbContext.achievement_instance.Local.Where(ai => ai.user_id == userID).ToList()).ToList();
                _unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(achievementID, user, userAchievements, autoSave);
                CheckRingSystemAchievements(userID,userAchievements);
                CheckOneKAndTenKSystemAchievements();
            }

            if (autoSave)
                Save();

            return result;
        }

        private AchievementInstanceResult CanAwardRepeatableAchievement(achievement_template template, int userID)
        {
            achievement_instance lastInstance = _dbContext.achievement_instance.Where(ai => ai.achievement_id == template.id && ai.user_id == userID).ToList().LastOrDefault();

            //If lastInstance is null, the user has never gotten this achievement before, so it can be awarded
            if (lastInstance == null)
                return AchievementInstanceResult.First;

            var dateToday = DateTime.Now.Date;
            var datelastAchievedPlusDelay = lastInstance.achieved_date.Date.AddDays((double)template.repeat_delay_days);
            //Check if enough days have passed since the last instance was achieved
            if (dateToday.CompareTo(datelastAchievedPlusDelay) < 0)
                return AchievementInstanceResult.TooSoon;

            //Enough time has passed, the achievement can be awarded again.
            return AchievementInstanceResult.Repeat;
        }

        #region Old Code For Assigning Scans, this is now handled in AssignAchievement
        /// <summary>
        /// Used for creating instances of the type "Scan", also will call CheckForThreshold
        /// if an Achievement is repeatable to check for Threshold unlocks
        /// </summary>
        /// <param name="userID">ID of the user to assign the achievement to</param>
        /// <param name="achievementID">The ID of the achievement to assign</param>
        /// <param name="assignedByID">ID of the User that assigns the achievement</param>
        /// <param name="cardGiven">Has a card been given for this achievement</param>
        /*   public void AssignScanAchievement(int userID, int achievementID, int assignedByID, DateTime timeAssigned, bool cardGiven = false)
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
           }*/
        #endregion


        /// <summary>
        /// Assigns an achievement with user content associated with it.
        /// TODO: SET UP A VIEWMODEL TO SHORTEN THE AMOUNT OF PARAMETERS
        /// TODO: CHECK THE LOGIC TO MAKE SURE IT ALL WORKS THE WAY IT SHOULD
        /// </summary>
        public void AssignContentSubmissionAchievement(int approvedByID, int contentType, achievement_user_content_pending pendingContent)
        {
            //Assign the achievement
            AssignAchievement(pendingContent.submitted_by_id, pendingContent.achievement_id, approvedByID, false);
            //Get the newly assigned achievement
            achievement_instance newInstance = _dbContext.achievement_instance.SingleOrDefault(ai => ai.user_id == pendingContent.submitted_by_id && ai.achievement_id == pendingContent.achievement_id);            
            //Create the user content to be added
            achievement_user_content newUserContent = new achievement_user_content()
            {
                approved_by_id = approvedByID,
                approved_date = DateTime.Now,
                content_type = contentType,
                image = contentType == (int)JPPConstants.UserSubmissionTypes.Image ? pendingContent.image : null,
                submitted_date = pendingContent.submitted_date,
                text = pendingContent.text,
                url = contentType == (int)JPPConstants.UserSubmissionTypes.URL ? pendingContent.url : null
            };

            //Add the new user content to the database
            _dbContext.achievement_user_content.Add(newUserContent);
            //append the instance to point to the new user content
            newInstance.has_user_content = true;
            newInstance.user_content_id = newUserContent.id;
            //Remove the content from the pending list
            _dbContext.achievement_user_content_pending.Remove(pendingContent);
            //Save changes
            Save();
        }

        //TODO:// Change this to use the list already created
        /// <summary>
        /// Check to see if an scan achievement instance triggers a threshold achievement
        /// </summary>
        /// <param name="achievementID">ID of the repeatable achievement the user was assigned</param>
        /// <param name="userID">ID of the user that was assigned the achievement</param>
        private bool CheckForThresholdUnlock(int achievementID,int userID)
        {
            bool thresholdUnlocked = false;
            // Get the list of all the specified user instances of achievements
            _dbContext.achievement_instance.Where(ai => ai.achievement_id == achievementID && ai.user_id == userID).Load();
            int instanceCount = _dbContext.achievement_instance.Local.Count(ai => ai.achievement_id == achievementID && ai.user_id == userID);
            // Get the list of all the threshold achievements for the specified achievement
            List<achievement_template> thresholdList = _dbContext.achievement_template.Where(at => at.type == (int)JPPConstants.AchievementTypes.Threshold && at.parent_id == achievementID).ToList();
            // This achievement has no threshold achievements
            if (thresholdList == null)
                return thresholdUnlocked;

            // If the threshold achievement instance doesn't exist, assign the achievement to the user
            foreach (achievement_template threshold in thresholdList)
            {
                if (instanceCount >= threshold.threshold && !_dbContext.achievement_instance.Any(ai => ai.achievement_id == threshold.id && ai.user_id == userID))
                {
                    AssignAchievement(userID, threshold.id, null, false);
                    thresholdUnlocked = true;
                }                    
            }

            return thresholdUnlocked;
        }

        //TODO : OPTIMIZE THIS
        /// <summary>
        /// Assigns the specified achievement to all users in the system.
        /// </summary>
        /// <param name="achievementID">The ID of the achievement to assign</param>
        /// <param name="assignedByID">The ID of the User who assigned the achievement</param>
        public void AssignGlobalAchievement(int achievementID, DateTime startRange, DateTime endRange, int? assignedByID = null)
        {
            // Get the achievement template
            achievement_template template = _dbContext.achievement_template.Find(achievementID);
            if (template == null)
                throw new ArgumentException("Invalid achievement ID");

            bool partOfQuest = _dbContext.quest_achievement_step.Any(qas => qas.achievement_id == template.id);

            List<user> qualifiedUsers = _dbContext.user.Where(u => u.status == (int)JPPConstants.UserStatus.Active && u.is_player == true && u.created_date.Date >= startRange.Date && u.created_date.Date <= endRange.Date).ToList();

            // Loop through the user list and assign the achievement to each user.
            foreach (user user in qualifiedUsers)
            {
                AssignAchievement(user.id, achievementID, assignedByID, false, null, false, true);
            }

            List<achievement_instance> localAndDatabaseInstances = _dbContext.achievement_instance.Local.ToList().Union(_dbContext.achievement_instance.ToList()).ToList();

            //List that will hold a specific user's achievements
            List<achievement_instance> userAchievements = new List<achievement_instance>();
            //Check for quest unlocks and Ring system achievement unlocks
            foreach (user user in qualifiedUsers)
            {
                //Get only the current users achievements
                userAchievements = localAndDatabaseInstances.Where(ai => ai.user_id == user.id).ToList();
                if(partOfQuest)
                    _unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(achievementID, user, userAchievements, false);
                CheckRingSystemAchievements(user.id, userAchievements);
                localAndDatabaseInstances = localAndDatabaseInstances.Except(userAchievements).ToList();
                userAchievements.Clear();
            }
            Save();
        }

        //TODO: MAKE THIS PRETTY AND NOT SUPER SLOW
        public void RevokeAchievement(int instanceID)
        {
            achievement_instance instanceToRevoke = _dbContext.achievement_instance.Find(instanceID);
            if (instanceToRevoke == null)
                return;
            achievement_template achievementTemplate = _dbContext.achievement_template.Find(instanceToRevoke.achievement_id);
            user user = _dbContext.user.Find(instanceToRevoke.user_id);
            achievement_user_content userContent = null;
            achievement_user_story userStory = null;
            
            //TODO: ADD COMMENT LOCATIONS TO CONSTANTS
            List<comment> comments = _dbContext.comment.Where(c => c.location_type == 0 && c.location_id == instanceID).ToList();

            //Get the user content and story if they exist
            if (instanceToRevoke.has_user_content)
                userContent = _dbContext.achievement_user_content.Find(instanceToRevoke.user_content_id);
            if (instanceToRevoke.has_user_story)
                userStory = _dbContext.achievement_user_story.Find(instanceToRevoke.user_story_id);

            //Get rid of threshold achievements if they no longer qualify
            if (achievementTemplate.is_repeatable)
            {
                List<achievement_instance> instancesOfAchievement = _dbContext.achievement_instance.Where(ai => ai.achievement_id == instanceToRevoke.achievement_id && ai.user_id == instanceToRevoke.user_id).ToList();
                if (instancesOfAchievement[0] == instanceToRevoke && instancesOfAchievement.Count > 1)
                {
                    instancesOfAchievement[1].points_create = instancesOfAchievement[0].points_create;
                    instancesOfAchievement[1].points_explore = instancesOfAchievement[0].points_explore;
                    instancesOfAchievement[1].points_learn = instancesOfAchievement[0].points_learn;
                    instancesOfAchievement[1].points_socialize = instancesOfAchievement[0].points_socialize;
                }
                List<achievement_template> thresholdAchievements = _dbContext.achievement_template.Where(at => at.parent_id == instanceToRevoke.achievement_id).ToList();
                achievement_instance thresholdInstance = null;
                foreach (achievement_template thresholdTemplate in thresholdAchievements)
                {
                    thresholdInstance = _dbContext.achievement_instance.SingleOrDefault(ai => ai.achievement_id == thresholdTemplate.id && ai.user_id == instanceToRevoke.user_id);

                    //Check the instance vs the threshold for it, minus 1 for the instance currently being removed
                    if (thresholdInstance != null && thresholdTemplate.threshold > instancesOfAchievement.Count - 1)
                    {
                        RevokeAchievement(thresholdInstance.id);
                    }
                }
            }

            //Delete Associated Images
            if(userContent != null && userContent.image != null && System.IO.File.Exists(userContent.image))
                System.IO.File.Delete(userContent.image);
            if (userStory != null && userStory.image != null && System.IO.File.Exists(userStory.image))
                System.IO.File.Delete(userStory.image);

            //Remove the achievement instance from the database
            _dbContext.achievement_instance.Remove(instanceToRevoke);
            if (userContent != null)
                _dbContext.achievement_user_content.Remove(userContent);
            if (userStory != null)
                _dbContext.achievement_user_story.Remove(userStory);
            //TODO: REMOVE COMMENTS           

            Save();
            //TODO: FIX THIS TO PASS IN LIST OF ACHIEVEMENTS
            _unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(achievementTemplate.id, user, null, true, true);
 
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

        public Boolean UserSubmittedContentForImage(int achievementID, int userID, string imageLocation, string text)
        {
            achievement_template achievementTemplate = _dbContext.achievement_template.Find(achievementID);

            if (achievementTemplate == null)
                return false;
            if (achievementTemplate.content_type == null || achievementTemplate.content_type != (int)JPPConstants.UserSubmissionTypes.Image)
                return false;
            if (_dbContext.achievement_instance.Any(ai => ai.achievement_id == achievementID && ai.user_id == userID) || _dbContext.achievement_user_content_pending.Any(aucp => aucp.achievement_id == achievementID && aucp.submitted_by_id == userID))
                return false;

            achievement_user_content_pending newUserContentPending = new achievement_user_content_pending()
            {
                achievement_id = achievementID,
                content_type = (int)JPPConstants.UserSubmissionTypes.Image,
                image = imageLocation,
                submitted_by_id = userID,
                submitted_date = DateTime.Now,
                text = text
            };

            _dbContext.achievement_user_content_pending.Add(newUserContentPending);
            Save();
            return true;
        }

        public Boolean UserSubmittedContentForText(int achievementID, int userID, String text)
        {
            achievement_template achievementTemplate = _dbContext.achievement_template.Find(achievementID);

            if (achievementTemplate == null)
                return false;
            if (achievementTemplate.content_type == null || achievementTemplate.content_type != (int)JPPConstants.UserSubmissionTypes.Text)
                return false;
            if (_dbContext.achievement_instance.Any(ai => ai.achievement_id == achievementID && ai.user_id == userID) || _dbContext.achievement_user_content_pending.Any(aucp => aucp.achievement_id == achievementID && aucp.submitted_by_id == userID))
                return false;

            achievement_user_content_pending newUserContentPending = new achievement_user_content_pending()
            {
                achievement_id = achievementID,
                content_type = (int)JPPConstants.UserSubmissionTypes.Text,
                submitted_by_id = userID,
                submitted_date = DateTime.Now,
                text = text
            };

            _dbContext.achievement_user_content_pending.Add(newUserContentPending);
            Save();

            return true;
        }

        public Boolean UserSubmittedContentForURL(int achievementID, int userID, String text, String url)
        {
            achievement_template achievementTemplate = _dbContext.achievement_template.Find(achievementID);

            if (achievementTemplate == null)
                return false;
            if (achievementTemplate.content_type == null || achievementTemplate.content_type != (int)JPPConstants.UserSubmissionTypes.URL)
                return false;
            if (_dbContext.achievement_instance.Any(ai => ai.achievement_id == achievementID && ai.user_id == userID) || _dbContext.achievement_user_content_pending.Any(aucp => aucp.achievement_id == achievementID && aucp.submitted_by_id == userID))
                return false;

            achievement_user_content_pending newUserContentPending = new achievement_user_content_pending()
            {
                achievement_id = achievementID,
                content_type = (int)JPPConstants.UserSubmissionTypes.URL,
                submitted_by_id = userID,
                submitted_date = DateTime.Now,
                text = text,
                url = url
            };

            _dbContext.achievement_user_content_pending.Add(newUserContentPending);
            Save();
            return true;
        }

        /// <summary>
        /// Adds or Edits an Image for a User's story
        /// </summary>
        /// <param name="instanceID">Achievement Instance</param>
        /// <param name="imagePath">Filepath of the new Image</param>
        public Boolean UserAddAchievementStoryImage(int instanceID, String imagePath)
        {
            //Get the achievement instance, if it doesn't exist, don't continue
            achievement_instance instance = _dbContext.achievement_instance.Find(instanceID);
            if (instance == null)
                return false;

            //Set up the user story
            achievement_user_story userStory = null;
            //If the instance has a story already, get it and set userStory equal to it
            if (instance.has_user_story)
                userStory = _dbContext.achievement_user_story.Find(instance.user_story_id);
            //Make sure the userStory isn't null and then set the image equal to the new imagePath
            if (userStory != null)
                userStory.image = imagePath;
            else
            {
                //userStory was null create one and add it to the database
                userStory = new achievement_user_story()
                {
                    date_submitted = DateTime.Now,
                    image = imagePath
                };

                _dbContext.achievement_user_story.Add(userStory);
                //Update the instance to include the user story
                instance.has_user_story = true;
                instance.user_story_id = userStory.id;
            }
            Save();
            return true;
        }

        public Boolean UserAddAchievementStoryText(int instanceID, String text)
        {
            //Get the achievement instance, if it doesn't exist, don't continue
            achievement_instance instance = _dbContext.achievement_instance.Find(instanceID);
            if (instance == null)
                return false;

            //Set up the user story
            achievement_user_story userStory = null;
            //If the instance has a story already, get it and set userStory equal to it
            if (instance.has_user_story)
                userStory = _dbContext.achievement_user_story.Find(instance.user_story_id);
            //Make sure the userStory isn't null and then set the text equal to the new text
            if (userStory != null)
                userStory.text = text;
            else
            {
                //userStory was null create one and add it to the database
                userStory = new achievement_user_story()
                {
                    date_submitted = DateTime.Now,
                    text = text
                };

                _dbContext.achievement_user_story.Add(userStory);
                //Update the instance to include the user story
                instance.has_user_story = true;
                instance.user_story_id = userStory.id;
            }
            Save();
            return true;
        }
            


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

        //TODO: DOUBLE CHECK VARIABLE NAMES TO MAKE SURE COPY and PASTED CODE WAS RENAMED (BEN)
        //TODO: CHECK FOR REVOKE
        /// <summary>
        /// Checks for Ring_x4, Ring_x25, and Ring_x100 System Achievements
        /// (User gets 4 points in each quadrant, 25 points in each quadrant, 100 points in each quadrant)
        /// </summary>
        /// <param name="userID"></param>
        public void CheckRingSystemAchievements(int userID, List<achievement_instance> userAchievements = null)
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

            if(userAchievements == null)
                userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == userID).ToList();

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
                            AssignAchievement(userID, ringAchievement.id, null, false);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Ring_x25):
                        if (totalPointsCreate >= 25 && totalPointsExplore >= 25 && totalPointsLearn >= 25 && totalPointsSocialize >= 25)
                        {
                            AssignAchievement(userID, ringAchievement.id, null, false);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Ring_x100):
                        if (totalPointsCreate >= 100 && totalPointsExplore >= 100 && totalPointsLearn >= 100 && totalPointsSocialize >= 100)
                        {
                            AssignAchievement(userID, ringAchievement.id, null, false);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Checks for Six Word Bio System Achievement
        /// (User add a Six Word Bio for the first time)
        /// </summary>
        /// <param name="userID"></param>
        public void CheckSixWordBioSystemAchievements(int userID)
        {
            achievement_template sixWordBioSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.SixWordBio);

            if (sixWordBioSystemAchievement == null || _dbContext.achievement_instance.Any(ai => ai.achievement_id == sixWordBioSystemAchievement.id && ai.user_id == userID))
                return;

            AssignAchievement(userID, sixWordBioSystemAchievement.id);
        }

        /// <summary>
        /// Check Profile Picture System Achievement
        /// (User adds a profile picture for the first time)
        /// </summary>
        /// <param name="userID"></param>
        public void CheckProfilePictureSystemAchievement(int userID)
        {
            achievement_template profilePictureSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.ProfilePic);

            if (profilePictureSystemAchievement == null || _dbContext.achievement_instance.Any(ai => ai.achievement_id == profilePictureSystemAchievement.id && ai.user_id == userID))
                return;

            AssignAchievement(userID, profilePictureSystemAchievement.id);
        }

        /// <summary>
        /// Checks for Friends_x1, Friends_x10, and Friends_x25 System Achievements
        /// (User acquires 1 Friend, acquires 10 friends, acquires 25 friends)
        /// </summary>
        /// <param name="userID"></param>
        public void CheckFriendSystemAchievements(int userID)
        {
            List<achievement_template> friendAchievementsList = _dbContext.achievement_template.Where(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Friends_x1 ||
                                                              at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Friends_x10 ||
                                                              at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.Friends_x25 &&
                                                              at.state == (int)JPPConstants.AchievementQuestStates.Active).ToList();

            if (friendAchievementsList == null || friendAchievementsList.Count <= 0)
            {
                //None of the Friend Achievements have been made or are inactive so they can't be awarded
                return;
            }

            List<friend> friendsList = _dbContext.friend.Where(f => f.source_id == userID).ToList();

            foreach (achievement_template friendAchievement in friendAchievementsList)
            {
                if (_dbContext.achievement_instance.Any(ua => ua.achievement_id == friendAchievement.id && ua.user_id == userID))
                    continue;

                switch (friendAchievement.system_trigger_type)
                {
                    case ((int)JPPConstants.SystemAchievementTypes.Friends_x1):
                        if (friendsList != null && friendsList.Count >= 1)
                        {
                            AssignAchievement(userID, friendAchievement.id);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Friends_x10):
                        if (friendsList != null && friendsList.Count >= 10)
                        {
                            AssignAchievement(userID, friendAchievement.id);
                        }
                        break;

                    case ((int)JPPConstants.SystemAchievementTypes.Friends_x25):
                        if (friendsList != null && friendsList.Count >= 25)
                        {
                            AssignAchievement(userID, friendAchievement.id);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Checks for One-K and Ten-K System Achievements
        /// (1000 and 10000 achievements systemwide)
        /// </summary>
        public void CheckOneKAndTenKSystemAchievements()
        {
            achievement_template oneKSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.OneK && at.state == (int)JPPConstants.AchievementQuestStates.Active);
            achievement_template tenKSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.TenK && at.state == (int)JPPConstants.AchievementQuestStates.Active);
            int numberOfAchievements = _dbContext.achievement_instance.Count();

            //Check if the achievement exists and is active, and if the threshold is met
            if (oneKSystemAchievement != null && numberOfAchievements >= 1000)
            {
                oneKSystemAchievement.state = (int)JPPConstants.AchievementQuestStates.Retired;
                Save();
                if (!_dbContext.achievement_instance.Any(ai => ai.achievement_id == oneKSystemAchievement.id))
                {
                    //Task.Run(() =>
                    //{
                    AssignGlobalAchievement(oneKSystemAchievement.id, DateTime.MinValue, DateTime.Now);
                    //});
                }
            }

            if (tenKSystemAchievement != null && numberOfAchievements >= 10000)
            {
                tenKSystemAchievement.state = (int)JPPConstants.AchievementQuestStates.Retired;
                Save();
                if (!_dbContext.achievement_instance.Any(ai => ai.achievement_id == tenKSystemAchievement.id))
                    AssignGlobalAchievement(tenKSystemAchievement.id, DateTime.MinValue, DateTime.Now);
            }


        }

        /// <summary>
        /// Checks for Public Profile System Achievement 
        /// (User changes privacy setting to "Public")
        /// </summary>
        /// <param name="userID"></param>
        public void CheckPublicProfileSystemAchievement(int userID)
        {
            achievement_template publicProfileSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.PublicProfile);

            if (publicProfileSystemAchievement == null || _dbContext.achievement_instance.Any(ai => ai.achievement_id == publicProfileSystemAchievement.id && ai.user_id == userID))
                return;

            AssignAchievement(userID, publicProfileSystemAchievement.id);
        }

        /// <summary>
        /// Checks for Facebook Link System Achievement
        /// (User connects their JPP account with their Facebook account)
        /// </summary>
        /// <param name="userID"></param>
        public void CheckFacebookLinkSystemAchievement(int userID)
        {
            achievement_template facebookLinkSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.FacebookLink);

            if (facebookLinkSystemAchievement == null || _dbContext.achievement_instance.Any(ai => ai.achievement_id == facebookLinkSystemAchievement.id && ai.user_id == userID))
                return;

            AssignAchievement(userID, facebookLinkSystemAchievement.id);
        }

        #endregion

    }

}