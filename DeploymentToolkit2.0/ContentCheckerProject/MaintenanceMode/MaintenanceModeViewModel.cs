using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Web.Authentication;

namespace Sitecore.DeploymentToolKit.MaintenanceMode
{
    [Serializable]
    public class MaintenanceModeViewModel
    {
        public List<MaintenanceModeModel> Users;
        public List<DomainAccessGuard.Session> UserSessions;
        public string Output;

        public string GetEmailTo()
        {
            return Users.Aggregate(string.Empty, (current, item) => current + (current + ";"));
        }
     
    }

}