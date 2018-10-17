using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace Sitecore.DeploymentToolKit.ContentChecker
{
    public class ContentCheckerCommand
    {
        public ContentCheckerCommand()
        {
            try
            {
                SetFilename();
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Content Checker", ex, this);
            }
        }

        public string Filename { set; get; }
    
        public void SetFilename()
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~");
                var directory = Settings.GetSetting("DeploymentToolKit.ContentChecker.Directory");

                var fulldirectory = path + directory;
                Filename = fulldirectory + Settings.GetSetting("DeploymentToolKit.ContentChecker.SnapshotFilename");
                CreateIfMissing(fulldirectory);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Content Checker", ex, this);
            }

        }
        public void CreateIfMissing(string path)
        {
            var folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }

        public string PerformGet(string sitecoreItem)
        {
            try
            {
                var jssUrl = Settings.GetSetting("DeploymentToolKit.ContentChecker.JssUrl");

                var key = Settings.GetSetting("DeploymentToolKit.ContentChecker.StandardJssKey");
                var jssKey = "&sc_apikey=" + key;
                
                var requestUrl =  jssUrl + sitecoreItem + jssKey;

                var request = (HttpWebRequest)WebRequest.Create(requestUrl);

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Content Checker", ex, this);

                return $"error{ex.Message}";
            }
            
        }
        public void SecondCheck()
        {
            try
            {
                var odt = DateTime.Now;
                var vw = Deserialize(Filename);
                if (vw.DataCheckerTable == null)
                {
                    vw.DataCheckerTable = PopulateContent();
                }

                foreach (var item in vw.DataCheckerTable)
                {
                    var response = PerformGet(item.Path);
                    item.SecondCheck = response;
                    item.SecondCheckDate = odt;
                }

                Serialization(vw, Filename);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Content Checker", ex, this);
            }
            
        }
        public void BaselineCheck()
        {
            try
            {
                var odt = DateTime.Now;
                var vw = Deserialize(Filename);
                if (vw.DataCheckerTable == null)
                {
                    vw.DataCheckerTable = PopulateContent();
                }

                foreach (var item in vw.DataCheckerTable)
                {
                    var response = PerformGet(item.Path);
                    item.BaselineContent = response;
                    item.BaselineContentDate = odt;
                }

                Serialization(vw, Filename);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Content Checker", ex, this);
            }
            
        }

        public ContentCheckerViewModel Fill()
        {
            var oContentCheckerVw = GetAll();
            if ((oContentCheckerVw.DataCheckerTable == null) || !(oContentCheckerVw.DataCheckerTable.Any()))
            {
                oContentCheckerVw.DataCheckerTable = PopulateContent();
            }

            return oContentCheckerVw;
        }
        

        public ContentCheckerViewModel GetAll()
        {
            var vw = Deserialize(Filename);
            return vw;
        }

        public List<ContentCheckerModel> PopulateContent()
        {
            var dataCheckerTable = new List<ContentCheckerModel>();

            var mainDirConfigParam = Settings.GetSetting("DeploymentToolKit.ContentChecker.WebsiteRoot");
            var checkForLayout = Settings.GetSetting("DeploymentToolKit.ContentChecker.CheckForLayout");

            var oContent = Context.Database.GetItem(mainDirConfigParam);

            var list = oContent.Axes.GetDescendants().OrderBy(t => t.Name).ToList();

            foreach (var item in list)
            {
                var oItem = new ContentCheckerModel();
                oItem.Path = item.Paths.Path;

                if (checkForLayout.ToLower() == "true")
                {
                    if (DoesItemHasPresentationDetails(item))
                    {
                        dataCheckerTable.Add(oItem);
                    }
                }
                else
                {
                    dataCheckerTable.Add(oItem);
                }
            }

            return dataCheckerTable;
        }

        public bool DoesItemHasPresentationDetails(Item item)
        {
            if (item != null)
            {
                return item.Fields[Sitecore.FieldIDs.LayoutField] != null
                       && !String.IsNullOrEmpty(item.Fields[FieldIDs.LayoutField].Value);
            }

            return false;
        }

        private void Serialization(ContentCheckerViewModel aStations, string aFileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ContentCheckerViewModel));
            
            using (var stream = File.Create(aFileName))
            {
                serializer.Serialize(stream, aStations);
                stream.Close();
            }
        }
        private ContentCheckerViewModel Deserialize(string aFileName)
        {
            var lst = new ContentCheckerViewModel();
            if (!File.Exists(aFileName))
            {
                return lst;
            }
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ContentCheckerViewModel));

                TextReader reader = new StreamReader(aFileName);

                object obj = deserializer.Deserialize(reader);

                lst = (ContentCheckerViewModel)obj;

                reader.Close();
                return lst;

            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("Contet Checker", ex, this);
                return lst;

            }

        }
      
        public string AddUrl(string path)
        {
            var vw = Deserialize(Filename);

            var oCandidate = new ContentCheckerModel {Path = path};
            if (vw.DataCheckerTable == null)
            {
                vw.DataCheckerTable = new List<ContentCheckerModel>();
            }
            vw.DataCheckerTable.Add(oCandidate);

            Serialization(vw, Filename);

            return "User added";

        }
        public string RemoveUrl(string user)
        {
            var lstRevertUsers = Deserialize(Filename);

            var item = lstRevertUsers.DataCheckerTable.FirstOrDefault(f => f.Path == user);
            lstRevertUsers.DataCheckerTable.Remove(item);

            Serialization(lstRevertUsers, Filename);

            return "Url Removed";
        }
    }
}