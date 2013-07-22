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
    }
}
