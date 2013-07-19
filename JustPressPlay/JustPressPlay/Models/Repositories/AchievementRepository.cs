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

        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Insert/Delete-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Insert/Delete

        /// <summary>
        /// Adds the achievement and associated requirements and possible caretakers to the database
        /// </summary>
        /// <param name="achievementTemplate">Template to the achievement to add to the database</param>
        /// <param name="requirementsList">List of requirements for the achievement</param>
        /// <param name="caretakersList">List of caretakers for an achievement (can be null if the achievement type is not scan)</param>
        private void AddAchievementToDatabase(achievement_template achievementTemplate, List<achievement_requirement> requirementsList, List<achievement_caretaker> caretakersList)
        {
            //add the achievement first
            if (achievementTemplate != null)
                _dbContext.achievement_template.Add(achievementTemplate);

            //add all the requirements
            foreach (achievement_requirement requirement in requirementsList)
                _dbContext.achievement_requirement.Add(requirement);
            
            //add any caretakers if need be
            if (caretakersList != null)
                foreach (achievement_caretaker caretaker in caretakersList)
                    _dbContext.achievement_caretaker.Add(caretaker);
        }

        //-----Admin Insert/Delete-----//
        #region Admin Insert/Delete

        public string AdminAddAchievement(AddAchievementViewModel model)
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
                state = 1, //NEED TO MAKE CONSTANSTS FOR ACHIEVEMENT STATE
                parent_id = model.ParentID,
                threshold = model.Threshold,
                creator_id = model.CreatorID,
                created_date = DateTime.Now,
                posted_date = null,
                retire_date = "", //Need to fix this in db - needs to be datetime and nullable
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

            AddAchievementToDatabase(newAchievement, requirementsList, caretakersList);

            Save();

            return "";
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