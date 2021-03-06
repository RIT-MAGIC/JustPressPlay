﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.WebData;

using JustPressPlay.Utilities;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using System.Web.Mvc;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Contains common information about a notification
	/// </summary>
	public class LayoutNotification
	{
		public enum NotificationType
		{
			Text,
			FriendRequest
		}

		public NotificationType Type { get; set; }
		public int ID { get; set; }
		public int SourceID { get; set; }
		public int DestinationID { get; set; }
		public DateTime Date { get; set; }
		public String Icon { get; set; }
		public String Message { get; set; }
		public String Name { get; set; }
        public String URL { get; set; }
		public Boolean Ignored { get; set; }
	}

	/// <summary>
	/// Holds basic info about a user that we might need
	/// on any given page
	/// </summary>
	public class LayoutViewModel
	{	
		public int ID { get; set; }
		public String DisplayName { get; set; }
		public String FirstName { get; set; }
		public String MiddleName { get; set; }
		public String LastName { get; set; }
        public String Image { get; set; }
        public String QRImage { get; set; }
		public bool IsPlayer { get; set; }
		public JPPConstants.UserStatus Status { get; set; }
		public JPPConstants.PrivacySettings Privacy { get; set; }
		public List<LayoutNotification> Notifications { get; set; }
	
		/// <summary>
		/// Fills in any required user data for this view model
		/// </summary>
		/// <param name="work">The Unit of Work for DB access</param>
		public static LayoutViewModel Populate(UnitOfWork work = null)
		{
			if (!WebSecurity.IsAuthenticated)
				return null;

			if (work == null)
				work = new UnitOfWork();

			// Get the user information
			user user = work.UserRepository.GetUser(WebSecurity.CurrentUserId);
			if (user == null)
				return null;

			LayoutViewModel layout = new LayoutViewModel()
			{
				ID = user.id,
				DisplayName = user.display_name,
				IsPlayer = user.is_player,
				Privacy = (JPPConstants.PrivacySettings)user.privacy_settings,
				Status = (JPPConstants.UserStatus)user.status,
				FirstName = user.first_name,
				MiddleName = user.middle_name,
				LastName = user.last_name,
                Image = user.image,
                QRImage = user.qr_image,
				Notifications = new List<LayoutNotification>()
			};

			// Handle notifications
			var q = (from n in work.EntityContext.notification
					 where n.destination_id == user.id
					 orderby n.date
					 select new LayoutNotification()
					 {
						 Type = LayoutNotification.NotificationType.Text,
						 ID = n.id,
						 Date = n.date,
						 DestinationID = n.destination_id,
						 SourceID = n.source_id,
						 Icon = n.icon,
						 Message = n.message,
						 Ignored = false,
                         URL = n.url
					 }).ToList();
			layout.Notifications.AddRange(q);

			// Handle friend requests
			var qf = (from f in work.EntityContext.friend_pending
					  where f.destination_id == user.id
					  orderby f.request_date
					  select new LayoutNotification()
					  {
						  Type = LayoutNotification.NotificationType.FriendRequest,
						  ID = f.id,
						  Date = f.request_date,
						  DestinationID = f.destination_id,
						  SourceID = f.source_id,
						  Icon = f.source.image,
						  Message = "[" + f.source.display_name + "] would like to be your friend",
						  Ignored = f.ignored
					  }).ToList();

            foreach (var f in qf)
            {
                f.URL = new UrlHelper(HttpContext.Current.Request.RequestContext).Action(
                    "Profile",
                    "Players",
                    new { id = f.SourceID }
                );
            }
			layout.Notifications.AddRange(qf);

			// Sort and done
			layout.Notifications = layout.Notifications.OrderBy(n => n.Date).ToList();
			return layout;
		}
	}
}