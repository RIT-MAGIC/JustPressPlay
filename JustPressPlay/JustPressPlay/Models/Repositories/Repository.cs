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
		protected IUnitOfWork _unitOfWork;

		/// <summary>
		/// Creates a new repository with the specified dbContext
		/// </summary>
		/// <param name="unitOfWork">The unit of work that created this repo</param>
		public Repository(IUnitOfWork unitOfWork)
		{
			_dbContext = unitOfWork.EntityContext;
			_unitOfWork = unitOfWork;
		}
	}
}