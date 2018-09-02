using System.Linq;
using System.Web.Mvc;

namespace Sitecore.DeploymentToolKit.MaintenanceMode
{
    public class MaintenanceModeController : Controller
    {
        /// <summary>
        /// Wrapper class for Sitecore Job object
        /// </summary>
        public MaintenanceModeViewModel GetView()
        {
            var ommc = new MaintenanceModeCommand();

            var oCandidates = new MaintenanceModeViewModel();
            oCandidates.Users = ommc.GetCanditatesusers();
            oCandidates.UserSessions = ommc.GetNonAdminLoggedUsers();
            if (!oCandidates.Users.ToList().Any())
            {
                oCandidates.Users = ommc.GetCandidatesBlockedUsers();
            }

            return oCandidates;
        }
        [HttpPost]
        public ActionResult BlockUsers()
        {
            var ommc = new MaintenanceModeCommand();
            var output = ommc.LockOrUnlockUsers();
            var oCandidates = GetView();
            oCandidates.Output = output;

            return View("MaintenanceModeList", oCandidates);
        }

        [HttpPost]
        public ActionResult KillSessions()
        {
            var ommc = new MaintenanceModeCommand();
            var output = ommc.KillNonAdminSessions();
            var oCandidates = GetView();
            oCandidates.Output = output;

            return View("MaintenanceModeList", oCandidates);
        }

        [HttpGet]
        public ActionResult MaintenanceMode()
        {
            var oCandidates = GetView();
            oCandidates.Output = "";

            return View("MaintenanceModeList", oCandidates);
        }
    }
}