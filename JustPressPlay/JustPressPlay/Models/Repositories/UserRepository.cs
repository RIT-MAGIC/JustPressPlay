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

        public JPPConstants.PrivacySettings GetPrivacySettingsById(int id)
        {
            user myUser = GetUser(id);
            return (JPPConstants.PrivacySettings)myUser.privacy_settings;
        }

        public JPPConstants.CommunicationSettings GetCommunicationSettingsById(int id)
        {
            user myUser = GetUser(id);
            return (JPPConstants.CommunicationSettings)myUser.communication_settings;
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

        public void UpdateUserSettings(int userId, JPPConstants.CommunicationSettings communicationSettings, JPPConstants.PrivacySettings privacySettings)
        {
            user user = _dbContext.user.Find(userId);
            user.communication_settings = (int)communicationSettings;
            user.privacy_settings = (int)privacySettings;
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

        /// <summary>
        /// Attempts to get a user's Facebook settings
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Their connection information, or null if not found</returns>
        public facebook_connection GetUserFacebookSettingsById(int userId)
        {
            return _dbContext.facebook_connection.Find(userId);
        }


		public void Save()
		{
			_dbContext.SaveChanges();
		}

		/// <summary>
		/// Attempts to add a friend (sends a request)
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
            LoggerModel logFriendRequest = new LoggerModel()
            {
                Action = Logger.PlayerFriendLogType.AddFriend.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = id,
                IDType1 = Logger.LogIDType.User.ToString()
            };
            Logger.LogSingleEntry(logFriendRequest, _dbContext);

			Save();
			return true;
		}

		/// <summary>
		/// Accepts a friend request between the specified
		/// id (the source) and the logged in user (the destination)
		/// </summary>
		/// <param name="id">The id of the source of the friend request</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean AcceptFriendRequest(int id)
		{
			// Ssame user?
			if (id == WebSecurity.CurrentUserId)
				return false;

			// Find the pending request
			friend_pending pending = (from f in _dbContext.friend_pending
									  where f.source_id == id && f.destination_id == WebSecurity.CurrentUserId
									  select f).FirstOrDefault();
			if (pending == null)
				return false;

			// Remove pending, then add friendships in both directions
			_dbContext.friend_pending.Remove(pending);

			friend f1 = new friend()
			{
				source_id = id,
				destination_id = WebSecurity.CurrentUserId,
				request_date = pending.request_date,
				friended_date = DateTime.Now
			};

			friend f2 = new friend()
			{
				source_id = WebSecurity.CurrentUserId,
				destination_id = id,
				request_date = pending.request_date,
				friended_date = f1.friended_date
			};

			_dbContext.friend.Add(f1);
			_dbContext.friend.Add(f2);
            LoggerModel logFriendRequest = new LoggerModel()
            {
                Action = Logger.PlayerFriendLogType.AcceptRequest.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = id,
                IDType1 = Logger.LogIDType.User.ToString()
            };
            Logger.LogSingleEntry(logFriendRequest, _dbContext);
			_dbContext.SaveChanges();
			return true;
		}

		/// <summary>
		/// Declines a friend request between the specified
		/// id (the source) and the logged in user (the destination)
		/// </summary>
		/// <param name="id">The id of the source of the friend request</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean DeclineFriendRequest(int id)
		{
			// Find the pending request
			friend_pending pending = (from f in _dbContext.friend_pending
									  where f.source_id == id && f.destination_id == WebSecurity.CurrentUserId
									  select f).FirstOrDefault();
			if (pending == null)
				return false;

			// Remove pending request
			_dbContext.friend_pending.Remove(pending);
            LoggerModel logFriendRequest = new LoggerModel()
            {
                Action = Logger.PlayerFriendLogType.DeclineRequest.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = id,
                IDType1 = Logger.LogIDType.User.ToString()
            };
            Logger.LogSingleEntry(logFriendRequest, _dbContext);

			_dbContext.SaveChanges();
			return true;
		}

		/// <summary>
		/// Ignores a friend request between the specified
		/// id (the source) and the logged in user (the destination)
		/// </summary>
		/// <param name="id">The id of the source of the friend request</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean IgnoreFriendRequest(int id)
		{
			// Find the pending request
			friend_pending pending = (from f in _dbContext.friend_pending
									  where f.source_id == id && f.destination_id == WebSecurity.CurrentUserId
									  select f).FirstOrDefault();
			if (pending == null)
				return false;

			// Ignore pending request
			pending.ignored = true;

            LoggerModel logFriendRequest = new LoggerModel()
            {
                Action = Logger.PlayerFriendLogType.IgnoreRequest.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = id,
                IDType1 = Logger.LogIDType.User.ToString()
            };
            Logger.LogSingleEntry(logFriendRequest, _dbContext);

			_dbContext.SaveChanges();
			return true;
		}

		/// <summary>
		/// Removes a friendship between the specified
		/// id and the logged in user
		/// </summary>
		/// <param name="id">The id of the friend to remove</param>
		/// <returns>True if successful, false otherwise</returns>
		public Boolean RemoveFriend(int id)
		{
			// Find the friendships
			friend f1 = (from f in _dbContext.friend
						 where f.source_id == id && f.destination_id == WebSecurity.CurrentUserId
						 select f).FirstOrDefault();
			friend f2 = (from f in _dbContext.friend
						 where f.source_id == WebSecurity.CurrentUserId && f.destination_id == id
						 select f).FirstOrDefault();

			// If neither exists, can't remove
			if (f1 == null && f2 == null)
				return false;

            LoggerModel logFriendRequest = new LoggerModel()
            {
                Action = Logger.PlayerFriendLogType.RemoveFriend.ToString(),
                UserID = WebSecurity.CurrentUserId,
                IPAddress = HttpContext.Current.Request.UserHostAddress,
                TimeStamp = DateTime.Now,
                ID1 = id,
                IDType1 = Logger.LogIDType.User.ToString()
            };
            Logger.LogSingleEntry(logFriendRequest, _dbContext);
			// Remove both - It should be either both or neither, but
			// during testing we may end up with just one way, so better
			// to remove the stragglers
			if (f1 != null) _dbContext.friend.Remove(f1);
			if (f2 != null) _dbContext.friend.Remove(f2);
			_dbContext.SaveChanges();
			return true;
		}
	}
}