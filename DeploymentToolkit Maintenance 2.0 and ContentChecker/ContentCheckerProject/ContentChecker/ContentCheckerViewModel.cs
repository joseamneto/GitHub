using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Sitecore.Configuration;

namespace Sitecore.DeploymentToolKit.ContentChecker
{
    [Serializable]
    public class ContentCheckerViewModel
    {
        [XmlElement("Key")]
        public string Key => Settings.GetSetting("DeploymentToolKit.ContentChecker.StandardJssKey");

        [XmlElement("Key")]
        public string Url => Settings.GetSetting("DeploymentToolKit.ContentChecker.JssUrl");

        [XmlElement("DataCheckerTable")]
        public List<ContentCheckerModel> DataCheckerTable { set; get; }
        
    }
}