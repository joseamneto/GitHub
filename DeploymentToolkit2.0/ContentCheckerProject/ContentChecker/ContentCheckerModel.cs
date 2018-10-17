using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using static System.String;


namespace Sitecore.DeploymentToolKit.ContentChecker
{
    [Serializable]
    public class ContentCheckerModel
    {
        public string Path { get; set; }
        public string BaselineContent { get; set; }
        public DateTime? BaselineContentDate { get; set; }
        public string SecondCheck { get; set; }
        public DateTime? SecondCheckDate { get; set; }

        public string ContainsBaseLineContent()
        {
            if (IsNullOrEmpty(BaselineContent))
            {
                return "Empty";
            }

            return BaselineContent == "error" ? BaselineContent : "Data saved";

        }
        public string ContainsSecondContent()
        {
            if (IsNullOrEmpty(SecondCheck))
            {
                return "Empty";
            }

            return BaselineContent == "error" ? BaselineContent : "Data saved";
        }
        public string CheckDifference()
        {
            if ((!IsNullOrEmpty(BaselineContent)) && (!IsNullOrEmpty(SecondCheck)))
            {
                var leftJson = BaselineContent;
                var rightJson = SecondCheck;
                var jdp = new JsonDiffPatch();
                JToken diffResult = jdp.Diff(leftJson, rightJson);
                
                return diffResult.ToString();
            }

            return Empty;
        }

        public string SecondCheckDateTime()
        {
            if (SecondCheckDate.HasValue)
            {
                return SecondCheckDate.Value.ToShortDateString() + " " + SecondCheckDate.Value.ToShortTimeString();
            }
            return "";
        }

        public string BaselineContentDateTime()
        {
            if (BaselineContentDate.HasValue)
            {
                return BaselineContentDate.Value.ToShortDateString() + " " + BaselineContentDate.Value.ToShortTimeString();
            }
            return "";
        }

    }
}