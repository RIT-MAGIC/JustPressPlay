using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

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
        private JustPressPlayDBEntities entities;
        private UserRepository ur;
        private SystemRepository sr;
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
		/// <param name="dbContext">The context for DB communications</param>
		public UserRepository(JustPressPlayDBEntities dbContext)
			: base(dbContext)
		{
            entities = dbContext;
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
            return entities.user.SingleOrDefault(u => u.id == id);
        }

        public user GetUser(string username)
        {
            return entities.user.SingleOrDefault(u => u.username == username);
        }

        public user GetUser(string email)
        {
            return entities.user.SingleOrDefault(u => u.email == email);
        }
        


        #endregion
        //------------------------------------------------------------------------------------//
        //------------------------------------Insert/Delete-----------------------------------//
        //------------------------------------------------------------------------------------//
        #region Insert/Delete

        //-----Admin Insert/Delete-----//
        #region Admin Insert/Delete

        public void AdminCreateUser()//CreateAchievementViewModel createAchievementModel)
        {
            user newUser = new user()
            {
                username = "test",
                first_name = "test",
                middle_name = "ing",
                last_name = "this",
                is_player = true,
                created_date = DateTime.Now,
                status = 1,
                first_login = true,
                email = "test@test.test",
                last_login_date = DateTime.Now,
                organization_id = "beep",
                organization_program_code = "boop",
                organization_year_level = "three",
                organization_user_type = "lame",
                display_name = "TestingThis",
                six_word_bio = null,
                full_bio = null,
                image = null,
                personal_url = null,
                privacy_settings = 1,
                has_agreed_to_tos = false,
                creator_id = 7,
                modified_date = null,
            };

            entities.user.Add(newUser);

            Save();
        }


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
            entities.SaveChanges();
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