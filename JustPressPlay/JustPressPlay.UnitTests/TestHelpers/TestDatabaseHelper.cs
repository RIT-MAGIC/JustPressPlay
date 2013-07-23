using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JustPressPlay.Models;

namespace JustPressPlay.UnitTests.TestHelpers
{
    [TestClass]
    public class TestDatabaseHelper
    {
        [TestMethod]
        public void TestCreateDatabase()
        {
            // Check to make sure the DB is created without an exception being thrown
            JustPressPlayDBEntities db = DatabaseHelper.CreateNewDatabase();
        }
    }
}
