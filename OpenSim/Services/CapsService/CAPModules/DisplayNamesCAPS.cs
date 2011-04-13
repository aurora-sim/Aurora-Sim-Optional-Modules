﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;
using Nini.Config;
using Aurora.Simulation.Base;
using OpenSim.Services.Interfaces;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Framework.Capabilities;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

using OpenMetaverse;
using Aurora.DataManager;
using Aurora.Framework;
using Aurora.Services.DataService;
using OpenMetaverse.StructuredData;

namespace OpenSim.Services.CapsService
{
    public class DisplayNamesCAPS : ICapsServiceConnector
    {
        private IRegionClientCapsService m_service;
        private IUserAccountService m_userService;
        private IEventQueueService m_eventQueue;
        private IProfileConnector m_profileConnector;
        private List<string> bannedNames = new List<string> ();

        #region Stream Handler

        public delegate byte[] StreamHandlerCallback(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse);

        public class StreamHandler : BaseStreamHandler
        {
            StreamHandlerCallback m_callback;

            public StreamHandler(string httpMethod, string path, StreamHandlerCallback callback)
                : base (httpMethod, path)
            {
                m_callback = callback;
            }

            public override byte[] Handle(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
            {
                return m_callback (path, request, httpRequest, httpResponse);
            }
        }

        #endregion

        #region ICapsServiceConnector Members

        public void RegisterCaps(IRegionClientCapsService service)
        {
            IConfig displayNamesConfig = service.ClientCaps.Registry.RequestModuleInterface<ISimulationBase>().ConfigSource.Configs["DisplayNamesModule"];
            if (displayNamesConfig != null)
            {
                if (!displayNamesConfig.GetBoolean ("Enabled", true))
                    return;
                string bannedNamesString = displayNamesConfig.GetString ("BannedUserNames", "");
                if (bannedNamesString != "")
                    bannedNames = new List<string> (bannedNamesString.Split (','));
            }
            m_service = service;
            m_profileConnector = Aurora.DataManager.DataManager.RequestPlugin<IProfileConnector> ();
            m_eventQueue = service.Registry.RequestModuleInterface<IEventQueueService> ();
            m_userService = service.Registry.RequestModuleInterface<IUserAccountService> ();

            string post = CapsUtil.CreateCAPS ("SetDisplayName", "");
            service.AddCAPS ("SetDisplayName", post);
            service.AddStreamHandler ("SetDisplayName", new RestHTTPHandler ("POST", post,
                                                      ProcessSetDisplayName));

            post = CapsUtil.CreateCAPS ("GetDisplayNames", "");
            service.AddCAPS ("GetDisplayNames", post);
            service.AddStreamHandler ("GetDisplayNames", new StreamHandler ("GET", post,
                                                      ProcessGetDisplayName));
        }

        public void EnteringRegion()
        {
        }

        public void DeregisterCaps()
        {
        }

        #endregion

        #region Caps Messages

        /// <summary>
        /// Set the display name for the given user
        /// </summary>
        /// <param name="mDhttpMethod"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        private Hashtable ProcessSetDisplayName(Hashtable mDhttpMethod)
        {
            try
            {
                OSDMap rm = (OSDMap)OSDParser.DeserializeLLSDXml ((string)mDhttpMethod["requestbody"]);
                OSDArray display_name = (OSDArray)rm["display_name"];
                string oldDisplayName = display_name[0].AsString ();
                string newDisplayName = display_name[1].AsString ();
                UserAccount account = m_userService.GetUserAccount (UUID.Zero, m_service.AgentID);

                //Check to see if their name contains a banned character
                foreach (string bannedUserName in bannedNames)
                {
                    string BannedUserName = bannedUserName.Replace (" ", "");
                    if (newDisplayName.ToLower ().Contains (BannedUserName.ToLower ()))
                    {
                        //Revert the name to the original and send them a warning
                        newDisplayName = account.Name;
                        //m_avatar.ControllingClient.SendAlertMessage ("You cannot update your display name to the name chosen, your name has been reverted. This request has been logged.");
                        break; //No more checking
                    }
                }

                IUserProfileInfo info = m_profileConnector.GetUserProfile (m_service.AgentID);
                if (info == null)
                {
                    //m_avatar.ControllingClient.SendAlertMessage ("You cannot update your display name currently as your profile cannot be found.");
                }
                else
                {
                    //Set the name
                    info.DisplayName = newDisplayName;
                    m_profileConnector.UpdateUserProfile (info);

                    //One for us
                    DisplayNameUpdate (newDisplayName, oldDisplayName, account, m_service.AgentID);

                    foreach (IRegionClientCapsService avatar in m_service.RegionCaps.GetClients ())
                    {
                        if (avatar.AgentID != m_service.AgentID)
                        {
                            //Update all others
                            DisplayNameUpdate (newDisplayName, oldDisplayName, account, avatar.AgentID);
                        }
                    }
                    //The reply
                    SetDisplayNameReply (newDisplayName, oldDisplayName, account);
                }
            }
            catch
            {
            }
            //Send back data
            Hashtable responsedata = new Hashtable ();
            responsedata["int_response_code"] = 200; //501; //410; //404;
            responsedata["content_type"] = "text/plain";
            responsedata["keepalive"] = false;
            responsedata["str_response_string"] = "";

            return responsedata;
        }

        /// <summary>
        /// Get the user's display name, currently not used?
        /// </summary>
        /// <param name="mDhttpMethod"></param>
        /// <param name="agentID"></param>
        /// <returns></returns>
        private byte[] ProcessGetDisplayName(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            //I've never seen this come in, so for now... do nothing
            NameValueCollection query = HttpUtility.ParseQueryString (httpRequest.Url.Query);
            string[] ids = query.GetValues ("ids");
            string username = query.GetOne ("username");

            OSDMap map = new OSDMap ();
            OSDArray agents = new OSDArray ();
            OSDArray bad_ids = new OSDArray ();
            OSDArray bad_usernames = new OSDArray ();

            if (ids != null)
            {
                foreach (string id in ids)
                {
                    UserAccount account = m_userService.GetUserAccount (UUID.Zero, UUID.Parse (id));
                    if (account != null)
                    {
                        IUserProfileInfo info = Aurora.DataManager.DataManager.RequestPlugin<IProfileConnector> ().GetUserProfile (UUID.Parse (id));
                        if (info != null)
                            PackUserInfo (info, account, ref agents);
                        else
                            bad_ids.Add (id);
                    }
                }
            }
            //TODO: usernames

            map["agents"] = agents;
            map["bad_ids"] = bad_ids;
            map["bad_usernames"] = bad_usernames;

            byte[] m = OSDParser.SerializeLLSDXmlBytes (map);
            httpResponse.Body.Write (m, 0, m.Length);
            httpResponse.StatusCode = (int)System.Net.HttpStatusCode.OK;
            httpResponse.Send ();
            return null;
        }

        private void PackUserInfo(IUserProfileInfo info, UserAccount account, ref OSDArray agents)
        {
            OSDMap agentMap = new OSDMap ();
            agentMap["username"] = account.Name;
            agentMap["display_name"] = info.DisplayName;
            agentMap["display_name_next_update"] = OSD.FromDate (DateTime.ParseExact ("1970-01-01 00:00:00 +0", "yyyy-MM-dd hh:mm:ss z", System.Globalization.DateTimeFormatInfo.InvariantInfo).ToUniversalTime ());
            agentMap["legacy_first_name"] = account.FirstName;
            agentMap["legacy_last_name"] = account.LastName;
            agentMap["id"] = info.PrincipalID;
            agentMap["is_display_name_default"] = isDefaultDisplayName (account.FirstName, account.LastName, account.Name, info.DisplayName);

            agents.Add (agentMap);
        }

        #region Event Queue

        /// <summary>
        /// Send the user a display name update
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="oldDisplayName"></param>
        /// <param name="InfoFromAv"></param>
        /// <param name="ToAgentID"></param>
        public void DisplayNameUpdate(string newDisplayName, string oldDisplayName, UserAccount InfoFromAv, UUID ToAgentID)
        {
            if (m_eventQueue != null)
            {
                //If the DisplayName is blank, the client refuses to do anything, so we send the name by default
                if (newDisplayName == "")
                    newDisplayName = InfoFromAv.Name;

                bool isDefaultName = isDefaultDisplayName (InfoFromAv.FirstName, InfoFromAv.LastName, InfoFromAv.Name, newDisplayName);

                OSD item = DisplayNameUpdate (newDisplayName, oldDisplayName, InfoFromAv.PrincipalID, isDefaultName, InfoFromAv.FirstName, InfoFromAv.LastName, InfoFromAv.FirstName + "." + InfoFromAv.LastName);
                m_eventQueue.Enqueue (item, ToAgentID, m_service.RegionHandle);
            }
        }

        private bool isDefaultDisplayName(string first, string last, string name, string displayName)
        {
            if (displayName == name)
                return true;
            else if (displayName == first + "." + last)
                return true;
            return false;
        }

        /// <summary>
        /// Reply to the set display name reply
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="oldDisplayName"></param>
        /// <param name="m_avatar"></param>
        public void SetDisplayNameReply(string newDisplayName, string oldDisplayName, UserAccount m_avatar)
        {
            if (m_eventQueue != null)
            {
                bool isDefaultName = isDefaultDisplayName (m_avatar.FirstName, m_avatar.LastName, m_avatar.Name, newDisplayName);

                OSD item = DisplayNameReply (newDisplayName, oldDisplayName, m_avatar.PrincipalID, isDefaultName, m_avatar.FirstName, m_avatar.LastName, m_avatar.FirstName + "." + m_avatar.LastName);
                m_eventQueue.Enqueue (item, m_avatar.PrincipalID, m_service.RegionHandle);
            }
        }

        /// <summary>
        /// Tell the user about an update
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="oldDisplayName"></param>
        /// <param name="ID"></param>
        /// <param name="isDefault"></param>
        /// <param name="First"></param>
        /// <param name="Last"></param>
        /// <param name="Account"></param>
        /// <returns></returns>
        public OSD DisplayNameUpdate(string newDisplayName, string oldDisplayName, UUID ID, bool isDefault, string First, string Last, string Account)
        {
            OSDMap nameReply = new OSDMap ();
            nameReply.Add ("message", OSD.FromString ("DisplayNameUpdate"));

            OSDMap body = new OSDMap ();
            OSDMap agentData = new OSDMap ();
            agentData.Add ("display_name", OSD.FromString (newDisplayName));
            agentData.Add ("display_name_next_update", OSD.FromDate (DateTime.ParseExact ("1970-01-01 00:00:00 +0", "yyyy-MM-dd hh:mm:ss z", System.Globalization.DateTimeFormatInfo.InvariantInfo).ToUniversalTime ()));
            agentData.Add ("id", OSD.FromUUID (ID));
            agentData.Add ("is_display_name_default", OSD.FromBoolean (isDefault));
            agentData.Add ("legacy_first_name", OSD.FromString (First));
            agentData.Add ("legacy_last_name", OSD.FromString (Last));
            agentData.Add ("username", OSD.FromString (Account));
            body.Add ("agent", agentData);
            body.Add ("agent_id", OSD.FromUUID (ID));
            body.Add ("old_display_name", OSD.FromString (oldDisplayName));

            nameReply.Add ("body", body);

            return nameReply;
        }

        /// <summary>
        /// Send back a user's display name
        /// </summary>
        /// <param name="newDisplayName"></param>
        /// <param name="oldDisplayName"></param>
        /// <param name="ID"></param>
        /// <param name="isDefault"></param>
        /// <param name="First"></param>
        /// <param name="Last"></param>
        /// <param name="Account"></param>
        /// <returns></returns>
        public OSD DisplayNameReply(string newDisplayName, string oldDisplayName, UUID ID, bool isDefault, string First, string Last, string Account)
        {
            OSDMap nameReply = new OSDMap ();

            OSDMap body = new OSDMap ();
            OSDMap content = new OSDMap ();
            OSDMap agentData = new OSDMap ();
            content.Add ("display_name", OSD.FromString (newDisplayName));
            content.Add ("display_name_next_update", OSD.FromDate (DateTime.ParseExact ("1970-01-01 00:00:00 +0", "yyyy-MM-dd hh:mm:ss z", System.Globalization.DateTimeFormatInfo.InvariantInfo).ToUniversalTime ()));
            content.Add ("id", OSD.FromUUID (ID));
            content.Add ("is_display_name_default", OSD.FromBoolean (isDefault));
            content.Add ("legacy_first_name", OSD.FromString (First));
            content.Add ("legacy_last_name", OSD.FromString (Last));
            content.Add ("username", OSD.FromString (Account));
            body.Add ("content", content);
            body.Add ("agent", agentData);
            body.Add ("reason", OSD.FromString ("OK"));
            body.Add ("status", OSD.FromInteger (200));

            nameReply.Add ("body", body);
            nameReply.Add ("message", OSD.FromString ("SetDisplayNameReply"));

            return nameReply;
        }

        #endregion

        #endregion
    }
}
