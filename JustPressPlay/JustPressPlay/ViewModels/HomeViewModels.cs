using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using WebMatrix.WebData;

using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// The view model for the homepage of the site
	/// </summary>
	[DataContract]
	public class TimelineViewModel
	{
		/// <summary>
		/// Holds stats about the overall game
		/// </summary>
		[DataContract]
		public class GameStats
		{
			[DataMember]
			public int PointsCreate { get; set; }

			[DataMember]
			public int PointsExplore { get; set; }
			
			[DataMember]
			public int PointsLearn { get; set; }

			[DataMember]
			public int PointsSocialize { get; set; }

			[DataMember]
			public int TotalPlayers { get; set; }

			[DataMember]
			public int TotalQuests { get; set; }

			[DataMember]
			public int TotalAchievements { get; set; }
		}

		/// <summary>
		/// When logged in, this can be a mix of public and friends earnings, or just
		/// friends earnings (when necessary).  When logged out, it's just public earnings.
		/// </summary>
		[DataMember]
		public EarningsViewModel OtherEarnings { get; set; }

		[DataMember]
		public ProfileViewModel MyProfile { get; set; }

		[DataMember]
		public GameStats Stats { get; set; }

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
		public static TimelineViewModel Populate(
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includePublic = null,
			UnitOfWork work = null)
		{
			if (work == null)
				work = new UnitOfWork();

			// Point stats
			var q = (from ai in work.EntityContext.achievement_instance
					group ai by 1 into achieves
					select new
					{
						PointsCreate = achieves.Sum(p => p.points_create),
						PointsExplore = achieves.Sum(p => p.points_explore),
						PointsLearn = achieves.Sum(p => p.points_learn),
						PointsSocialize = achieves.Sum(p => p.points_socialize),
						TotalAchievements = achieves.Count()
					}).FirstOrDefault();
			
			// Assemble and return
			return new TimelineViewModel()
			{
				MyProfile = WebSecurity.IsAuthenticated ? ProfileViewModel.Populate(WebSecurity.CurrentUserId, work) : null,
				OtherEarnings =
					WebSecurity.IsAuthenticated ?
						// User is logged in
						EarningsViewModel.Populate(
							WebSecurity.CurrentUserId,
							null,
							null,
							true,
							includePublic,
							start,
							count,
							startComments,
							countComments,
							null,
							work) :
						// User is not logged in - public only!
						EarningsViewModel.Populate(
							null,
							null,
							null,
							null,
							true,
							start,
							count,
							startComments,
							countComments,
							null,
							work),
				Stats = new GameStats()
				{
					PointsCreate = q == null ? 0 : q.PointsCreate,
					PointsExplore = q == null ? 0 : q.PointsExplore,
					PointsLearn = q == null ? 0 : q.PointsLearn,
					PointsSocialize = q == null ? 0 : q.PointsSocialize,
					TotalAchievements = q == null ? 0 : q.TotalAchievements,
					TotalQuests = (from qi in work.EntityContext.quest_instance select qi).Count(),
					TotalPlayers = (from u in work.EntityContext.user where u.is_player == true select u).Count()
				}
			};
		}
	}
}