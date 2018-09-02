using System.Web.Mvc;

namespace Sitecore.DeploymentToolKit.ContentChecker
{
    public class DeploymentToolKitController : Controller
    {
       
        [HttpGet]
        public ActionResult ToolKit()
        {
            return this.View("DeploymentToolkit");
        }
    }
}