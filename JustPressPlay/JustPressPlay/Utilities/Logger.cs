using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using JustPressPlay.Models;

namespace JustPressPlay.Utilities
{
    public class Logger
    {
        JustPressPlayDBEntities _dbContext = new JustPressPlayDBEntities();

        public enum AchievementInstanceLogType
        {
            CardGiven,
            GlobalAssigned,
            Revoked
        }

        public enum QuestInstanceLogType
        {
            Unlocked,
            Revoked
        }

        public enum CommentBehaviorLogType
        {
            CommentDelete,
            CommentEdit
        }

        public enum EditProfileContentLogType
        {
            DisplayNameEdit,
            FullBioEdit,
            ProfilePictureEdit,
            SixWordBioEdit
        }

        public enum EditProfileSettingsLogType
        {
            Privacy,
            Facebook,
            Password,
            Communications,
            LeftGame
        }

        public enum PlayerFriendLogType
        {
            AddFriend,
            AcceptRequest,
            HideRequest,
            IgnoreRequest,
            RemoveFriend
        }

        public enum UserStoryLogType
        {
            AddImage,
            AddText,
            EditImage,
            EditText
        }

        public enum EditAchievementLogType
        {
            Title,
            Description,
            Icon,
            Type,
            Featured,
            Hidden,
            IsRepeatable,
            State,
            ParentID,
            Threshold,
            ContentType,
            SystemTriggerType,
            RepeatDelayDays,
            PointsCreate,
            PointsExplore,
            PointsLearn,
            PointsSocialize
        }

        public enum EditQuestLogType
        {
            Title,
            Description,
            Icon,
            Featured,
            State,
            Threshold
        }

        public enum ManageSubmissionsLogType
        {
            ApprovedUserQuest,
            DeniedUserQuest,
            ApprovedSubmission,
            DeniedSubmission
        }

        public void LogSingleEntry(LoggerModel loggerModel, bool autoSave = true)
        {
            log newLogEntry = new log()
            {
                action = loggerModel.Action,
                ip_address = loggerModel.IPAddress,
                id_1 = (int)loggerModel.ID1,
                id_2 = (int)loggerModel.ID2,
                timestamp = loggerModel.TimeStamp,
                user_id = loggerModel.UserID,
                value_1 = loggerModel.Value1,
                value_2 = loggerModel.Value2,
            };

            _dbContext.log.Add(newLogEntry);

            if (autoSave)
                Save();
        }

        public void LogMultipleEntries(List<LoggerModel> loggerModelList)
        {
            foreach (LoggerModel loggerModel in loggerModelList)
            {
                LogSingleEntry(loggerModel, false);
            }

            Save();
        }


        private void Save()
        {
            _dbContext.SaveChanges();
        }
    }

    public class LoggerModel
    {
        public String Action { get; set; }
        public String IPAddress { get; set; }
        public DateTime TimeStamp { get; set; }
        public int? ID1 { get; set; }
        public int? ID2 { get; set; }
        public int UserID { get; set; }
        public String Value1 { get; set; }
        public String Value2 { get; set; }
    }
}