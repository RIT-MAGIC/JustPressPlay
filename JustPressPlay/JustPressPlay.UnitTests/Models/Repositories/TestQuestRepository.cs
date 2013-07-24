using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JustPressPlay.Models;
using JustPressPlay.UnitTests.TestHelpers;
using JustPressPlay.Utilities;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace JustPressPlay.UnitTests.Models.Repositories
{
    [TestClass]
    public class TestQuestRepository
    {
        JustPressPlayDBEntities _db;
        user user;

        [TestInitialize]
        public void SetupTest()
        {
            _db = DatabaseHelper.CreateNewDatabase();
            user = new user
            {
                username = "UserName",
                first_name = "FirstName",
                middle_name = "MiddleName",
                last_name = "LastName",
                is_player = true,
                created_date = DateTime.Now,
                status = (int)JPPConstants.UserStatus.Active,
                first_login = true,
                email = "test@email.com",
                last_login_date = DateTime.Now,
                display_name = "DisplayName",
                privacy_settings = (int)JPPConstants.PrivacySettings.JustPressPlayOnly,
                has_agreed_to_tos = false,
                creator_id = null
            };

            _db.user.Add(user);

            DatabaseHelper.TrySaveChanges(ref _db);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            _db.Dispose();
        }

        [TestMethod]
        public void TestAddQuestTemplate()
        {
            quest_template template = new quest_template
            {
                created_date = DateTime.Now, creator = user, title = "QuestTitle", description = "Description", icon = ""
            };

            _db.quest_template.Add(template);

            DatabaseHelper.TrySaveChanges(ref _db);
        }
    }
}
