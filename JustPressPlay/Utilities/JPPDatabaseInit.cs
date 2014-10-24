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
using System.Web.Security;
using System.Data.Entity;
using WebMatrix.WebData;
using System.Reflection;

using JustPressPlay.Models;

namespace JustPressPlay.Utilities
{
	/// <summary>
	/// Handles database initialization and seeding
	/// </summary>
	public class JPPDatabaseInit : IDatabaseInitializer<JustPressPlayDBEntities>
	{
		/// <summary>
		/// Handles initializing the DB by seeding it with required data
		/// </summary>
		/// <param name="context">The database context</param>
		public void InitializeDatabase(JustPressPlayDBEntities context)
		{
			// Make sure web security is set up
			if (!WebSecurity.Initialized)
				WebSecurity.InitializeDatabaseConnection(
					"JustPressPlayDBWebSecurity",	// The special connection string to bypass EF
					"user",							// Our users table
					"id",							// The primary key of the users table
					"username",						// The "username" column of the users table
					autoCreateTables: true			// Creates ASP tables if necessary
				);

			// Add constants and such
			Seed(context);
		}

		/// <summary>
		/// Seeds the database with any required constants if they don't exist
		/// </summary>
		/// <param name="context"></param>
		private void Seed(JustPressPlayDBEntities context)
		{
			// Get all the role constants through reflection
			Type rolesType = typeof(JPPConstants.Roles);
			List<FieldInfo> roleFields = 
				rolesType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(f => f.IsLiteral && !f.IsInitOnly)
				.ToList();

			// Add the roles to the database
			foreach (FieldInfo f in roleFields)
			{
				String roleName = f.GetValue(null).ToString();
				if (!Roles.RoleExists(roleName))
				{
					Roles.CreateRole(roleName);
				}
			}

			// TODO: Handle roles that exist in the DB but are no longer in our constants
		}
	}
}