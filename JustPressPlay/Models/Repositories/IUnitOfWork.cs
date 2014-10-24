/*
 * Copyright 2014 Rochester Institute of Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
