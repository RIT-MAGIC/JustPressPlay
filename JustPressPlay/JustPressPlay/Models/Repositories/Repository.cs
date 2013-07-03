using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace JustPressPlay.Models.Repositories
{
	public abstract class Repository
	{
		// The database context for communication with the DB
		protected JustPressPlayDBEntities _dbContext;

		/// <summary>
		/// Creates a new repository with the specified dbContext
		/// </summary>
		/// <param name="dbContext">The context to use for DB communication</param>
		public Repository(JustPressPlayDBEntities dbContext)
		{
			_dbContext = dbContext;
		}
	}
}