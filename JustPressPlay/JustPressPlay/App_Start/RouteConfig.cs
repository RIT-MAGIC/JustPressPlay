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
			routes.MapRoute(
				"AchievementsPlayersRoute",
				"Achievements/{achievementID}/{playerID}",
				new { controller = "Achievements", action = "AchievementPlayer" },
				new { achievementID = @"\d+", playerID = @"\d+" }
			);

			routes.MapRoute(
				"QuestsPlayersRoute",
				"Quests/{questID}/{playerID}",
				new { controller = "Quests", action = "QuestPlayer" },
				new { questID = @"\d+", playerID = @"\d+" }
			);

			// Routes a url (Achievements/{id}) to the specified action,
			// completely invisible to the user.  Also constrains the
			// parameter to contain only digits
			routes.MapRoute(
				"AchievementsRoute",
				"Achievements/{id}",
				new { controller = "Achievements", action = "IndividualAchievement" },
				new { id = @"\d+" }
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
				new { controller = "Players", action = "IndividualPlayer" },
				new { id = @"\d+" }
			);

			routes.MapRoute(
				"NewsRoute",
				"News/{id}",
				new { controller = "News", action = "IndividualNews" },
				new { id = @"\d+" }
			);

			// Default route for MVC
			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}