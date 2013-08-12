using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JustPressPlay.Utilities
{
    public class Logger
    {
        #region AchievementInstanceLogging
        #endregion

        public enum AchievementInstanceLogType
        {
            GlobalAssigned,
            Revoked
        }

        public enum QuestInstanceLogType
        {
            Revoked
        }

        public enum CommentBehaviorLogType
        {
            CommentEdit
        }

        public enum PlayerEditProfileContentLogType
        {
            DisplayNameEdit,
            FullBioEdit,
            ProfilePictureEdit,
            SixWordBioEdit
        }

        public enum PlayerEditProfileSettingsLogType
        {
            Privacy,
            Facebook,
            Password,
            Communications,
            LeftGame
        }

        //Action taken by the player involving Friend
        //(Adding Friends, Ignoring Requests, etc.)
        public enum PlayerFriendLogType
        {
            AddFriend,
            AcceptRequest,
            HideRequest,
            IgnoreRequest,
            RemoveFriend
        }
     
    }
}