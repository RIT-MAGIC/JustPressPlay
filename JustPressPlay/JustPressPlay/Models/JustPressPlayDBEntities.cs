using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace JustPressPlay.Models
{
    // Extends auto-generated JustPressPlayDBEntities (see JustPressPlayEF.Context.cs)
    public partial class JustPressPlayDBEntities : DbContext
    {
        public JustPressPlayDBEntities(string connectionString)
            : base(connectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public JustPressPlayDBEntities(DbConnection connection)
            : base(connection, true)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}