using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sitecore.DeploymentToolKit.ContentChecker
{
    [Serializable]
    public class ContentCheckerViewModel
    {
        [XmlElement("Key")]
        public string Key { set; get; }

        [XmlElement("DataCheckerTable")]
        public List<ContentCheckerModel> DataCheckerTable { set; get; }
        
    }
}