using System.Linq;
using System.Web.Mvc;

namespace Sitecore.DeploymentToolKit.ContentChecker
{
    public class ContentCheckerController : Controller
    {
        /// <summary>
        /// Wrapper class for Sitecore Job object
        /// </summary>
        [HttpPost]
        public ActionResult SecondCheck()
        {
            var ommc = new ContentCheckerCommand();
            ommc.SecondCheck();

            var oCandidates = GetAll();

            return View("ContentCheckerResultTable", oCandidates);
        }
        [HttpPost]
        public ActionResult BaselineCheck()
        {
            var ommc = new ContentCheckerCommand();
            ommc.BaselineCheck();

            var oContentCheckerVw = GetAll();

            return View("ContentCheckerResultTable", oContentCheckerVw);
        }
        public ContentCheckerViewModel GetAll()
        {
            var ommc = new ContentCheckerCommand();
            var oContentCheckerVw = ommc.GetAll();
            if ((oContentCheckerVw.DataCheckerTable == null) || !(oContentCheckerVw.DataCheckerTable.Any()))
            {
                oContentCheckerVw.DataCheckerTable = ommc.PopulateContent();
            }

            return oContentCheckerVw;
        }
        
        [HttpPost]
        public ActionResult AddKey(string key)
        {
            var ommc = new ContentCheckerCommand();
            ommc.AddKey(key);
            var oContentCheckerVw = GetAll();
            return View("ContentCheckerResultTable", oContentCheckerVw);
        }

        [HttpPost]
        public ActionResult AddUrl(string url)
        {
            var ommc = new ContentCheckerCommand();
            ommc.AddUrl(url);
            var oContentCheckerVw = GetAll();
            return View("ContentCheckerResultTable", oContentCheckerVw);
        }
        [HttpPost]
        public ActionResult RemoveUrl(string param1)
        {
            var ommc = new ContentCheckerCommand();
            ommc.RemoveUrl(param1);
            var oContentCheckerVw = GetAll();

            return View("ContentCheckerResultTable", oContentCheckerVw);
        }

        [HttpGet]
        public ActionResult ContentChecker()
        {
            var oContentCheckerVw = GetAll();
            return View("ContentCheckerList", oContentCheckerVw);
        }
        [HttpGet]
        public ActionResult ToolKit()
        {
            var oContentCheckerVw = GetAll();

            return View("DeploymentToolkit", oContentCheckerVw);
        }
    }
}