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
using Facebook;
using WebMatrix.WebData;
using System.Data.Objects;

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

        public achievement_instance GetUserAchievementInstance(int achievementID)
        {
            return _dbContext.achievement_instance.First(a => a.id == achievementID);
        }

		public achievement_instance GetUserAchievementInstance(int userId, int achievementId)
		{
			return _dbContext.achievement_instance.First(a => (a.user_id == userId && a.achievement_id == achievementId));
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
				if (caretakerEntry.achievement_template.type == (int)JPPConstants.AchievementTypes.Scan)
					templateList.Add(caretakerEntry.achievement_template);
			}
			return templateList;
		}

        public List<JustPressPlay.Utilities.JPPNewsFeed> GetAchievementsForFeed()
        {
            List<achievement_template> dbAchievementsList = _dbContext.achievement_template.Where(n => n.featured == true).ToList();
            List<JustPressPlay.Utilities.JPPNewsFeed> newsList = new List<Utilities.JPPNewsFeed>();

            foreach (achievement_template at in dbAchievementsList)
            {
                newsList.Add(new Utilities.JPPNewsFeed()
                {
                    Controller = JustPressPlay.Utilities.JPPConstants.FeaturedControllerType.Achievements.ToString(),
                    Action = JustPressPlay.Utilities.JPPConstants.FeaturedActionType.IndividualAchievement.ToString(),
                    ID = at.id,
                    Icon = at.icon,
                    Title = at.title,
                    Text = at.description
                });

            }

            return newsList;

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
				icon_file_name = model.Icon,
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
            if (!String.IsNullOrWhiteSpace(model.Requirement1))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement1 });
            if (!String.IsNullOrWhiteSpace(model.Requirement2))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement2 });
            if (!String.IsNullOrWhiteSpace(model.Requirement3))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement3 });
            if (!String.IsNullOrWhiteSpace(model.Requirement4))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement4 });
            if (!String.IsNullOrWhiteSpace(model.Requirement5))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement5 });
            if (!String.IsNullOrWhiteSpace(model.Requirement6))
                requirementsList.Add(new achievement_requirement { achievement_id = newAchievement.id, description = model.Requirement6 });


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
			List<LoggerModel> logChanges = new List<LoggerModel>();
			//Create all the requirements for the achievement to be added to the database
			List<achievement_requirement> requirementsList = new List<achievement_requirement>();
            if (!String.IsNullOrWhiteSpace(model.Requirement1))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement1 });
            if (!String.IsNullOrWhiteSpace(model.Requirement2))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement2 });
            if (!String.IsNullOrWhiteSpace(model.Requirement3))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement3 });
            if (!String.IsNullOrWhiteSpace(model.Requirement4))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement4 });
            if (!String.IsNullOrWhiteSpace(model.Requirement5))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement5 });
            if (!String.IsNullOrWhiteSpace(model.Requirement6))
                requirementsList.Add(new achievement_requirement { achievement_id = currentAchievement.id, description = model.Requirement6 });

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
			#region//ContentType
			if (model.ContentType != currentAchievement.content_type)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.ContentType.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.content_type.ToString(),
					Value2 = model.ContentType.ToString()
				});
				currentAchievement.content_type = model.ContentType;
			}
			#endregion

			#region//Description
			if (!String.IsNullOrWhiteSpace(model.Description) && !String.Equals(currentAchievement.description, model.Description))
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Description.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.description,
					Value2 = model.Description
				});
				currentAchievement.description = model.Description;
			}
			#endregion

			#region//Hidden
			if (currentAchievement.hidden != model.Hidden)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Hidden.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.hidden.ToString(),
					Value2 = model.Hidden.ToString()
				});
				currentAchievement.hidden = model.Hidden;
			}
			#endregion

			#region//Base Icon
			if (!String.IsNullOrWhiteSpace(model.Icon) && !String.Equals(currentAchievement.icon_file_name, model.Icon))
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Icon.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.icon_file_name.ToString(),
					Value2 = model.Icon.ToString()
				});
				currentAchievement.icon = model.IconFilePath;
				currentAchievement.icon_file_name = model.Icon;
			}
			#endregion

            #region//Icon File Path
            if (!String.IsNullOrWhiteSpace(model.IconFilePath) && !String.Equals(currentAchievement.icon, model.IconFilePath))
            {
                logChanges.Add(new LoggerModel()
                {
                    Action = "Edit Achievement: " + Logger.EditAchievementLogType.Icon.ToString(),
                    UserID = model.EditorID,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
                    ID1 = id,
                    Value1 = currentAchievement.icon.ToString(),
                    Value2 = model.IconFilePath.ToString()
                });
                currentAchievement.icon = model.IconFilePath;
                currentAchievement.icon_file_name = model.Icon;
            }
            #endregion

			#region//IsRepeatable
			if (currentAchievement.is_repeatable != model.IsRepeatable)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.IsRepeatable.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.is_repeatable.ToString(),
					Value2 = model.IsRepeatable.ToString()
				});
				currentAchievement.is_repeatable = model.IsRepeatable;
			}
			#endregion

			#region//Last Modified By (userID and DateTime)
			currentAchievement.last_modified_by_id = model.EditorID;
			currentAchievement.modified_date = DateTime.Now;
			#endregion

			#region//Parent Achievement ID
			if (currentAchievement.parent_id != model.ParentID)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.ParentID.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.parent_id.ToString(),
					Value2 = model.ParentID.ToString()
				});
				currentAchievement.parent_id = model.ParentID;
			}
			#endregion

			#region//Points Create
			if (currentAchievement.points_create != model.PointsCreate)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.PointsCreate.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.points_create.ToString(),
					Value2 = model.PointsCreate.ToString()
				});
				currentAchievement.points_create = model.PointsCreate;
			}
			#endregion

			#region//Points Explore
			if (currentAchievement.points_explore != model.PointsExplore)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.PointsExplore.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.points_explore.ToString(),
					Value2 = model.PointsExplore.ToString()
				});
				currentAchievement.points_explore = model.PointsExplore;
			}
			#endregion

			#region//Points Learn
			if (currentAchievement.points_learn != model.PointsLearn)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.PointsLearn.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.points_learn.ToString(),
					Value2 = model.PointsLearn.ToString()
				});
				currentAchievement.points_learn = model.PointsLearn;
			}
			#endregion

			#region//Points Socialize
			if (currentAchievement.points_socialize != model.PointsSocialize)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.PointsSocialize.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.points_socialize.ToString(),
					Value2 = model.PointsSocialize.ToString()
				});
				currentAchievement.points_socialize = model.PointsSocialize;
			}
			#endregion

			#region//Posted Date
			if (currentAchievement.state != model.State && model.State == (int)JPPConstants.AchievementQuestStates.Active && currentAchievement.posted_date == null)
				currentAchievement.posted_date = DateTime.Now;
			#endregion

			#region//Repeat Delay Days
			if (currentAchievement.repeat_delay_days != model.RepeatDelayDays)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.RepeatDelayDays.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.repeat_delay_days.ToString(),
					Value2 = model.RepeatDelayDays.ToString()
				});
				currentAchievement.repeat_delay_days = model.RepeatDelayDays;
			}
			#endregion

			#region//Retire Date
			if (currentAchievement.state != model.State && model.State == (int)JPPConstants.AchievementQuestStates.Retired && currentAchievement.retire_date == null)
				currentAchievement.retire_date = DateTime.Now;
			if (currentAchievement.state != model.State && currentAchievement.state == (int)JPPConstants.AchievementQuestStates.Retired)
				currentAchievement.retire_date = null;
			#endregion

			#region//Achievement State
			if (currentAchievement.state != model.State)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.State.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.state.ToString(),
					Value2 = model.State.ToString()
				});
				currentAchievement.state = model.State;
			}
			#endregion

			#region//Featured
			if (currentAchievement.state != (int)JPPConstants.AchievementQuestStates.Active)
				currentAchievement.featured = false;
			#endregion

			#region//System Trigger Type
			if (currentAchievement.system_trigger_type != model.SystemTriggerType)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.SystemTriggerType.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.system_trigger_type.ToString(),
					Value2 = model.SystemTriggerType.ToString()
				});
				currentAchievement.system_trigger_type = model.SystemTriggerType;
			}
			#endregion

			#region//Threshold
			if (currentAchievement.threshold != model.Threshold)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Threshold.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.threshold.ToString(),
					Value2 = model.Threshold.ToString()
				});
				currentAchievement.threshold = model.Threshold;
			}
			#endregion

			#region//Title
			if (!String.IsNullOrWhiteSpace(model.Title) && !String.Equals(currentAchievement.title, model.Title))
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Title.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.title.ToString(),
					Value2 = model.Title.ToString()
				});
				currentAchievement.title = model.Title;
			}
			#endregion

			#region//Type
			if (currentAchievement.type != model.Type)
			{
				logChanges.Add(new LoggerModel()
				{
					Action = "Edit Achievement: " + Logger.EditAchievementLogType.Type.ToString(),
					UserID = model.EditorID,
					IPAddress = HttpContext.Current.Request.UserHostAddress,
					TimeStamp = DateTime.Now,
					IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					ID1 = id,
					Value1 = currentAchievement.type.ToString(),
					Value2 = model.Type.ToString()
				});
				currentAchievement.type = model.Type;
			}
			#endregion

			if (logChanges.Count > 0)
				Logger.LogMultipleEntries(logChanges, _dbContext);

			AddRequirementsToDatabase(requirementsList);
			AddCaretakersToDatabase(caretakersList);

			Save();
		}

        public String DiscardAchievementDraft(int id)
        {
            achievement_template t = _unitOfWork.EntityContext.achievement_template.Find(id);
            var title = t.title;
            if (t == null)
                return String.Empty;

            if (t.state != (int)JPPConstants.AchievementQuestStates.Draft)
                return String.Empty;


            var discardReq = _dbContext.achievement_requirement.Where(a => a.achievement_id == t.id).ToList();
            foreach (achievement_requirement req in discardReq)
            {
                _dbContext.achievement_requirement.Remove(req);
            }

            var discardCt = _dbContext.achievement_caretaker.Where(a => a.achievement_id == t.id).ToList();
            foreach (achievement_caretaker ct in discardCt)
            {
                _dbContext.achievement_caretaker.Remove(ct);
            }

            _dbContext.achievement_template.Remove(t);
            Save();
            return "The Draft for "+title+" was successfully discarded";
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
				achieved_date = dateAssigned == null ? DateTime.Now : (DateTime)dateAssigned,
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
				) + "#" + newInstance.id,
				false);

			#region Facebook Sharing
			bool facebookEnabledOnSite = bool.Parse(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookIntegrationEnabled));
			if (facebookEnabledOnSite)
			{
				facebook_connection fbConnectionData = _unitOfWork.UserRepository.GetUserFacebookSettingsById(userID);
				if (fbConnectionData != null)
				{
					string appNamespace = JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.FacebookAppNamespace);
					UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
					string achievementUri = JppUriInfo.GetAbsoluteUri(new HttpRequestWrapper(HttpContext.Current.Request),
						urlHelper.RouteUrl("AchievementsPlayersRoute", new { id = achievementID })
						);
					string relativeEarnedAchievementUri = urlHelper.RouteUrl("AchievementsPlayersRoute", new { id = achievementID, playerID = userID });

					try
					{
						FacebookClient fbClient = new FacebookClient();

						// Cannot send notifications unless we're a canvas app. Code implemented,
						// but will return an OAuth error
						/*
						if (fbConnectionData.notifications_enabled)
						{
							string appAccessToken = JppFacebookHelper.GetAppAccessToken(fbClient);

							fbClient.Post("/" + fbConnectionData.facebook_user_id + "/notifications", new
							{
								access_token = appAccessToken,
								template = JPPConstants.GetFacebookNotificationMessage(template.title),
								href = VirtualPathUtility.ToAbsolute(relativeEarnedAchievementUri),
							});
						}//*/

						if (fbConnectionData.automatic_sharing_enabled)
						{
							fbClient.Post("/me/" + appNamespace + ":earn", new
							{
								access_token = fbConnectionData.access_token,
								achievement = achievementUri
							});
						}
					}
					catch (FacebookOAuthException e)
					{
						// TODO: log FB error
					}
				}
			}
			#endregion

			JPPConstants.AssignAchievementResult result = JPPConstants.AssignAchievementResult.Success;

			if (Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.CardDistributionEnabled)))
			{
				if (cardGiven)
				{
					result = JPPConstants.AssignAchievementResult.SuccessYesCard;
					LoggerModel logCard = new LoggerModel()
					{
						Action = Logger.AchievementInstanceLogType.CardGiven.ToString(),
						IPAddress = HttpContext.Current.Request.UserHostAddress,
						UserID = newInstance.assigned_by_id,
						TimeStamp = (DateTime)newInstance.card_given_date,
						ID1 = newInstance.achievement_id,
						IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
					};
					Logger.LogSingleEntry(logCard, _dbContext);
				}
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
				List<achievement_instance> userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == userID).ToList().Union(_dbContext.achievement_instance.Local.Where(ai => ai.user_id == userID).ToList()).ToList();
				_unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(achievementID, user, userAchievements, autoSave);
				CheckRingSystemAchievements(userID, userAchievements);
				CheckOneKAndTenKSystemAchievements(userID);
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
		/// TODO: CHECK THE LOGIC TO MAKE SURE IT ALL WORKS THE WAY IT SHOULD
		/// </summary>
		private void AssignContentSubmissionAchievement(int approvedByID, achievement_user_content_pending pendingContent)
		{
			//Assign the achievement
			var test = AssignAchievement(pendingContent.submitted_by_id, pendingContent.achievement_id, approvedByID);
			//Get the newly assigned achievement
			achievement_instance newInstance = _dbContext.achievement_instance.SingleOrDefault(ai => ai.user_id == pendingContent.submitted_by_id && ai.achievement_id == pendingContent.achievement_id);
			//Create the user content to be added
			achievement_user_content newUserContent = new achievement_user_content()
			{
				approved_by_id = approvedByID,
				approved_date = DateTime.Now,
				content_type = pendingContent.content_type,
				image = pendingContent.content_type == (int)JPPConstants.UserSubmissionTypes.Image ? pendingContent.image : null,
				submitted_date = pendingContent.submitted_date,
				text = pendingContent.text,
				url = pendingContent.content_type == (int)JPPConstants.UserSubmissionTypes.URL ? pendingContent.url : null
			};

            achievement_user_story newuserStory = new achievement_user_story()
            {
                image = pendingContent.content_type == (int)JPPConstants.UserSubmissionTypes.Image ? pendingContent.image : null,
                text = pendingContent.text,
                date_submitted = DateTime.Now
            };

			//Add the new user content to the database
			_dbContext.achievement_user_content.Add(newUserContent);
            _dbContext.achievement_user_story.Add(newuserStory);
			//append the instance to point to the new user content
			newInstance.has_user_content = true;
			newInstance.user_content_id = newUserContent.id;
            newInstance.has_user_story = true;
            newInstance.user_story_id = newuserStory.id;
			//Remove the content from the pending list
			_dbContext.achievement_user_content_pending.Remove(pendingContent);
			//Save changes
			Save();
		}

		public void HandleContentSubmission(int contentID, JPPConstants.HandleUserContent handleContent, string reason = null)
		{
			achievement_user_content_pending pendingContent = _dbContext.achievement_user_content_pending.Find(contentID);
			if (pendingContent == null)
				return;

			switch (handleContent)
			{
				case JPPConstants.HandleUserContent.Approve:
					AssignContentSubmissionAchievement(WebSecurity.CurrentUserId, pendingContent);
					return;
				case JPPConstants.HandleUserContent.Deny:
					if (!String.IsNullOrWhiteSpace(reason))
						DenyContentSubmission(pendingContent, reason);
					return;
				default:
					return;
			}
		}

		private void DenyContentSubmission(achievement_user_content_pending pendingContent, string reason)
		{
			_unitOfWork.SystemRepository.AddNotification(
				pendingContent.submitted_by_id,
				WebSecurity.CurrentUserId,
				"Your submission for the achievement [" + pendingContent.achievement_template.title + "] was denied for the following reason: " + reason,
				pendingContent.achievement_template.icon, new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
					"IndividualAchievement",
					"Achievements",
					new { id = pendingContent.achievement_id }
				),
				false);

			LoggerModel logSubmissionDeny = new LoggerModel()
			{
				Action = Logger.ManageSubmissionsLogType.DeniedContentSubmission.ToString(),
				UserID = WebSecurity.CurrentUserId,
				IPAddress = HttpContext.Current.Request.UserHostAddress,
				TimeStamp = DateTime.Now,
				ID1 = pendingContent.submitted_by_id,
				IDType1 = Logger.LogIDType.User.ToString(),
				ID2 = pendingContent.achievement_id,
				IDType2 = Logger.LogIDType.AchievementTemplate.ToString(),
				Value1 = reason
			};

			Logger.LogSingleEntry(logSubmissionDeny, _dbContext);

			_dbContext.achievement_user_content_pending.Remove(pendingContent);

			Save();
		}

		/// <summary>
		/// Check to see if an scan achievement instance triggers a threshold achievement
		/// </summary>
		/// <param name="achievementID">ID of the repeatable achievement the user was assigned</param>
		/// <param name="userID">ID of the user that was assigned the achievement</param>
		private bool CheckForThresholdUnlock(int achievementID, int userID)
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
		public void AssignGlobalAchievement(int achievementID, DateTime startRange, DateTime endRange, int assignedByID)
		{
            _unitOfWork.EntityContext.Configuration.AutoDetectChangesEnabled = false;
			// Get the achievement template
			achievement_template template = _dbContext.achievement_template.Find(achievementID);
			if (template == null)
				throw new ArgumentException("Invalid achievement ID");
            
			bool partOfQuest = _dbContext.quest_achievement_step.Any(qas => qas.achievement_id == template.id);            

			List<user> qualifiedUsers = _dbContext.user.Where(u => u.status == (int)JPPConstants.UserStatus.Active && u.is_player == true && EntityFunctions.TruncateTime(u.created_date) >= EntityFunctions.TruncateTime(startRange) && EntityFunctions.TruncateTime(u.created_date) <= EntityFunctions.TruncateTime(endRange)).ToList();
            
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
				if (partOfQuest)
					_unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(achievementID, user, userAchievements, false);
				CheckRingSystemAchievements(user.id, userAchievements);
				localAndDatabaseInstances = localAndDatabaseInstances.Except(userAchievements).ToList();
				userAchievements.Clear();
			}

			#region//Log that this was assigned
			LoggerModel logGlobal = new LoggerModel()
			{
				Action = Logger.AchievementInstanceLogType.GlobalAssigned.ToString(),
				UserID = assignedByID,
				IPAddress = HttpContext.Current.Request.UserHostAddress,
				TimeStamp = DateTime.Now,
				ID1 = achievementID,
				IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
				Value1 = "Assigned to " + qualifiedUsers.Count + " players",
			};
			Logger.LogSingleEntry(logGlobal, _dbContext);
			#endregion
            _unitOfWork.EntityContext.Configuration.AutoDetectChangesEnabled = true;
			Save();

			CheckOneKAndTenKSystemAchievements(assignedByID);
		}

		//TODO: MAKE THIS PRETTY AND NOT SUPER SLOW
		public void RevokeAchievement(int instanceID, string reason, bool autoSave = true, int? adminID = null)
		{
			//Get the instance
			achievement_instance instanceToRevoke = _dbContext.achievement_instance.Find(instanceID);

			if (instanceToRevoke == null)
				return;

			//Get the user
			user user = instanceToRevoke.user;
			//Get all the user's instances, minus the one being removed
			List<achievement_instance> userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == instanceToRevoke.user_id).ToList();
			userAchievements.Remove(instanceToRevoke);

			//Get Content, Story, and Comments for the instance
			achievement_user_content userContent = instanceToRevoke.user_content;
			achievement_user_story userStory = instanceToRevoke.user_story;
			List<comment> comments = _dbContext.comment.Where(c => c.location_type == (int)JPPConstants.CommentLocation.Achievement && c.location_id == instanceID).ToList();

			#region Check Threshold Achievements
			//Get rid of threshold achievements if they no longer qualify
			if (instanceToRevoke.achievement_template.is_repeatable)
			{
				bool revokingFirstInstance = true;
				if (instanceToRevoke.points_create == 0 && instanceToRevoke.points_explore == 0 && instanceToRevoke.points_learn == 0 && instanceToRevoke.points_socialize == 0)
					revokingFirstInstance = false;

				//If the achievement being revoked was the first one earned in a set of repeatables, set the next instance's points equal to the revoked achievement's 
				List<achievement_instance> instancesOfAchievement = userAchievements.Where(ua => ua.achievement_id == instanceToRevoke.achievement_id).ToList();
				if (instancesOfAchievement != null && revokingFirstInstance)
				{
					instancesOfAchievement[0].points_create = instanceToRevoke.points_create;
					instancesOfAchievement[0].points_explore = instanceToRevoke.points_explore;
					instancesOfAchievement[0].points_learn = instanceToRevoke.points_learn;
					instancesOfAchievement[0].points_socialize = instanceToRevoke.points_socialize;
				}

				//Get all the threshold achievements.
				List<achievement_template> thresholdAchievements = _dbContext.achievement_template.Where(at => at.parent_id == instanceToRevoke.achievement_id).ToList();
				achievement_instance thresholdInstance = null;
				if (thresholdAchievements != null)
				{
					foreach (achievement_template thresholdTemplate in thresholdAchievements)
					{
						//Get the instance if it exists
						thresholdInstance = _dbContext.achievement_instance.SingleOrDefault(ai => ai.achievement_id == thresholdTemplate.id && ai.user_id == instanceToRevoke.user_id);
						//Check the instance count vs the threshold for it
						if (thresholdInstance != null && thresholdTemplate.threshold > instancesOfAchievement.Count)
						{
							RevokeAchievement(thresholdInstance.id, "Achievement:" + instanceToRevoke.achievement_id.ToString() + " was revoked.", false);
						}
					}
				}
			}
			#endregion

			#region Log the Achievement Revoke
			LoggerModel logAchievementRevoke = new LoggerModel()
			{
				Action = Logger.AchievementInstanceLogType.AchievementRevoked.ToString(),
				UserID = user.id,
				IPAddress = HttpContext.Current.Request.UserHostAddress,
				TimeStamp = DateTime.Now,
				ID1 = instanceToRevoke.achievement_id,
				IDType1 = Logger.LogIDType.AchievementTemplate.ToString(),
				ID2 = adminID == null ? null : adminID,
				IDType2 = adminID == null ? null : Logger.LogIDType.Admin.ToString(),
				Value1 = reason
			};
			Logger.LogSingleEntry(logAchievementRevoke, _dbContext);
			#endregion

            _unitOfWork.SystemRepository.AddNotification(
                user.id,
                WebSecurity.CurrentUserId,
                "Your achievement [" + instanceToRevoke.achievement_template.title + "] was revoked for the following reason: " + reason,
                instanceToRevoke.achievement_template.icon, new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
                    "IndividualAchievement",
                    "Achievements",
                    new { id = instanceToRevoke.achievement_id }
                ),
                false);

			#region Deletion and Removal
			//Delete Associated Images
			if (userContent != null && userContent.image != null && System.IO.File.Exists(userContent.image))
			{
				System.IO.File.Delete(userContent.image);
				userContent.image = null;
			}
			if (userStory != null && userStory.image != null && System.IO.File.Exists(userStory.image))
			{
				System.IO.File.Delete(userStory.image);
				userStory.image = null;
			}

			//Remove the achievement instance from the database along with the content, story, and comments
			_dbContext.achievement_instance.Remove(instanceToRevoke);
			if (userContent != null)
				_dbContext.achievement_user_content.Remove(userContent);
			if (userStory != null)
				_dbContext.achievement_user_story.Remove(userStory);
			if (comments != null)
			{
				foreach (comment comment in comments)
				{
					_dbContext.comment.Remove(comment);
				}
			}
			#endregion

			//Check for quest revokes
			_unitOfWork.QuestRepository.CheckAssociatedQuestCompletion(instanceToRevoke.achievement_id, user, userAchievements, false, true);

			if (autoSave)
				Save();

		}


		public void AwardCard(achievement_instance instance)
		{
            if (!instance.card_given)
            {
                LoggerModel logCard = new LoggerModel()
                {
                    Action = Logger.AchievementInstanceLogType.CardGiven.ToString(),
                    UserID = WebSecurity.CurrentUserId,
                    IPAddress = HttpContext.Current.Request.UserHostAddress,
                    TimeStamp = DateTime.Now,
                    IDType1 = Logger.LogIDType.User.ToString(),
                    ID1 = instance.user_id,
                    IDType2 = Logger.LogIDType.AchievementTemplate.ToString(),
                    ID2 = instance.achievement_template.id
                };
                instance.card_given_date = DateTime.Now;
                instance.card_given = true;
            }

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


			LoggerModel logUserStoryImage = new LoggerModel()
			{
				Action = Logger.UserStoryLogType.AddStoryImage.ToString(),
				UserID = instance.user_id,
				IPAddress = HttpContext.Current.Request.UserHostAddress,
				TimeStamp = DateTime.Now,
				ID1 = userStory.id,
				IDType1 = Logger.LogIDType.UserStory.ToString(),
				Value1 = imagePath
			};
			Logger.LogSingleEntry(logUserStoryImage, _dbContext);
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
			LoggerModel logUserStoryText = new LoggerModel()
			{
				Action = Logger.UserStoryLogType.AddStoryText.ToString(),
				UserID = instance.user_id,
				IPAddress = HttpContext.Current.Request.UserHostAddress,
				TimeStamp = DateTime.Now,
				ID1 = userStory.id,
				IDType1 = Logger.LogIDType.UserStory.ToString(),
				Value1 = text
			};
			Logger.LogSingleEntry(logUserStoryText, _dbContext);

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

			if (ringAchievementsList == null || ringAchievementsList.Count <= 0)
			{
				//None of the Ring Achievements have been made or are inactive so they can't be awarded
				return;
			}

			if (userAchievements == null)
				userAchievements = _dbContext.achievement_instance.Where(ai => ai.user_id == userID).ToList();

			int totalPointsCreate = userAchievements.Sum(ua => ua.points_create);
			int totalPointsExplore = userAchievements.Sum(ua => ua.points_explore);
			int totalPointsLearn = userAchievements.Sum(ua => ua.points_learn);
			int totalPointsSocialize = userAchievements.Sum(ua => ua.points_socialize);

			foreach (achievement_template ringAchievement in ringAchievementsList)
			{
				if (userAchievements.Any(ua => ua.achievement_id == ringAchievement.id))
					continue;

				switch (ringAchievement.system_trigger_type)
				{
					case ((int)JPPConstants.SystemAchievementTypes.Ring_x4):
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
		public void CheckOneKAndTenKSystemAchievements(int userID)
		{
			achievement_template oneKSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.OneK && at.state == (int)JPPConstants.AchievementQuestStates.Active);
			achievement_template tenKSystemAchievement = _dbContext.achievement_template.SingleOrDefault(at => at.system_trigger_type == (int)JPPConstants.SystemAchievementTypes.TenK && at.state == (int)JPPConstants.AchievementQuestStates.Active);
			int numberOfAchievements = _dbContext.achievement_instance.Count();

			//Check if the achievement exists and is active, and if the threshold is met
			if (oneKSystemAchievement != null && numberOfAchievements >= 1000)
			{
				//oneKSystemAchievement.state = (int)JPPConstants.AchievementQuestStates.Retired;
				//Save();
				if (!_dbContext.achievement_instance.Any(ai => ai.achievement_id == oneKSystemAchievement.id))
				{
					//Task.Run(() =>
					//{
					AssignGlobalAchievement(oneKSystemAchievement.id, DateTime.MinValue, DateTime.Now, userID);

					//});
				}
			}

			if (tenKSystemAchievement != null && numberOfAchievements >= 10000)
			{
				//tenKSystemAchievement.state = (int)JPPConstants.AchievementQuestStates.Retired;
				//Save();
				if (!_dbContext.achievement_instance.Any(ai => ai.achievement_id == tenKSystemAchievement.id))
				{
					AssignGlobalAchievement(tenKSystemAchievement.id, DateTime.MinValue, DateTime.Now, userID);
				}
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