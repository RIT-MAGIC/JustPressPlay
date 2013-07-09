using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using System.Data.Entity;
using WebMatrix.WebData;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			AuthConfig.RegisterAuth();

			// Database initialization
			Database.SetInitializer<JustPressPlayDBEntities>(null);
			using (UnitOfWork db = new UnitOfWork())
			{
				db.EntityContext.Database.Initialize(true);
			}

			// Make sure web security is set up
			if(!WebSecurity.Initialized)
				WebSecurity.InitializeDatabaseConnection(
					"JustPressPlayDBWebSecurity",	// The special connection string to bypass EF
					"user",							// Our users table
					"id",							// The primary key of the users table
					"username",						// The "username" column of the users table
					autoCreateTables: true			// Creates ASP tables if necessary
				);
		}
	}
}