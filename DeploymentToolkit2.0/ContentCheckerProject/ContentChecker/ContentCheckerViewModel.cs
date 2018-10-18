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

        [XmlElement("JssLayoutServiceUrl")]
        public string Url => Settings.GetSetting("DeploymentToolKit.ContentChecker.JssLayoutServiceUrl");

        [XmlElement("DataCheckerTable")]
        public List<ContentCheckerModel> DataCheckerTable { set; get; }
        
    }
}