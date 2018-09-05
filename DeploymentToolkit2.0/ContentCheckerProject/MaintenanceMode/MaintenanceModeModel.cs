using System;

namespace Sitecore.DeploymentToolKit.MaintenanceMode
{
    [Serializable]
    public class MaintenanceModeModel
    {
        public string UserName { get; set; }
        public string ID { get; set; }
        public string Locked { get; set; }
        public string Email { get; set; }

    }
}