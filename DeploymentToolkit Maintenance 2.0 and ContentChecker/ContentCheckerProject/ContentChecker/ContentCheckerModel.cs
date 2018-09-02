using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                string val1 = BaselineContent;
                string val2 = SecondCheck;

                MatchCollection words1 = Regex.Matches(val1, @"\b(\w+)\b");
                MatchCollection words2 = Regex.Matches(val2, @"\b(\w+)\b");

                var hs1 = new HashSet<string>(words1.Cast<Match>().Select(m => m.Value));
                var hs2 = new HashSet<string>(words2.Cast<Match>().Select(m => m.Value));

                // Optionaly you can use a custom comparer for the words.
                // var hs2 = new HashSet<string>(words2.Cast<Match>().Select(m => m.Value), new MyComparer());

                // h2 contains after this operation only 'very' and 'Joe'
                hs2.ExceptWith(hs1);

                var diff = Empty;
                foreach (var lst in hs2.ToList())
                {
                    diff += lst;
                }
                return diff;
            }

            return Empty;
        }

        public string SecondCheckDateTime()
        {
            if (SecondCheckDate.HasValue)
            {
                return SecondCheckDate.Value.ToShortDateString();
            }
            return "";
        }

        public string BaselineContentDateTime()
        {
            if (BaselineContentDate.HasValue)
            {
                return BaselineContentDate.Value.ToShortDateString();
            }
            return "";
        }

    }
}