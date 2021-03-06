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
using System.Web.Mvc;
using System.Web.Routing;

namespace JustPressPlay
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			// Routes a url (Achievements/{aID}/{pID}) to the specified action,
			// completely invisible to the user.  All constrains both params
			// to contain only digits
			/*routes.MapRoute(
				"AchievementsPlayersRoute",
				"Achievements/{achievementID}/{playerID}",
				new { controller = "Achievements", action = "AchievementPlayer" },
				new { achievementID = @"\d+", playerID = @"\d+" }
			);*/

			routes.MapRoute(
				"QuestsPlayersRoute",
				"Quests/{questID}/{playerID}",
				new { controller = "Quests", action = "QuestPlayer" },
				new { questID = @"\d+", playerID = @"\d+" }
			);

            // Routes a url (Achievements/{id}/{playerID}) to the specified action,
            // completely invisible to the user.  Also constrains the
            // parameter to contain only digits
            routes.MapRoute(
                "AchievementsPlayersRoute",
                "Achievements/{id}/{playerID}",
                new { controller = "Achievements", action = "IndividualAchievement", playerID = UrlParameter.Optional },
                new { id = @"\d+"}
            );

			routes.MapRoute(
				"QuestsRoute",
				"Quests/{id}",
				new { controller = "Quests", action = "IndividualQuest" },
				new { id = @"\d+" }
			);

			routes.MapRoute(
				"PlayersRoute",
				"Players/{id}",
				new { controller = "Players", action = "Profile" },
				new { id = @"\d+" }
			);

			routes.MapRoute(
				"NewsRoute",
				"News/{id}",
				new { controller = "News", action = "IndividualNews" },
				new { id = @"\d+" }
			);

            #region Mozilla OpenBadges API
            routes.MapRoute(
                name: "OpenBadgesIssuerRoute",
                url: "api/OpenBadges/Issuer",
                defaults: new { controller = "OpenBadges", action = "Issuer" }
            );

            routes.MapRoute(
                name: "OpenBadgeDescriptionRoute",
                url: "api/OpenBadges/BadgeDescription/{achievementID}",
                defaults: new { controller = "OpenBadges", action = "BadgeDescription" },
                constraints: new { achievementID = @"\d+" }
            );

            routes.MapRoute(
                name: "OpenBadgeRoute",
                url: "api/OpenBadges/{action}/{achievementID}/{userID}",
                defaults: new { controller = "OpenBadges" },
                constraints: new { achievementID = @"\d+", userID = @"\d+" }
            );
            #endregion

            // Default route for MVC
			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}