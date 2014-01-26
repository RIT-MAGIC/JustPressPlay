using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;

using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;
using JustPressPlay.ViewModels;
using JustPressPlay.Utilities;
using System.Web.Security;

public class InitializeSiteAdminAndSettings : AuthorizeAttribute
{
    /// <summary>
    /// What to do while authorizing - This occurs AFTER the base controller's OnAuthorization
    /// </summary>
    /// <param name="filterContext">Info about the authorization</param>
    public override void OnAuthorization(AuthorizationContext filterContext)
    {        
        if (!Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.AdminAccountCreated)))
        {
            filterContext.Result = new RedirectResult("/InitializeSite");
        }

        if (!Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SiteInitialized)))
        {
            filterContext.Result = new RedirectResult("/InitializeSite/InitializeSiteSettings");
        }
    }
}
