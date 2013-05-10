/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using Aurora.Framework.ClientInterfaces;
using Aurora.Framework.ConsoleFramework;
using Aurora.Framework.DatabaseInterfaces;
using Aurora.Framework.Modules;
using Aurora.Framework.Servers;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework.Servers.HttpServer.Implementation;
using Aurora.Framework.Servers.HttpServer.Interfaces;
using Aurora.Framework.Services;
using Aurora.Framework.Services.ClassHelpers.Profile;
using Aurora.Framework.Utilities;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace OpenSim.Services
{
    /// <summary>
    /// Reference:
    /// http://wiki.secondlife.com/wiki/Registration_API_Reference
    /// http://wiki.secondlife.com/wiki/Registration_API
    /// </summary>
    public class RegAPIHandler : IService
    {
        public IHttpServer m_server = null;
        public string Name
        {
            get { return GetType().Name; }
        }

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            IConfig handlerConfig = config.Configs["Handlers"];
            if (handlerConfig.GetString("RegApiHandler", "") != Name)
                return;
            m_server = registry.RequestModuleInterface<ISimulationBase>().GetHttpServer(handlerConfig.GetUInt("WireduxHandlerPort"));
            //This handler allows sims to post CAPS for their sims on the CAPS server.
            m_server.AddStreamHandler(new RegApiHTTPHandler(registry, m_server));
        }

        public void FinishedStartup()
        {
        }
    }

    public class RegApiHTTPHandler : BaseRequestHandler
    {
        protected IRegistryCore m_registry;
        protected IHttpServer m_server;
        public const int RegApiAllowed = 512;
        public const int RegApiAddToGroup = 1024;
        public const int RegApiCheckName = 2048;
        public const int RegApiCreateUser = 4096;
        public const int RegApiGetErrorCodes = 8192;
        public const int RegApiGetLastNames = 16384;
        public Dictionary<int, string> m_lastNameRegistry = new Dictionary<int, string>();

        public RegApiHTTPHandler(IRegistryCore reg, IHttpServer s) :
            base("POST", "/get_reg_capabilities")
        {
            m_registry = reg;
            m_server = s;
        }

        public override byte[] Handle(string path, Stream requestData,
                OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            string body = HttpServerHandlerHelpers.ReadString(requestData);

            //MainConsole.Instance.DebugFormat("[XXX]: query String: {0}", body);
            try
            {
                OSDMap map = (OSDMap)OSDParser.DeserializeLLSDXml(body);
                //Make sure that the person who is calling can access the web service
                if (map["submit"] == "Get Capabilities")
                {

                }
            }
            catch (Exception)
            {
            }
            OSDMap resp = new OSDMap();
            resp.Add("response", OSD.FromString("Failed"));
            string xmlString = OSDParser.SerializeLLSDXmlString(resp);
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(xmlString);
        }

        private byte[] ProcessLogin(OSDMap map)
        {
            bool Verified = false;
            string FirstName = map["first_name"].AsString();
            string LastName = map["last_name"].AsString();
            string Password = map["password"].AsString();

            ILoginService loginService = m_registry.RequestModuleInterface<ILoginService>();

            Verified = loginService.VerifyClient(UUID.Zero, FirstName + " " + LastName, "UserAccount", Password);
            
            OSDMap resp = new OSDMap();
            if (Verified)
            {
                UserAccount account = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(null, FirstName, LastName);
                if (Verified)
                {
                    AddCapsUrls(resp, account);
                }
            }
            string xmlString = OSDParser.SerializeLLSDXmlString(resp);
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(xmlString);
        }

        private void AddCapsUrls(OSDMap resp, UserAccount account)
        {
            //Check whether they can use the Api
            if ((account.UserFlags & RegApiAllowed) == RegApiAllowed)
            {
                if ((account.UserFlags & RegApiAddToGroup) == RegApiAddToGroup)
                    resp["add_to_group"] = AddSpecificUrl("add_to_group");

                if ((account.UserFlags & RegApiCheckName) == RegApiCheckName)
                    resp["check_name"] = AddSpecificUrl("check_name");

                if ((account.UserFlags & RegApiCreateUser) == RegApiCreateUser)
                    resp["create_user"] = AddSpecificUrl("create_user");

                if ((account.UserFlags & RegApiGetErrorCodes) == RegApiGetErrorCodes)
                    resp["get_error_codes"] = AddSpecificUrl("get_error_codes");

                if ((account.UserFlags & RegApiGetLastNames) == RegApiGetLastNames)
                    resp["get_last_names"] = AddSpecificUrl("get_last_names");
            }
        }

        /// <summary>
        /// Creates a cap for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string AddSpecificUrl(string type)
        {
            string capPath = "/cap/"+UUID.Random()+"/"+type;
            m_server.AddStreamHandler(new GenericStreamHandler("GET", capPath, 
                delegate(string path, Stream request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
                {
                    httpResponse.ContentType = "text/html";

                    OSD resp = new OSD();
                    try
                    {
                        OSDMap r = (OSDMap)OSDParser.DeserializeLLSDXml(HttpServerHandlerHelpers.ReadFully(request));

                        if (type == "add_to_group")
                            resp = AddUserToGroup(r);

                        if (type == "check_name")
                            resp = CheckName(r);

                        if (type == "create_user")
                            resp = CreateUser(r);

                        if (type == "get_error_codes")
                            resp = GetErrorCode(r);

                        if (type == "get_last_names")
                            resp = GetLastNames(r);
                    }
                    catch
                    {
                    }

                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    return OSDParser.SerializeLLSDXmlBytes(resp);
                }));
            return MainServer.Instance.ServerURI + capPath;
        }

        private OSD AddUserToGroup(OSDMap map)
        {
            bool finished = false;
            IGroupsServiceConnector groupsService = Aurora.Framework.Utilities.DataManager.RequestPlugin<IGroupsServiceConnector>();
            if (groupsService != null)
            {
                string first = map["first"];
                string last = map["last"];
                string group_name = map["group_name"];
                GroupRecord record = groupsService.GetGroupRecord(UUID.Zero, UUID.Zero, group_name);
                if (record != null)
                {
                    UserAccount user = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(null, first, last);
                    if (user != null)
                    {
                        groupsService.AddAgentToGroup(UUID.Zero, user.PrincipalID, record.GroupID, UUID.Zero);
                        finished = true;
                    }
                }
            }
            return finished;
        }

        private OSD CheckName(OSDMap map)
        {
            string userName = map["username"];
            int last_name_id = map["last_name_id"];

            bool found = false;
            if (m_lastNameRegistry.ContainsKey(last_name_id))
            {
                string lastName = m_lastNameRegistry[last_name_id];
                IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
                UserAccount user = accountService.GetUserAccount(null, userName, lastName);
                if (user != null)
                    found = true;
            }

            return found;
        }

        /// <summary>
        /// Creates a user
        /// TODO: Does not implement the restrict to estate option
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private OSD CreateUser(OSDMap map)
        {
            //Required params
            string username = map["username"];
            int last_name_id = map["last_name_id"];
            string email = map["email"];
            string password = map["password"];
            string dob = map["dob"];

            //Optional params
            int limited_to_estate = map["limited_to_estate"];
            string start_region_name = map["start_region_name"];
            float start_local_x = map["start_local_x"];
            float start_local_y = map["start_local_y"];
            float start_local_z = map["start_local_z"];
            float start_look_at_x = map["start_look_at_x"];
            float start_look_at_y = map["start_look_at_y"];
            float start_look_at_z = map["start_look_at_z"];
            OSD resp = null;

            if (username != "" && last_name_id != 0 && email != "" &&
                password != "" && dob != "")
            {
                IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
                if (m_lastNameRegistry.ContainsKey(last_name_id))
                {
                    string lastName = m_lastNameRegistry[last_name_id];
                    UserAccount user = accountService.GetUserAccount(null, username, lastName);
                    if (user == null)
                    {
                        //The pass is in plain text... so put it in and create the account
                        accountService.CreateUser(username + " " + lastName, password, email);
                        DateTime time = DateTime.ParseExact(dob, "YYYY-MM-DD", System.Globalization.CultureInfo.InvariantCulture);
                        user = accountService.GetUserAccount(null, username, lastName);
                        //Update the dob
                        user.Created = Util.ToUnixTime(time);
                        accountService.StoreUserAccount(user);

                        IAgentConnector agentConnector = Aurora.Framework.Utilities.DataManager.RequestPlugin<IAgentConnector>();
                        if (agentConnector != null)
                        {
                            agentConnector.CreateNewAgent(user.PrincipalID);
                            if (map.ContainsKey("limited_to_estate"))
                            {
                                IAgentInfo agentInfo = agentConnector.GetAgent(user.PrincipalID);
                                agentInfo.OtherAgentInformation["LimitedToEstate"] = limited_to_estate;
                                agentConnector.UpdateAgent(agentInfo);
                            }
                        }

                        MainConsole.Instance.Info("[RegApi]: Created new user " + user.Name);
                        try
                        {
                            if (start_region_name != "")
                            {
                                IAgentInfoService agentInfoService = m_registry.RequestModuleInterface<IAgentInfoService>();
                                if (agentInfoService != null)
                                {
                                    agentInfoService.SetHomePosition(user.PrincipalID.ToString(),
                                            m_registry.RequestModuleInterface<IGridService>().GetRegionByName
                                            (null, start_region_name).RegionID,
                                            new Vector3(start_local_x,
                                            start_local_y,
                                            start_local_z),
                                            new Vector3(start_look_at_x,
                                            start_look_at_y,
                                            start_look_at_z));
                                }
                            }
                        }
                        catch
                        {
                            MainConsole.Instance.Warn("[RegApi]: Encountered an error when setting the home position of a new user");
                        }
                        OSDMap r = new OSDMap();
                        r["agent_id"] = user.PrincipalID;
                        resp = r;
                    }
                    else //Already exists
                        resp = false;
                }
                else //Could not find last name
                    resp = false;
            }
            else //Not enough params
                resp = false;

            return resp;
        }

        private OSD GetErrorCode(OSDMap map)
        {
            string userName = map["username"];
            int last_name_id = map["last_name_id"];

            bool found = false;
            if (m_lastNameRegistry.ContainsKey(last_name_id))
            {
                string lastName = m_lastNameRegistry[last_name_id];
                IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
                UserAccount user = accountService.GetUserAccount(null, userName, lastName);
                if (user != null)
                    found = true;
            }

            return found;
        }

        private OSD GetLastNames(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            //Add all the last names
            foreach (KeyValuePair<int, string> kvp in m_lastNameRegistry)
            {
                resp[kvp.Key.ToString()] = kvp.Value;
            }

            return resp;
        }
    }
}
