using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace JustPressPlay.Models.Repositories
{
	public class UserRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="dbContext">The context for DB communications</param>
		public UserRepository(JustPressPlayDBEntities dbContext)
			: base(dbContext)
		{

		}

	}
}