using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Sitecore.Configuration;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.Authentication;
using Sitecore.Web.UI.Sheer;


namespace Sitecore.DeploymentToolKit.MaintenanceMode
{
    public class MaintenanceModeCommand : Command
    {
        public MaintenanceModeCommand()
        {
            SetFilename();
        }
        private static readonly object Monitor = new object();

        public string Filename = "";
        public string Mappath = "";

        public void SetFilename()
        {
            Mappath = HttpContext.Current.Server.MapPath("~");
            
            Filename = Mappath + Settings.GetSetting("DeploymentToolKit.MaintenanceMode.xml", @"upload\\maintenancemode.xml");
        }

        /// <summary>
        /// Locks or Unlock the users. the return is  the Output message from the software
        /// </summary>
        /// <returns></returns>
        public string LockOrUnlockUsers()
        {
            try
            {
                string result = String.Empty;

                if (!HasWriteAccessToFolder(Mappath + "\\upload"))
                {
                    result = "The Module needs permission to the folder upload in order to be executed. Operation Failed";
                }
                else
                {
                    var lstRevertUsers = Deserialize(Filename);
                    if (lstRevertUsers.Count > 0)
                    {
                        if (Restore(lstRevertUsers))
                        {
                            result = "Users Unlocked:" + lstRevertUsers.Count;
                            try
                            {
                                File.Delete(Filename);
                            }
                            catch (Exception)
                            {
                                result = "Failed to delete the restoration file";
                            }

                        }
                        else
                        {
                            result = "Failed to Unlocked";
                        }
                    }
                    else if ((lstRevertUsers.Count == 0) && (File.Exists(Filename)))
                    {
                        result = "Could not load restore file";
                    }
                    else
                    {

                        List<MaintenanceModeModel> lstUsersNotLocked = GetCanditatesusers();



                        if (LockUsers(lstUsersNotLocked))
                        {
                            result += "Successfully locked users :" + lstUsersNotLocked.Count.ToString();
                        }
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return ex.Message;
            }
        }

        public override void Execute(CommandContext context)
        {
            if (context == null)
                return;

            //string result = LockOrUnlockUsers();
            //SheerResponse.Alert(result, new string[0]);
            SheerResponse.ShowModalDialog("/sitecore/client/Your%20Apps/MaintenanceMode/ListPage?sc_lang=en");

        }

        public void Refresh(params object[] parameters)
        {
            // Do Stuff
        }

        public bool LockUsers(List<MaintenanceModeModel> lst)
        {

            string userlist = lst.Aggregate(string.Empty, (current, u) => current + ("'" + u.ID + "',"));
            if (userlist.Length == 0)
            {
                return false;

            }
            userlist = userlist.Substring(0, userlist.Length - 1);

            //set items as locked
            foreach (var item in lst)
            {
                item.Locked = "True";
            }

            Serialization(lst, Filename);

            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["core"].ConnectionString;

                SqlConnection sqlConnection1 = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                string query = "UPDATE [aspnet_Membership] SET isLockedOut = 1 where UserId in (" + userlist + ")";

                cmd.CommandText = query;

                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();

                var lstNotLocked = new List<string>();
                while (reader.Read())
                {
                    var myString = reader.GetString(0);
                    lstNotLocked.Add(myString);

                }

                //SAVE USERS
                sqlConnection1.Close();
                return true;

            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return false;

            }
        }

        public bool Restore(List<MaintenanceModeModel> lst)
        {
            var userlist = lst.Aggregate(string.Empty, (current, u) => current + ("'" + u.ID + "',"));
            if (userlist.Length == 0)
            {
                return false;

            }
            userlist = userlist.Substring(0, userlist.Length - 1);


            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["core"].ConnectionString;

                SqlConnection sqlConnection1 = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                string query = "UPDATE [aspnet_Membership] SET isLockedOut = 0 where UserId in (" + userlist + ")";

                cmd.CommandText = query;

                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();

                List<string> lstNotLocked = new List<string>();
                while (reader.Read())
                {
                    var myString = reader.GetString(0);
                    lstNotLocked.Add(myString);

                }

                //SAVE USERS
                sqlConnection1.Close();
                return true;

            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return false;

            }
        }

        private bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public List<MaintenanceModeModel> GetCanditatesusers()
        {

            //get the list of users that can be subjet to this change
            var lst = Sitecore.Security.Accounts.UserManager.GetUsers().
                Where(c => c.IsAdministrator == false && c.Domain.Name == "sitecore" && c.LocalName != "ServicesAPI")
                .ToList();

            string userlist = lst.Aggregate(string.Empty, (current, u) => current + ("'" + u.DisplayName + "',"));

            userlist = userlist.Substring(0, userlist.Length - 1);

            List<MaintenanceModeModel> lstUsersNotLocked = GetUsersNotLocked(userlist);
            return lstUsersNotLocked;


        }

        public List<MaintenanceModeModel> GetCandidatesBlockedUsers()
        {
            List<MaintenanceModeModel> lstRevertUsers = Deserialize(Filename);

            return lstRevertUsers;
        }

        public List<MaintenanceModeModel> GetUsersNotLocked(string users)
        {
            List<MaintenanceModeModel> lstNotLocked = new List<MaintenanceModeModel>();

            try
            {

                string connectionString = ConfigurationManager.ConnectionStrings["core"].ConnectionString;

                SqlConnection sqlConnection1 = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                string query =
                    "SELECT aspnet_Users.UserId,username,IsLockedOut,Email FROM [dbo].[aspnet_Membership],aspnet_Users where [aspnet_Membership].UserId = aspnet_Users.UserId ";
                query += " and IsLockedOut =0 and username in (" + users + ")";

                cmd.CommandText = query;

                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();

                reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    var guid = reader.GetGuid(0).ToString();
                    var name = reader.GetString(1).ToString();
                    var isLocked = reader.GetBoolean(2).ToString();
                    var email = reader.GetString(3).ToString();
                    var oCandidate = new MaintenanceModeModel();

                    oCandidate.ID = guid;
                    oCandidate.UserName = name;
                    oCandidate.Locked = isLocked;
                    oCandidate.Email = email;


                    lstNotLocked.Add(oCandidate);

                }

                //SAVE USERS
                sqlConnection1.Close();
                return lstNotLocked;

            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return lstNotLocked;
            }


        }

        private void Serialization(IList<MaintenanceModeModel> aStations, string aFileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<MaintenanceModeModel>));

            using (var stream = File.OpenWrite(aFileName))
            {
                serializer.Serialize(stream, aStations);
                stream.Close();
            }

        }

        private List<MaintenanceModeModel> Deserialize(string aFileName)
        {
            List<MaintenanceModeModel> lst = new List<MaintenanceModeModel>();
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<MaintenanceModeModel>));

                TextReader reader = new StreamReader(aFileName);

                object obj = deserializer.Deserialize(reader);

                lst = (List<MaintenanceModeModel>)obj;

                reader.Close();
                return lst;

            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error("MaintenanceModeCommand", ex, this);
                return lst;

            }

        }

        public List<DomainAccessGuard.Session> GetNonAdminLoggedUsers()
        {
            List<DomainAccessGuard.Session> lstNonAdminUsersLogged = new List<DomainAccessGuard.Session>();
            var lstSession = GetUserSessions().ToList();
            var lstCandidates = GetCanditatesusers();
            foreach (DomainAccessGuard.Session session in lstSession)
            {
                var lstTotal = lstCandidates.Where(c => session.UserName == c.UserName).ToList();
                if (lstTotal.Count > 0)
                {
                    lstNonAdminUsersLogged.Add(session);
                }
            }
            return lstNonAdminUsersLogged;

        }

        public IEnumerable<DomainAccessGuard.Session> GetUserSessions()
        {
            return DomainAccessGuard.Sessions.OrderByDescending(s => s.LastRequest);
        }


        public void KickUserSession(string sessionId, string userName)
        {
            DomainAccessGuard.Kick(sessionId);

        }
        public string KillNonAdminSessions()
        {
            var lst = GetNonAdminLoggedUsers();

            foreach (var session in lst)
            {
                KickUserSession(session.SessionID, session.UserName);
            }

            return "users kicked out";
        }
    }
}