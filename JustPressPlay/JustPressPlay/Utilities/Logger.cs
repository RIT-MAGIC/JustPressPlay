using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using JustPressPlay.Models;

namespace JustPressPlay.Utilities
{
    public class Logger
    {

        public enum LogIDType
        {
            Admin,
            User,
            AchievementTemplate,
            QuestTemplate,
            QuestInstance,
            UserStory,
            UserContent,
            Comment

        }

        //DONE
        public enum AchievementInstanceLogType
        {
            CardGiven, //Done
            GlobalAssigned, //DONE
            AchievementRevoked //DONE
        }

        //DONE
        public enum QuestInstanceLogType
        {
            QuestUnlocked, //DONE
            QuestRevoked //DONE
        }

        //DONE
        public enum CommentBehaviorLogType
        {
            CommentDelete,//DONE
            CommentEdit //DONE
        }

        public enum EditProfileContentLogType
        {
            DisplayNameEdit,//DONE
            FullBioEdit, //DONE
            ProfilePictureEdit, //DONE
            SixWordBioEdit, //DONE
            EmailEdit, //DONE
            FirstNameEdit, //DONE
            MiddleNameEdit, //DONE
            LastNameEdit, //DONE
            IsPlayerEdit //DONE
        }

        public enum EditProfileSettingsLogType
        {
            Privacy,
            Facebook,
            Password,
            Communications,
            LeftGame
        }

        //DONE
        public enum PlayerFriendLogType
        {
            AddFriend, //DONE
            AcceptRequest, //DONE
            DeclineRequest, //DONE
            IgnoreRequest, //DONE
            RemoveFriend //DONE
        }

        public enum UserStoryLogType
        {
            AddStoryImage,//DONE
            AddStoryText,//DONE
            EditStoryImage,
            EditStoryText
        }

        //DONE
        public enum EditAchievementLogType
        {
            Title, //DONE
            Description, //DONE
            Icon, //DONE
            Type, //DONE
            Hidden, //DONE
            IsRepeatable, //DONE
            State, //DONE
            ParentID, //DONE
            Threshold, //DONE
            ContentType, //DONE
            SystemTriggerType, //DONE
            RepeatDelayDays, //DONE
            PointsCreate, //DONE
            PointsExplore, //DONE
            PointsLearn, //DONE
            PointsSocialize //DONE
        }

        //DONE
        public enum EditQuestLogType
        {
            Title, //DONE
            Description, //DONE
            Icon, //DONE
            State, //DONE
            Threshold //DONE
        }

        public enum ManageSubmissionsLogType
        {
            DeniedUserQuest, //DONE
            DeniedContentSubmission,//DONE
            EditContentSubmission
        }

        public static void LogSingleEntry(LoggerModel loggerModel, JustPressPlayDBEntities _dbContext, bool autoSave = false)
        {
            log newLogEntry = new log()
            {
                action = loggerModel.Action,
                ip_address = loggerModel.IPAddress,
                id_type_1 = loggerModel.IDType1 == null ? null : loggerModel.IDType1,
                id_type_2 = loggerModel.IDType2 == null ? null : loggerModel.IDType2,
                id_1 = loggerModel.ID1 == null ? null : loggerModel.ID1,
                id_2 = loggerModel.ID2 == null ? null : loggerModel.ID2,
                timestamp = loggerModel.TimeStamp,
                user_id = loggerModel.UserID,
                value_1 = loggerModel.Value1 == null? null : loggerModel.Value1,
                value_2 = loggerModel.Value2 == null? null : loggerModel.Value2,
            };

            _dbContext.log.Add(newLogEntry);

            if (autoSave)
                _dbContext.SaveChanges();
        }

        public static void LogMultipleEntries(List<LoggerModel> loggerModelList, JustPressPlayDBEntities _dbContext)
        {
            foreach (LoggerModel loggerModel in loggerModelList)
            {
                LogSingleEntry(loggerModel, _dbContext);
            }
        }
    }

    public class LoggerModel
    {
        public String Action { get; set; }
        public String IPAddress { get; set; }
        public DateTime TimeStamp { get; set; }
        public String IDType1 { get; set; }
        public String IDType2 { get; set; }
        public int? ID1 { get; set; }
        public int? ID2 { get; set; }
        public int UserID { get; set; }
        public String Value1 { get; set; }
        public String Value2 { get; set; }
    }
}