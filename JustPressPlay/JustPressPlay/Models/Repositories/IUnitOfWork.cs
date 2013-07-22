using System;
namespace JustPressPlay.Models.Repositories
{
    public interface IUnitOfWork
    {
        AchievementRepository AchievementRepository { get; }
        JustPressPlay.Models.JustPressPlayDBEntities EntityContext { get; }
        QuestRepository QuestRepository { get; }
        SystemRepository SystemRepository { get; }
        UserRepository UserRepository { get; }

        /// <summary>
        /// Saves all database changes for this unit of work
        /// </summary>
        /// <returns>The number of objects written to the database</returns>
        int SaveChanges();
    }
}
