using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.Entity;

namespace JustPressPlay.Models.Repositories
{
	public class QuestRepository : Repository
	{
		/// <summary>
		/// Creates a new user repository
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repository</param>
		public QuestRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{

		}

        public IEnumerable<quest_tracking> GetTrackedQuestsForUser(int userID)
        {
            return _dbContext.quest_tracking.Where(q => q.user_id == userID);
        }

	}
}