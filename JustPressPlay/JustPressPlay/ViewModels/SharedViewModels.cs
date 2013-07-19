using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	/// <summary>
	/// Holds a list of earnings
	/// </summary>
	[DataContract]
	public class EarningsViewModel
	{
		[DataMember]
		public List<Earning> Earnings { get; set; }

		/// <summary>
		/// Contains information about one earning
		/// </summary>
		[DataContract]
		public class Earning
		{
			[DataMember]
			public String DisplayName { get; set; }
			
			[DataMember]
			public String PlayerImage { get; set; }

			[DataMember]
			public DateTime EarnedDate { get; set; }

			[DataMember]
			public AchievementViewModel Achievement { get; set; }

			[DataMember]
			public String StoryPhoto { get; set; }

			[DataMember]
			public String StoryText { get; set; }

			[DataMember]
			public EarningCommentsViewModel Comments { get; set; }
		}

		/// <summary>
		/// Returns a list of earnings for the specified user.
		/// TODO: Secure this so users can't spoof and get non-friend info
		/// </summary>
		/// <param name="id">The id of the player whose earnings should be returned</param>
		/// <param name="friendsOf">Return earnings of players who are friends with the specified player instead?</param>
		/// <param name="start">The zero-based index of the first earning to return</param>
		/// <param name="count">How many earnings should be returned?</param>
		/// <param name="startComments">The zero-based index of the first comment to be returned</param>
		/// <param name="countComments">How many comments should be returned?</param>
		/// <param name="includeDeletedComments">Should deleted comments be returned?</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created</param>
		/// <returns>A list of earnings</returns>
		public static EarningsViewModel Populate(
			int? id = null,
			bool? friendsOf = null,
			int? start = null,
			int? count = null,
			int? startComments = null,
			int? countComments = null,
			bool? includeDeletedComments = null,
			UnitOfWork work = null)
		{
			if (work == null) 
				work = new UnitOfWork();

			// Start the query with all achievement instances
			var q = from e in work.EntityContext.achievement_instance
					select e;

			// What kind of user restrictions?
			if (id != null && friendsOf != null && friendsOf.Value == true)
			{
				// Get earnings of the user's friends
				q = from e in q
					from f in work.EntityContext.friend
					where e.user_id != id &&
						  ((f.source_id == id && f.destination_id == e.user_id) ||
						  (f.source_id == e.user_id && f.destination_id == id))
					select e;
			}
			else if (id != null)
			{
				// Get the earnings of just the user
				q = from e in q
					where e.user_id == id
					select e;
			}

			// Create the final query
			var final = from e in q
						join u in work.EntityContext.user
						on e.user_id equals u.id
						select new Earning()
						{
							DisplayName = u.display_name,
							PlayerImage = u.image,
							Achievement = AchievementViewModel.Populate(e.achievement_id, work), // TODO: Test!
							EarnedDate = e.achieved_date,
							StoryPhoto = e.user_story.image,
							StoryText = e.user_story.text,
							Comments = EarningCommentsViewModel.Populate(
								null,	// Null because we have the instance id
								null,	// Null because we have the instance id
								e.id,	// Use the instance id instead of user and template id
								startComments,
								countComments,
								includeDeletedComments,
								work)
						};

			// Make the model and run the query
			return new EarningsViewModel()
			{
				Earnings = final.ToList()
			};
		}
	}

	/// <summary>
	/// Comments on a single earning
	/// </summary>
	[DataContract]
	public class EarningCommentsViewModel
	{
		[DataMember]
		public List<EarningComment> Comments { get; set; }

		/// <summary>
		/// A single comment on an earning
		/// </summary>
		[DataContract]
		public class EarningComment
		{
			[DataMember]
			public String DisplayName { get; set; }

			[DataMember]
			public String PlayerImage { get; set; }

			[DataMember]
			public String Text { get; set; }
		}

		/// <summary>
		/// Returns a populated list of earning comments
		/// </summary>
		/// <param name="id">The id of the user who's achievement the comments are on (requires 'achievementID' to be present as well)</param>
		/// <param name="achievementID">The id of the achievement template the comments are on (requires 'id' to be present as well)</param>
		/// <param name="achievementInstanceID">The id of the achievement instance the comments are on (overrides the use of 'achievementID' and 'id')</param>
		/// <param name="start">The zero-based index of the first comment to get</param>
		/// <param name="count">How many comments should be returned?</param>
		/// <param name="includeDeleted">Should deleted comments be included? TODO: Require specific roles for this!</param>
		/// <param name="work">The unit of work for DB access.  If null, one will be created</param>
		/// <returns>A populated list of comments</returns>
		public static EarningCommentsViewModel Populate(
			int? id = null,
			int? achievementID = null,
			int? achievementInstanceID = null,
			int? start = null,
			int? count = null,
			bool? includeDeleted = null,
			UnitOfWork work = null)
		{
			if (work == null) work = new UnitOfWork();

			// TODO: Finish

			return new EarningCommentsViewModel()
			{

			};
		}
	}
}