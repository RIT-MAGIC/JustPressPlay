using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JustPressPlay.Models.Repositories
{
	public class UnitOfWork : IDisposable
	{
		// The database context through the entity framework
		private JustPressPlayDBEntities _entityContext;

		// The various repositories used by this unit of work
		private AchievementRepository _achievementRepo;
		private QuestRepository _questRepo;
		private UserRepository _userRepo;
		private SystemRepository _systemRepo;

		/// <summary>
		/// Gets the entity framework database context this Unit of Work uses
		/// </summary>
		public JustPressPlayDBEntities EntityContext { get { return _entityContext; } }

		/// <summary>
		/// Gets this unit of work's achievement repository
		/// </summary>
		public AchievementRepository AchievementRepository { get { return _achievementRepo; } }

		/// <summary>
		/// Gets this unit of work's quest repository
		/// </summary>
		public QuestRepository QuestRepository { get { return _questRepo; } }

		/// <summary>
		/// Gets this unit of work's user repository
		/// </summary>
		public UserRepository UserRepository { get { return _userRepo; } }

		/// <summary>
		/// Gets this unit of work's system repository
		/// </summary>
		public SystemRepository SystemRepository { get { return _systemRepo; } }

		/// <summary>
		/// Creates a new unit of work for the various database repositories
		/// </summary>
		public UnitOfWork()
		{
			_entityContext = new JustPressPlayDBEntities();
			_achievementRepo = new AchievementRepository(this);
			_questRepo = new QuestRepository(this);
			_userRepo = new UserRepository(this);
			_systemRepo = new SystemRepository(this);
		}

		/// <summary>
		/// Saves all database changes for this unit of work
		/// </summary>
		/// <returns>The number of objects written to the database</returns>
		public int SaveChanges()
		{
			return _entityContext.SaveChanges();
		}

		#region IDisposable Implementation
		// Remember if we've been disposed
		private bool _disposed = false;

		/// <summary>
		/// Dispose this unit of work
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Handles disposing the entity framework DB context
		/// </summary>
		/// <param name="disposing">Are we disposing?</param>
		public void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				_entityContext.Dispose();
			}
			_disposed = true;
		}
		#endregion
	}
}