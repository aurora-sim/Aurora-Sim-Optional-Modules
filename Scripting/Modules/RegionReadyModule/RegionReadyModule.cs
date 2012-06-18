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
using System.Collections.Generic;
using System.Reflection;

using log4net;
using Nini.Config;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.OptionalModules.Scripting.RegionReady
{
    public class RegionReadyModule : INonSharedRegionModule
    {
        private static readonly ILog m_log = 
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfig m_config = null;
        private bool m_firstEmptyCompileQueue;
        private bool m_oarFileLoading;
        private bool m_lastOarLoadedOk;
        private int m_channelNotify = -1000;
        private bool m_enabled = false;
        
        IScene m_scene = null;
        
        #region INonSharedRegionModule interface

        public Type ReplaceableInterface 
        { 
            get { return null; }
        }
            
        public void Initialise(IConfigSource config)
        {
            //m_log.Info("[RegionReady] Initialising");

            m_config = config.Configs["RegionReady"];
            if (m_config != null) 
            {
                m_enabled = m_config.GetBoolean("enabled", false);
                if (m_enabled) 
                {
                    m_channelNotify = m_config.GetInt("channel_notify", m_channelNotify);
                } 
            }

//            if (!m_enabled)
//                m_log.Info("[RegionReady] disabled.");
        }

        public void AddRegion(IScene scene)
        {
            if (!m_enabled)
                return;

            m_firstEmptyCompileQueue = true;
            m_oarFileLoading = false;
            m_lastOarLoadedOk = true;

            m_scene = scene;

            //m_scene.EventManager.OnEmptyScriptCompileQueue += OnEmptyScriptCompileQueue;
            m_scene.EventManager.OnStartupComplete += OnStartupComplete;
            m_scene.EventManager.OnOarFileLoaded += OnOarFileLoaded;

            m_log.DebugFormat("[RegionReady]: Enabled for region {0}", scene.RegionInfo.RegionName);
        }

        public void RemoveRegion(IScene scene)
        {
            if (!m_enabled)
                return;

            //m_scene.EventManager.OnEmptyScriptCompileQueue -= OnEmptyScriptCompileQueue;
            m_scene.EventManager.OnStartupComplete -= OnStartupComplete;
            m_scene.EventManager.OnOarFileLoaded -= OnOarFileLoaded;

            m_scene = null;
        }

        public void Close()
        {
        }

        public void RegionLoaded(IScene scene)
        {
        }

        public string Name
        {
            get { return "RegionReadyModule"; }
        }

        #endregion

        void OnStartupComplete(IScene scene, List<string> data)
        {
            int num = 0;
            while (num < data.Count - 1)
            {
                string type = data[num].ToString();
                int len = Convert.ToInt32(data[num + 1]);
                num += 2;
                if (type == "ScriptEngine")
                {
                    if (m_firstEmptyCompileQueue || m_oarFileLoading)
                    {
                        OSChatMessage c = new OSChatMessage();
                        if (m_firstEmptyCompileQueue)
                            c.Message = "server_startup,";
                        else
                            c.Message = "oar_file_load,";
                        m_firstEmptyCompileQueue = false;
                        m_oarFileLoading = false;

                        c.From = "RegionReady";
                        if (m_lastOarLoadedOk)
                            c.Message += "1,";
                        else
                            c.Message += "0,";
                        c.Channel = m_channelNotify;
                        //Equiv of 'c.Message += numScriptsFailed.ToString() + "," + message;'
                        c.Message += data[num] + "," + data[num+1];
                        c.Type = ChatTypeEnum.Region;
                        c.Position = new Vector3((m_scene.RegionInfo.RegionSizeX * 0.5f), (m_scene.RegionInfo.RegionSizeY * 0.5f), 30);
                        c.Sender = null;
                        c.SenderUUID = UUID.Zero;
                        c.Scene = m_scene;

                        m_log.InfoFormat("[RegionReady]: Region \"{0}\" is ready: \"{1}\" on channel {2}",
                                         m_scene.RegionInfo.RegionName, c.Message, m_channelNotify);
                        m_scene.EventManager.TriggerOnChatBroadcast(this, c);
                    }
                }
                else
                    num += len;
            }
        }
        
        //Old and hackish, see above
        /*void OnEmptyScriptCompileQueue(int numScriptsFailed, string message)
        {
            if (m_firstEmptyCompileQueue || m_oarFileLoading) 
            {
                OSChatMessage c = new OSChatMessage();
                if (m_firstEmptyCompileQueue) 
                    c.Message = "server_startup,";
                else 
                    c.Message = "oar_file_load,";
                m_firstEmptyCompileQueue = false;
                m_oarFileLoading = false;

                m_scene.Backup(false);

                c.From = "RegionReady";
                if (m_lastOarLoadedOk) 
                    c.Message += "1,";
                else
                    c.Message += "0,";
                c.Channel = m_channelNotify;
                c.Message += numScriptsFailed.ToString() + "," + message;
                c.Type = ChatTypeEnum.Region;
                c.Position = new Vector3(((int)Constants.RegionSize * 0.5f), ((int)Constants.RegionSize * 0.5f), 30);
                c.Sender = null;
                c.SenderUUID = UUID.Zero;
                c.Scene = m_scene;

                m_log.InfoFormat("[RegionReady]: Region \"{0}\" is ready: \"{1}\" on channel {2}",
                                 m_scene.RegionInfo.RegionName, c.Message, m_channelNotify);
                m_scene.EventManager.TriggerOnChatBroadcast(this, c); 
            }
        }*/

        void OnOarFileLoaded(Guid requestId, string message)
        {
            m_oarFileLoading = true;
            if (message==String.Empty) 
            {
                m_lastOarLoadedOk = true;
            } else {
                m_log.InfoFormat("[RegionReady]: Oar file load errors: {0}", message);
                m_lastOarLoadedOk = false;
            }
        }
    }
}
