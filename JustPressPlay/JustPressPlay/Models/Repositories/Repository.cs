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
		
		// The unit of work that created this repository
		protected UnitOfWork _unitOfWork;

		/// <summary>
		/// Creates a new repository with the specified dbContext
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repo</param>
		public Repository(UnitOfWork unitOfWork)
		{
			_dbContext = unitOfWork.EntityContext;
			_unitOfWork = unitOfWork;
		}
	}
}