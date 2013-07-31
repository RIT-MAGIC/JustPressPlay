using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Data.Entity;
using WebMatrix.WebData;

using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;

namespace JustPressPlay.Models.Repositories
{
	public class UserRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repository</param>
		public UserRepository(UnitOfWork unitOfWork)
			: base(unitOfWork)
		{

		}



		public user GetUser(int id)
		{
			return _dbContext.user.SingleOrDefault(u => u.id == id);
		}

		public user GetUser(string username)
		{
			return _dbContext.user.SingleOrDefault(u => u.username == username);
		}

		public user GetUserByEmail(string email)
		{
			return _dbContext.user.SingleOrDefault(u => u.email == email);
		}

		public List<user> GetAllCaretakers()
		{
			string[] fullAdmin = Roles.GetUsersInRole(JPPConstants.Roles.FullAdmin);

			string[] assign = Roles.GetUsersInRole(JPPConstants.Roles.AssignIndividualAchievements);

			var usernames = fullAdmin.Concat(assign).Distinct();

			var q = from u in _dbContext.user
					where usernames.Contains(u.username)
					select u;

			return q.ToList();

		}

        /// <summary>
        /// Adds (or, if it exists, updates) a user's Facebook settings
        /// </summary>
        public void AddOrUpdateFacebookSettings(user user, bool notificationsEnabled, bool automaticSharingEnabled)
        {
            facebook_connection connection = _dbContext.facebook_connection.Find(user.id);

            if (connection == null)
            {
                connection = new facebook_connection
                {
                    id = user.id,
                    user = user,
                    notifications_enabled = notificationsEnabled,
                    automatic_sharing_enabled = automaticSharingEnabled,
                };
                _dbContext.facebook_connection.Add(connection);
            }
            else
            {
                connection.notifications_enabled = notificationsEnabled;
                connection.automatic_sharing_enabled = automaticSharingEnabled;
            }
        }

        /// <summary>
        /// Updates the Facebook connection data (i.e. access token, etc) for a given user. It is assumed there
        /// is already a Facebook connection entry in the database.
        /// </summary>
        /// <param name="user">The user to be updated</param>
        /// <param name="fbUserId">The user's ID on Facebook</param>
        /// <param name="accessToken">The access token provided by Facebook</param>
        /// <param name="accessTokenExpiration">The date/time the access token will expire</param>
        public void UpdateFacebookDataForExistingConnection(user user, string fbUserId, string accessToken, DateTime accessTokenExpiration)
        {
            // TODO: Handle non-existent entries
            facebook_connection connection = _dbContext.facebook_connection.Find(user.id);
            connection.facebook_user_id = fbUserId;
            connection.access_token = accessToken;
            connection.access_token_expiration = accessTokenExpiration;
        }


		public void Save()
		{
			_dbContext.SaveChanges();
		}

		/// <summary>
		/// Attempts to add a friend
		/// </summary>
		/// <param name="id">The id of the user to add as a friend</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean AddFriend(int id)
		{
			// Can't be us!
			if (id == WebSecurity.CurrentUserId)
				return false;

			// Get the user and make sure they're playing
			user u = GetUser(id);
			if (u == null || !u.is_player || u.status != (int)JPPConstants.UserStatus.Active)
				return false;

			// Are we already friends?
			bool alreadyFriends = (from f in _dbContext.friend
								   where ((f.source_id == id && f.destination_id == WebSecurity.CurrentUserId) ||
										   (f.source_id == WebSecurity.CurrentUserId && f.destination_id == id))
								   select f).Any();
			if (alreadyFriends)
				return false;

			// Pending friend invite?
			bool alreadyPending = (from f in _dbContext.friend_pending
								   where ((f.source_id == id && f.destination_id == WebSecurity.CurrentUserId) ||
										   (f.source_id == WebSecurity.CurrentUserId && f.destination_id == id))
								   select f).Any();
			if (alreadyPending)
				return false;

			// Create the new pending request
			friend_pending friend = new friend_pending()
			{
				source_id = WebSecurity.CurrentUserId,
				destination_id = u.id,
				ignored = false,
				request_date = DateTime.Now
			};
			_dbContext.friend_pending.Add(friend);
			Save();
			return true;
		}
	}
}