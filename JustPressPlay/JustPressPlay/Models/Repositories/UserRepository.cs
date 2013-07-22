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
		//------------------------------------------------------------------------------------//
        //-------------------------------------Enums------------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Enums
        #endregion


        //------------------------------------------------------------------------------------//
        //------------------------------------Variables---------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Variables
        #endregion


        //------------------------------------------------------------------------------------//
        //------------------------------------Properties--------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Properties
        #endregion


        //------------------------------------------------------------------------------------//
        //-----------------------------------Constructors-------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Constructors
        /// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repository</param>
		public UserRepository(UnitOfWork unitOfWork)
			: base(unitOfWork)
		{
            
		}
        #endregion
		
        //------------------------------------------------------------------------------------//
        //---------------------------------Populate ViewModels--------------------------------//
        //------------------------------------------------------------------------------------//
        #region Populate ViewModels
        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Query Methods-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Query Methods

        //Methods for getting a user based on id/username/email

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

        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Insert/Delete-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Insert/Delete

        //-----Admin Insert/Delete-----//
        #region Admin Insert/Delete

        #endregion
        //-----User Insert/Delete------//
        #region User Insert/Delete
        #endregion

        #endregion
        //------------------------------------------------------------------------------------//
        //-------------------------------------Persistence------------------------------------//
        //------------------------------------------------------------------------------------//
        #region Persistence
        public void Save()
        {
            _dbContext.SaveChanges();
        }
        #endregion
        //------------------------------------------------------------------------------------//
        //-----------------------------------Helper Methods-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Helper Methods


        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------JSON Methods------------------------------------//
        //------------------------------------------------------------------------------------//
        #region JSON Methods
        #endregion
        //------------------------------------------------------------------------------------//
        //---------------------------------System Achievements--------------------------------//
        //------------------------------------------------------------------------------------//
        #region System Achievements
        #endregion

    
	}
}