using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{

    public class ContactPageViewModel
    {
        public String SenderName { get; set; }
        public String SenderEmail { get; set; }
        public String SenderMessage { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String DevPassword { get; set; }

        public static ContactPageViewModel Populate(UnitOfWork work = null)
        {
            if (work == null)
                work = new UnitOfWork();

            ContactPageViewModel model = new ContactPageViewModel();

            if (WebSecurity.IsAuthenticated)
            {
               var currentUser =  work.EntityContext.user.Find(WebSecurity.CurrentUserId);
               model.SenderEmail = currentUser.email;
                model.SenderName = currentUser.first_name + " " +currentUser.last_name;
            }

            return model;
        }
    }
	/// <summary>
	/// The view model for the homepage of the site
	/// </summary>
	public class HomeViewModel
	{
		/// <summary>
		/// Holds stats about the overall game
		/// </summary>
		public class Stats
		{
			public int PointsCreate { get; set; }
			public int PointsExplore { get; set; }
			public int PointsLearn { get; set; }
			public int PointsSocialize { get; set; }
			public int TotalPlayers { get; set; }
			public int TotalQuests { get; set; }
			public int TotalAchievements { get; set; }
		}

		public Stats MyStats { get; set; }
		public Stats GameStats { get; set; }
		public List<news> News { get; set; }
		public List<achievement_template> FeaturedAchievements { get; set; }
		public List<quest_template> FeaturedQuests { get; set; }

		/// <summary>
		/// Populates the timeline view model.  If the user is logged in, the player's profile information
		/// and friends earnings will be included.  If the user is not logged in, only public earnings
		/// will be included.
		/// </summary>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">The amount of earnings to return</param>
		/// <param name="includePublic">Include public earnings? Only used when logged in.</param>
		/// <param name="work">Unit of work for DB access.  If null, one will be created.</param>
		/// <returns>A popualted timeline view model</returns>
		public static HomeViewModel Populate(
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includePublic = null,
			int? newsCount = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// News
			var news = from n in work.EntityContext.news select n;
			if (newsCount != null)
			{
				news = news.Take(newsCount.Value);
			}
			news = news.OrderByDescending(n => n.created_date);

			// Featured Achievements & quests
			var achievements = from a in work.EntityContext.achievement_template
							   where a.featured == true
							   orderby a.created_date descending
							   select a;
			var quests = from q in work.EntityContext.quest_template
						 where q.featured == true
						 orderby q.created_date descending
						 select q;

			// Overall game stats
			var gamePoints = (from ai in work.EntityContext.achievement_instance
							  group ai by 1 into achieves
							  select new
							  {
								  PointsCreate = achieves.Sum(p => p.points_create),
								  PointsExplore = achieves.Sum(p => p.points_explore),
								  PointsLearn = achieves.Sum(p => p.points_learn),
								  PointsSocialize = achieves.Sum(p => p.points_socialize),
								  TotalAchievements = achieves.Count()
							  }).FirstOrDefault();
			Stats gameStats = new Stats()
			{
				PointsCreate = gamePoints == null ? 0 : gamePoints.PointsCreate,
				PointsExplore = gamePoints == null ? 0 : gamePoints.PointsExplore,
				PointsLearn = gamePoints == null ? 0 : gamePoints.PointsLearn,
				PointsSocialize = gamePoints == null ? 0 : gamePoints.PointsSocialize,
				TotalAchievements = gamePoints == null ? 0 : gamePoints.TotalAchievements,
				TotalQuests = (from qi in work.EntityContext.quest_instance select qi).Count(),
				TotalPlayers = (from u in work.EntityContext.user where u.is_player == true select u).Count()
			};

			// Current player stats
			Stats myStats = null;
			if (WebSecurity.IsAuthenticated)
			{
				var myPoints = (from ai in work.EntityContext.achievement_instance
								where ai.user_id == WebSecurity.CurrentUserId
								group ai by 1 into achieves
								select new
								{
									PointsCreate = achieves.Sum(p => p.points_create),
									PointsExplore = achieves.Sum(p => p.points_explore),
									PointsLearn = achieves.Sum(p => p.points_learn),
									PointsSocialize = achieves.Sum(p => p.points_socialize),
									TotalAchievements = achieves.Count()
								}).FirstOrDefault();
				myStats = new Stats()
				{
					PointsCreate = myPoints == null ? 0 : myPoints.PointsCreate,
					PointsExplore = myPoints == null ? 0 : myPoints.PointsExplore,
					PointsLearn = myPoints == null ? 0 : myPoints.PointsLearn,
					PointsSocialize = myPoints == null ? 0 : myPoints.PointsSocialize,
					TotalAchievements = myPoints == null ? 0 : myPoints.TotalAchievements,
					TotalQuests = (from qi in work.EntityContext.quest_instance where qi.user_id == WebSecurity.CurrentUserId select qi).Count(),
					TotalPlayers = (from f in work.EntityContext.friend where f.destination_id == WebSecurity.CurrentUserId select f).Count()
				};
			}

			// Assemble and return
			return new HomeViewModel()
			{
				MyStats = myStats,
				GameStats = gameStats,
				News = news.ToList(),
				FeaturedAchievements = achievements.ToList(),
				FeaturedQuests = quests.ToList()
			};
		}
	}
}