using JustPressPlay.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlServerCe;
using System.IO;

namespace JustPressPlay.UnitTests.TestHelpers
{
    public static class DatabaseHelper
    {
        public static JustPressPlayDBEntities CreateNewDatabase()
        {
            // Code derived from http://www.codeproject.com/Articles/460175/Two-strategies-for-testing-Entity-Framework-Effort
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "")); // see http://stackoverflow.com/questions/12244495/how-to-set-up-localdb-for-unit-tests-in-visual-studio-2012-and-entity-framework/14680912#14680912
            var filePath = Directory.GetCurrentDirectory() + @"\JustPressPlayDB.mdf";

            var origConnectionString=@"metadata=res://*/Models.JustPressPlayEF.csdl|res://*/Models.JustPressPlayEF.ssdl|res://*/Models.JustPressPlayEF.msl;provider=System.Data.SqlClient;provider connection string=""data source=(LocalDB)\v11.0;attachdbfilename=|DataDirectory|\JustPressPlayDB.mdf;integrated security=True;MultipleActiveResultSets=True;""";

            if (File.Exists(filePath))
                File.Delete(filePath);

            string connectionString = origConnectionString;//"Datasource = " + filePath;
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

            // Initialize DB
            using (var context = new JustPressPlayDBEntities(connectionString))
            {
                context.Database.Create();
            }

            // Connect to DB and return it
            return new JustPressPlayDBEntities(connectionString);
        }
    }
}
