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

using System;
using System.Collections;
using System.Web;
using System.Reflection;
using Nini.Config;
using Aurora.Simulation.Base;
using OpenSim.Services.Interfaces;
using OpenSim.Framework.Servers.HttpServer;
using log4net;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;

namespace FreeswitchVoice
{
    public class FreeswitchServerConnector : IService, IGridRegistrationUrlModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IFreeswitchService m_FreeswitchService;
        protected readonly string m_freeSwitchAPIPrefix = "/fsapi";

        private IRegistryCore m_registry;

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
            if (handlerConfig.GetString("FreeswitchInHandler", "") != Name)
                return;

            m_registry = registry;

            m_registry.RequestModuleInterface<IGridRegistrationService>().RegisterModule(this);
        }

        public void FinishedStartup()
        {
        }

        public Hashtable FreeSwitchConfigHTTPHandler(Hashtable request)
        {
            Hashtable response = new Hashtable();
            response["str_response_string"] = string.Empty;
            response["content_type"] = "text/plain";
            response["keepalive"] = false;
            response["int_response_code"] = 500;

            Hashtable requestBody = ParseRequestBody((string)request["body"]);

            string section = (string)requestBody["section"];

            if (section == "directory")
                response = m_FreeswitchService.HandleDirectoryRequest(requestBody);
            else if (section == "dialplan")
                response = m_FreeswitchService.HandleDialplanRequest(requestBody);
            else
                m_log.WarnFormat("[FreeSwitchVoice]: section was {0}", section);

            return response;
        }

        private Hashtable ParseRequestBody(string body)
        {
            Hashtable bodyParams = new Hashtable();
            // split string
            string[] nvps = body.Split(new Char[] { '&' });

            foreach (string s in nvps)
            {
                if (s.Trim() != "")
                {
                    string[] nvp = s.Split(new Char[] { '=' });
                    bodyParams.Add(HttpUtility.UrlDecode(nvp[0]), HttpUtility.UrlDecode(nvp[1]));
                }
            }

            return bodyParams;
        }

        public Hashtable RegionConfigHTTPHandler(Hashtable request)
        {
            Hashtable response = new Hashtable();
            response["content_type"] = "text/json";
            response["keepalive"] = false;
            response["int_response_code"] = 200;

            response["str_response_string"] = m_FreeswitchService.GetJsonConfig();

            return response;
        }

        #region IGridRegistrationUrlModule Members

        public string UrlName
        {
            get { return "FreeswitchServiceURL"; }
        }

        public void AddExistingUrlForClient (string SessionID, string url, uint port)
        {
            IHttpServer server = m_registry.RequestModuleInterface<ISimulationBase> ().GetHttpServer (port);

            m_FreeswitchService = m_registry.RequestModuleInterface<IFreeswitchService>();

            server.AddHTTPHandler(String.Format("{0}/freeswitch-config", url), FreeSwitchConfigHTTPHandler);
            server.AddHTTPHandler(String.Format("{0}/region-config", url), RegionConfigHTTPHandler);
        }

        public string GetUrlForRegisteringClient(string SessionID, uint port)
        {
            string url = String.Format("{0}/{1}", m_freeSwitchAPIPrefix, UUID.Random());

            IHttpServer server = m_registry.RequestModuleInterface<ISimulationBase> ().GetHttpServer (port);

            m_FreeswitchService = m_registry.RequestModuleInterface<IFreeswitchService>();

            server.AddHTTPHandler(url + "/freeswitch-config", FreeSwitchConfigHTTPHandler);
            server.AddHTTPHandler(url + "/region-config", RegionConfigHTTPHandler);
            return url;
        }

        public void RemoveUrlForClient (string sessionID, string url, uint port)
        {
            IHttpServer server = m_registry.RequestModuleInterface<ISimulationBase> ().GetHttpServer (port);
            server.RemoveHTTPHandler("POST", url + "/freeswitch-config");
            server.RemoveHTTPHandler("POST", url + "/region-config");
        }

        #endregion
    }
}
