﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;


namespace Sitecore.DeploymentToolKit.ContentChecker
{
    public class ContentCheckerCommand : Command
    {
        public ContentCheckerCommand()
        {
            SetFilename();
        }
        public string Filename { set; get; }
        public string Mappath  { set; get; }
        public  void SetFilename()
        {
             Mappath = HttpContext.Current.Server.MapPath("~");
          
            Filename = Mappath + Settings.GetSetting("DeploymentToolKit.ContentChecker.xml"); 
        }
        public override void Execute(CommandContext context)
        {
            if (context == null)
                return;
            SheerResponse.ShowModalDialog(Settings.GetSetting("DeploymentToolKit.ContentChecker.Checker")); 
        }
        public void Refresh(params object[] parameters)
        {
            // Do Stuff
        }
        public string PerformGet(string url)
        {
            try
            {
                var part1 = Settings.GetSetting("DeploymentToolKit.ContentChecker.JssUrl", "http://sxatest.local/sitecore/api/layout/render/jss?item=");
                var part2 = url;
                var key = GetAll().Key;
                if (key == string.Empty)
                {
                    key = Settings.GetSetting("DeploymentToolKit.ContentChecker.StandardJssKey");
                }
                var part3 = "&sc_apikey=" + key;

                var request = (HttpWebRequest)WebRequest.Create(part1 + part2 + part3);

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return responseString;
            }
            catch (Exception)
            {
                return "error";
            }
            
        }
        public void SecondCheck()
        {
            var odt = DateTime.Now;
            var vw = Deserialize(Filename);

            foreach (var item in vw.DataCheckerTable)
            {
                var respose = PerformGet(item.Path);
                item.SecondCheck = respose;
                item.SecondCheckDate = odt;
            }

            Serialization(vw, Filename);
        }
        public void BaselineCheck()
        {
            var odt = DateTime.Now;
            var vw = Deserialize(Filename);

            if (!vw.DataCheckerTable.Any())
            {
                vw.DataCheckerTable = PopulateContent();
            }

            foreach (var item in vw.DataCheckerTable)
            {
                var respose = PerformGet(item.Path);
                item.BaselineContent = respose;
                item.BaselineContentDate = odt;
            }

            Serialization(vw, Filename);
        }
        public ContentCheckerViewModel GetAll()
        {
            var vw = Deserialize(Filename);
            return vw;
        }
        public List<ContentCheckerModel> PopulateContent()
        {
            var dataCheckerTable = new List<ContentCheckerModel>();

            var mainDirConfigParam = Settings.GetSetting("DeploymentToolKit.ContentChecker.MainDir");
            var oContent = Context.Database.GetItem(mainDirConfigParam);

            var list = oContent.Axes.GetDescendants();

            foreach (var homeItem in list)
            {
                var fullList = homeItem.Axes.GetDescendants();

                foreach (var item in fullList)
                {
                    var oItem = new ContentCheckerModel();
                    oItem.Path = item.Paths.Path.Replace(homeItem.Paths.Path, "");

                    if (DoesItemHasPresentationDetails(item))
                    {
                        dataCheckerTable.Add(oItem);
                    }
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
                Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return lst;

            }

        }
        public string AddKey(string key)
        {
            var contentCheckerVw = Deserialize(Filename);
            contentCheckerVw.Key = key;

            Serialization(contentCheckerVw, Filename);

            return "User added";

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