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
using System.Text;
using System.Reflection;
using Nini.Config;
using Aurora.Framework;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System.Collections;
using Aurora.Simulation.Base;
using Aurora.Framework.Services;
using Aurora.Framework.Modules;
using Aurora.Framework.ConsoleFramework;
using Aurora.Framework.Servers.HttpServer.Implementation;
using Aurora.Framework.Servers;

namespace FreeswitchVoice
{
    public class FreeswitchService : IFreeswitchService, IService
    {
        protected string m_freeSwitchRealm;
        protected string m_freeSwitchSIPProxy;
        protected bool m_freeSwitchAttemptUseSTUN = false;
        protected string m_freeSwitchEchoServer;
        protected int m_freeSwitchEchoPort = 50505;
        protected string m_freeSwitchDefaultWellKnownIP;
        protected int m_freeSwitchDefaultTimeout = 5000;
        protected string m_freeSwitchContext = "default";
        protected string m_freeSwitchServerUser = "freeswitch";
        protected string m_freeSwitchServerPass = "password";
        protected readonly string m_freeSwitchAPIPrefix = "/fsapi";

        protected bool m_Enabled = false;

        public string Name
        {
            get { return GetType().Name; }
        }

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            IConfig voiceconfig = config.Configs["Voice"];
            if (voiceconfig == null)
                return;
            string voiceModule = "FreeSwitchVoice";
            if (voiceconfig.GetString("Module", voiceModule) != voiceModule)
                return;

            IConfig freeswitchConfig = config.Configs["FreeswitchService"];
            if (freeswitchConfig != null)
            {
                m_freeSwitchDefaultWellKnownIP = freeswitchConfig.GetString("ServerAddress", String.Empty);
                if (m_freeSwitchDefaultWellKnownIP == String.Empty)
                {
                    MainConsole.Instance.Debug("[FREESWITCH]: No FreeswitchServerAddress given, can't continue");
                    return;
                }

                m_freeSwitchRealm = freeswitchConfig.GetString("Realm", m_freeSwitchDefaultWellKnownIP);
                m_freeSwitchSIPProxy = freeswitchConfig.GetString("SIPProxy", m_freeSwitchDefaultWellKnownIP + ":5060");
                m_freeSwitchEchoServer = freeswitchConfig.GetString("EchoServer", m_freeSwitchDefaultWellKnownIP);
                m_freeSwitchEchoPort = freeswitchConfig.GetInt("EchoPort", m_freeSwitchEchoPort);
                m_freeSwitchAttemptUseSTUN = freeswitchConfig.GetBoolean("AttemptSTUN", false); // This may not work
                m_freeSwitchDefaultTimeout = freeswitchConfig.GetInt("DefaultTimeout", m_freeSwitchDefaultTimeout);
                m_freeSwitchContext = freeswitchConfig.GetString("Context", m_freeSwitchContext);
                m_freeSwitchServerUser = freeswitchConfig.GetString("UserName", m_freeSwitchServerUser);
                m_freeSwitchServerPass = freeswitchConfig.GetString("Password", m_freeSwitchServerPass);

                m_Enabled = true;
                registry.RegisterModuleInterface<IFreeswitchService>(this);
            }
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
        }

        public void FinishedStartup()
        {
        }

        public byte[] HandleDialplanRequest(Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MainConsole.Instance.DebugFormat("[FreeSwitchVoice] HandleDialplanRequest called with {0}", request.ToString());

            string requestcontext = (string)request["Hunt-Context"];
            httpResponse.ContentType = "text/xml";
            httpResponse.StatusCode = 200;

            if (m_freeSwitchContext != String.Empty && m_freeSwitchContext != requestcontext)
            {
                MainConsole.Instance.Debug("[FreeSwitchDirectory] returning empty as it's for another context");
                return MainServer.BadRequest;
            }
            else
            {
                return Encoding.UTF8.GetBytes(String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
                   <document type=""freeswitch/xml"">
                     <section name=""dialplan"">
                     <context name=""{0}"">" +

/*                           <!-- dial via SIP uri -->
                            <extension name=""sip_uri"">
                                   <condition field=""destination_number"" expression=""^sip:(.*)$"">
                                   <action application=""bridge"" data=""sofia/${use_profile}/$1""/>
                                   <!--<action application=""bridge"" data=""$1""/>-->
                                   </condition>
                           </extension>*/

                           @"<extension name=""opensim_conferences"">
                                   <condition field=""destination_number"" expression=""^confctl-(.*)$"">
                                           <action application=""answer""/>
                                           <action application=""conference"" data=""$1-{1}@{0}""/>
                                   </condition>
                           </extension>

                           <extension name=""opensim_conf"">
                                   <condition field=""destination_number"" expression=""^conf-(.*)$"">
                                           <action application=""answer""/>
                                           <action application=""conference"" data=""$1-{1}@{0}""/>
                                   </condition>
                           </extension>

                           <extension name=""avatar"">
                                   <condition field=""destination_number"" expression=""^(x.*)$"">
                                           <action application=""bridge"" data=""user/$1""/>
                                   </condition>
                           </extension>

                     </context>
                   </section>
                   </document>", m_freeSwitchContext, m_freeSwitchRealm));
            }
        }

        public byte[] HandleDirectoryRequest(Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            string domain = (string)request["domain"];
            if (domain != m_freeSwitchRealm)
            {
                httpResponse.ContentType = "text/xml";
                httpResponse.StatusCode = 200;
                return MainServer.BadRequest;
            }
            else
            {
                MainConsole.Instance.DebugFormat("[FreeSwitchDirectory] HandleDirectoryRequest called with {0}", request.ToString());

                // information in the request we might be interested in

                // Request 1 sip_auth for users account

                //Event-Calling-Function=sofia_reg_parse_auth
                //Event-Calling-Line-Number=1494
                //action=sip_auth
                //sip_user_agent=Vivox-SDK-2.1.3010.6151-Mac%20(Feb-11-2009/16%3A42%3A41)
                //sip_auth_username=xhZuXKmRpECyr2AARJYyGgg%3D%3D  (==)
                //sip_auth_realm=9.20.151.43
                //sip_contact_user=xhZuXKmRpECyr2AARJYyGgg%3D%3D (==)
                //sip_contact_host=192.168.0.3    // this shouldnt really be a local IP, investigate STUN servers
                //sip_to_user=xhZuXKmRpECyr2AARJYyGgg%3D%3D
                //sip_to_host=9.20.151.43
                //sip_auth_method=REGISTER
                //user=xhZuXKmRpECyr2AARJYyGgg%3D%3D
                //domain=9.20.151.43
                //ip=9.167.220.137    // this is the correct IP rather than sip_contact_host above when through a vpn or NAT setup

                foreach (DictionaryEntry item in request)
                {
                    MainConsole.Instance.InfoFormat("[FreeSwitchDirectory] requestBody item {0} {1}", item.Key, item.Value);
                }

                string eventCallingFunction = (string)request["Event-Calling-Function"];
                if (eventCallingFunction == null)
                {
                    eventCallingFunction = "sofia_reg_parse_auth";
                }

                if (eventCallingFunction.Length == 0)
                {
                    eventCallingFunction = "sofia_reg_parse_auth";
                }

                if (eventCallingFunction == "sofia_reg_parse_auth")
                {
                    string sipAuthMethod = (string)request["sip_auth_method"];

                    if (sipAuthMethod == "REGISTER")
                    {
                        return HandleRegister(m_freeSwitchContext, m_freeSwitchRealm, request, httpRequest, httpResponse);
                    }
                    else if (sipAuthMethod == "INVITE")
                    {
                        return HandleInvite(m_freeSwitchContext, m_freeSwitchRealm, request, httpRequest, httpResponse);
                    }
                    else
                    {
                        MainConsole.Instance.ErrorFormat("[FreeSwitchVoice] HandleDirectoryRequest unknown sip_auth_method {0}", sipAuthMethod);
                        httpResponse.ContentType = "text/xml";
                        httpResponse.StatusCode = 404;
                        return MainServer.BadRequest;
                    }
                }
                else if (eventCallingFunction == "switch_xml_locate_user")
                {
                    return HandleLocateUser(m_freeSwitchRealm, request, httpRequest, httpResponse);
                }
                else if (eventCallingFunction == "user_data_function") // gets called when an avatar to avatar call is made
                {
                    return HandleLocateUser(m_freeSwitchRealm, request, httpRequest, httpResponse);
                }
                else if (eventCallingFunction == "user_outgoing_channel")
                {
                    return HandleRegister(m_freeSwitchContext, m_freeSwitchRealm, request, httpRequest, httpResponse);
                }
                else if (eventCallingFunction == "config_sofia") // happens once on freeswitch startup
                {
                    return HandleConfigSofia(m_freeSwitchContext, m_freeSwitchRealm, request, httpRequest, httpResponse);
                }
                else if (eventCallingFunction == "switch_load_network_lists")
                {
                    //response = HandleLoadNetworkLists(request);
                    httpResponse.ContentType = "text/xml";
                    httpResponse.StatusCode = 404;
                    return MainServer.BadRequest;
                }
                else
                {
                    MainConsole.Instance.ErrorFormat("[FreeSwitchVoice] HandleDirectoryRequest unknown Event-Calling-Function {0}", eventCallingFunction);
                    httpResponse.ContentType = "text/xml";
                    httpResponse.StatusCode = 404;
                    return MainServer.BadRequest;
                }
            }
        }

        private byte[] HandleRegister(string Context, string Realm, Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MainConsole.Instance.Info("[FreeSwitchDirectory] HandleRegister called");

            // TODO the password we return needs to match that sent in the request, this is hard coded for now
            string password = "1234";
            string domain = (string)request["domain"];
            string user = (string)request["user"];
            httpResponse.ContentType = "text/xml";
            httpResponse.StatusCode = 200;

            return Encoding.UTF8.GetBytes(String.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<document type=\"freeswitch/xml\">\r\n" +
                    "<section name=\"directory\" description=\"User Directory\">\r\n" +
                        "<domain name=\"{0}\">\r\n" +
                            "<user id=\"{1}\">\r\n" +
                                "<params>\r\n" +
                                    "<param name=\"password\" value=\"{2}\" />\r\n" +
                                    "<param name=\"dial-string\" value=\"{{sip_contact_user={1}}}{{presence_id=${{dialed_user}}@${{dialed_domain}}}}${{sofia_contact(${{dialed_user}}@${{dialed_domain}})}}\"/>\r\n" +
                                "</params>\r\n" +
                                "<variables>\r\n" +
                                    "<variable name=\"user_context\" value=\"{3}\" />\r\n" +
                                    "<variable name=\"presence_id\" value=\"{1}@{0}\"/>" +
                                "</variables>\r\n" +
                            "</user>\r\n" +
                        "</domain>\r\n" +
                    "</section>\r\n" +
                "</document>\r\n",
                domain, user, password, Context));
        }

        private byte[] HandleInvite(string Context, string Realm, Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MainConsole.Instance.Info("[FreeSwitchDirectory] HandleInvite called");

            // TODO the password we return needs to match that sent in the request, this is hard coded for now
            string password = "1234";
            string domain = (string)request["domain"];
            string user = (string)request["user"];
            string sipRequestUser = (string)request["sip_request_user"];

            httpResponse.ContentType = "text/xml";
            httpResponse.StatusCode = 200; 

            return Encoding.UTF8.GetBytes(String.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<document type=\"freeswitch/xml\">\r\n" +
                    "<section name=\"directory\" description=\"User Directory\">\r\n" +
                        "<domain name=\"{0}\">\r\n" +
                            "<user id=\"{1}\">\r\n" +
                                "<params>\r\n" +
                                    "<param name=\"password\" value=\"{2}\" />\r\n" +
                                    "<param name=\"dial-string\" value=\"{{sip_contact_user={1}}}{{presence_id=${1}@${{dialed_domain}}}}${{sofia_contact(${1}@${{dialed_domain}})}}\"/>\r\n" +
                                "</params>\r\n" +
                                "<variables>\r\n" +
                                    "<variable name=\"user_context\" value=\"{4}\" />\r\n" +
                                    "<variable name=\"presence_id\" value=\"{1}@$${{domain}}\"/>" +
                                "</variables>\r\n" +
                            "</user>\r\n" +
                            "<user id=\"{3}\">\r\n" +
                                "<params>\r\n" +
                                    "<param name=\"password\" value=\"{2}\" />\r\n" +
                                    "<param name=\"dial-string\" value=\"{{sip_contact_user={1}}}{{presence_id=${3}@${{dialed_domain}}}}${{sofia_contact(${3}@${{dialed_domain}})}}\"/>\r\n" +
                                "</params>\r\n" +
                                "<variables>\r\n" +
                                    "<variable name=\"user_context\" value=\"{4}\" />\r\n" +
                                    "<variable name=\"presence_id\" value=\"{3}@$${{domain}}\"/>" +
                                "</variables>\r\n" +
                            "</user>\r\n" +
                        "</domain>\r\n" +
                    "</section>\r\n" +
                "</document>\r\n",
                domain, user, password, sipRequestUser, Context));
        }

        private byte[] HandleLocateUser(String Realm, Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MainConsole.Instance.Info("[FreeSwitchDirectory] HandleLocateUser called");

            // TODO the password we return needs to match that sent in the request, this is hard coded for now
            string domain = (string)request["domain"];
            string user = (string)request["user"];

            httpResponse.ContentType = "text/xml";
            httpResponse.StatusCode = 200; 
            return Encoding.UTF8.GetBytes(String.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<document type=\"freeswitch/xml\">\r\n" +
                    "<section name=\"directory\" description=\"User Directory\">\r\n" +
                        "<domain name=\"{0}\">\r\n" +
                            "<params>\r\n" +
                                "<param name=\"dial-string\" value=\"{{sip_contact_user=${{dialed_user}}}}{{presence_id=${{dialed_user}}@${{dialed_domain}}}}${{sofia_contact(${{dialed_user}}@${{dialed_domain}})}}\"/>\r\n" +
                            "</params>\r\n" +
                            "<user id=\"{1}\">\r\n" +
                            "<variables>\r\n" +
                              "<variable name=\"default_gateway\" value=\"$${{default_provider}}\"/>\r\n" +
                              "<variable name=\"presence_id\" value=\"{1}@$${{domain}}\"/>" +
                            "</variables>\r\n" +
                            "</user>\r\n" +
                        "</domain>\r\n" +
                    "</section>\r\n" +
                "</document>\r\n",
                domain, user));
        }

        private byte[] HandleConfigSofia(string Context, string Realm, Hashtable request, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MainConsole.Instance.Info("[FreeSwitchDirectory] HandleConfigSofia called");

            // TODO the password we return needs to match that sent in the request, this is hard coded for now
            string domain = (string)request["domain"];


            httpResponse.ContentType = "text/xml";
            httpResponse.StatusCode = 200; 
            return Encoding.UTF8.GetBytes(String.Format(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<document type=\"freeswitch/xml\">\r\n" +
                    "<section name=\"directory\" description=\"User Directory\">\r\n" +
                        "<domain name=\"{0}\">\r\n" +
                            "<params>\r\n" +
                                "<param name=\"dial-string\" value=\"{{sip_contact_user=${{dialed_user}}}}{{presence_id=${{dialed_user}}@${{dialed_domain}}}}${{sofia_contact(${{dialed_user}}@${{dialed_domain}})}}\"/>\r\n" +
                            "</params>\r\n" +
                            "<groups name=\"default\">\r\n" +
                                "<users>\r\n" +
                                    "<user id=\"$${{default_provider}}\">\r\n" +
                                        "<gateways>\r\n" +
                                          "<gateway name=\"$${{default_provider}}\">\r\n" +
                                            "<param name=\"username\" value=\"$${{default_provider_username}}\"/>\r\n" +
                                            "<param name=\"password\" value=\"$${{default_provider_password}}\"/>\r\n" +
                                            "<param name=\"from-user\" value=\"$${{default_provider_username}}\"/>\r\n" +
                                            "<param name=\"from-domain\" value=\"$${{default_provider_from_domain}}\"/>\r\n" +
                                            "<param name=\"expire-seconds\" value=\"600\"/>\r\n" +
                                            "<param name=\"register\" value=\"$${{default_provider_register}}\"/>\r\n" +
                                            "<param name=\"retry-seconds\" value=\"30\"/>\r\n" +
                                            "<param name=\"extension\" value=\"$${{default_provider_contact}}\"/>\r\n" +
                                            "<param name=\"contact-params\" value=\"domain_name=$${{domain}}\"/>\r\n" +
                                            "<param name=\"context\" value=\"{1}\"/>\r\n" +
                                          "</gateway>\r\n" +
                                        "</gateways>\r\n" +
                                        "<params>\r\n" +
                                          "<param name=\"password\" value=\"$${{default_provider_password}}\"/>\r\n" +
                                        "</params>\r\n" +
                                      "</user>\r\n" +
                                "</users>" +
                            "</groups>\r\n" +
                            "<variables>\r\n" +
                              "<variable name=\"default_gateway\" value=\"$${{default_provider}}\"/>\r\n" +
                            "</variables>\r\n" +
                        "</domain>\r\n" +
                    "</section>\r\n" +
                "</document>\r\n",
                domain, Context));
        }

        public string GetJsonConfig()
        {
            OSDMap map = new OSDMap(9);

            map.Add("Realm", m_freeSwitchRealm);
            map.Add("SIPProxy", m_freeSwitchSIPProxy);
            map.Add("AttemptUseSTUN", m_freeSwitchAttemptUseSTUN);
            map.Add("EchoServer", m_freeSwitchEchoServer);
            map.Add("EchoPort", m_freeSwitchEchoPort);
            map.Add("DefaultWellKnownIP", m_freeSwitchDefaultWellKnownIP);
            map.Add("DefaultTimeout", m_freeSwitchDefaultTimeout);
            map.Add("Context", m_freeSwitchContext);
            map.Add("APIPrefix", m_freeSwitchAPIPrefix);

            return OSDParser.SerializeJsonString(map);
        }
    }
}
