using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JustPressPlay.ViewModels
{
    public class UserSettingsViewModel
    {
        public FacebookConnectionViewModel FacebookConnectionModel { get; set; }

        public int PrivacySettings { get; set; }
        public List<SelectListItem> PrivacySettingsSelectList
        {
            get
            {
                return JPPConstants.SelectListFromEnum<JPPConstants.PrivacySettings>();
            }
        }

        public int CommunicationSettings { get; set; }
        public List<SelectListItem> CommunicationSettingsSelectList
        {
            get
            {
                return JPPConstants.SelectListFromEnum<JPPConstants.CommunicationSettings>();
            }
        }

        public static UserSettingsViewModel Populate(int userId, IUnitOfWork work = null)
        {
            if (work == null) work = new UnitOfWork();

            UserSettingsViewModel model = new UserSettingsViewModel()
            {
                FacebookConnectionModel = FacebookConnectionViewModel.Populate(userId, work),
                PrivacySettings = work.UserRepository.GetPrivacySettingsById(userId),
                CommunicationSettings = work.UserRepository.GetCommunicationSettingsById(userId),
            };

            return model;
        }
    }

    public class FacebookConnectionViewModel
    {
        public Boolean NotificationsEnabled { get; set; }

        public Boolean AutomaticSharingEnabled { get; set; }

        public static FacebookConnectionViewModel Populate(int userId, IUnitOfWork work = null)
        {
            if (work == null) work = new UnitOfWork();

            facebook_connection fbConnection = work.EntityContext.facebook_connection.Find(userId);
            if (fbConnection == null)
            {
                fbConnection = new facebook_connection() { notifications_enabled = false, automatic_sharing_enabled = false };
            }

            FacebookConnectionViewModel model = new FacebookConnectionViewModel()
            {
                NotificationsEnabled = fbConnection.notifications_enabled,
                AutomaticSharingEnabled = fbConnection.automatic_sharing_enabled,
            };

            return model;
        }
    }
}