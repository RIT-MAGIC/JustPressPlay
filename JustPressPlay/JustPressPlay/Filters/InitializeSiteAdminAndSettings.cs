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
        else if (!Convert.ToBoolean(JPPConstants.SiteSettings.GetValue(JPPConstants.SiteSettings.SiteInitialized)))
        {
            filterContext.Result = new RedirectResult("/InitializeSite/InitializeSiteSettings");
        }
    }
}
