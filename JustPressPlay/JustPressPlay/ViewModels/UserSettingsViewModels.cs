/*
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