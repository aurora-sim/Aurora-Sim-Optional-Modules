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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using log4net;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Framework.Capabilities;

namespace OpenSim.Region.DataSnapshot
{
    public class DataRequestHandler
    {
        private IScene m_scene = null;
        private DataSnapshotManager m_externalData = null;
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DataRequestHandler (IScene scene, DataSnapshotManager externalData)
        {
            m_scene = scene;
            m_externalData = externalData;

            //Register HTTP handler
            if (MainServer.Instance.AddHTTPHandler("collector", OnGetSnapshot))
            {
                //m_log.Info("[DATASNAPSHOT]: Set up snapshot service");
            }

            //Register CAPS handler event
            m_scene.EventManager.OnRegisterCaps += OnRegisterCaps;

            //harbl
        }

        public OSDMap OnRegisterCaps(UUID agentID, IHttpServer httpServer)
        {
            OSDMap retVal = new OSDMap();
            retVal["PublicSnapshotDataInfo"] = CapsUtil.CreateCAPS("PublicSnapshotDataInfo", "");

            httpServer.AddStreamHandler(new RestStreamHandler("POST", retVal["ViewerStartAuction"],
                                                      OnDiscoveryAttempt));
            //m_log.Info("[DATASNAPSHOT]: Registering service discovery capability for " + agentID);
            return retVal;
        }

        public string OnDiscoveryAttempt(string request, string path, string param,
                                         OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            //Very static for now, flexible enough to add new formats
            OSDMap resp = new OSDMap();
            resp["snapshot_resources"] = new OSDArray();

            OSDMap dataurl = new OSDMap();
            dataurl["snapshot_format"] = "os-datasnapshot-v1";
            dataurl["snapshot_url"] = "http://" + m_externalData.m_hostname + ":" + m_externalData.m_listener_port + "/?method=collector";

            ((OSDArray)resp["snapshot_resources"]).Add(dataurl);

            string response = OSDParser.SerializeLLSDXmlString(resp);

            return response;
        }

        public Hashtable OnGetSnapshot(Hashtable keysvals)
        {
            m_log.Info("[DATASNAPSHOT] Received collection request");
            Hashtable reply = new Hashtable();
            int statuscode = 200;

            string snapObj = (string)keysvals["region"];

            XmlDocument response = m_externalData.GetSnapshot(snapObj);

            reply["str_response_string"] = response.OuterXml;
            reply["int_response_code"] = statuscode;
            reply["content_type"] = "text/xml";

            return reply;
        }
    }
}
