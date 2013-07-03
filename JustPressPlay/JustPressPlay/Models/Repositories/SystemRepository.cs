using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace JustPressPlay.Models.Repositories
{
	public class SystemRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="dbContext">The context for DB communications</param>
		public SystemRepository(JustPressPlayDBEntities dbContext)
			: base(dbContext)
		{
			
		}

	}
}