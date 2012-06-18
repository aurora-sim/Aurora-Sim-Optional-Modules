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
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.DataSnapshot.Interfaces;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.DataSnapshot
{
    public class DataSnapshotManager : ISharedRegionModule, IDataSnapshot
    {
        #region Class members
        //Information from config
        private bool m_enabled = false;
        private bool m_configLoaded = false;
        private List<string> m_disabledModules = new List<string>();
        private Dictionary<string, string> m_gridinfo = new Dictionary<string, string>();
        private string m_snapsDir = "DataSnapshot";
        private string m_exposure_level = "minimum";

        //Lists of stuff we need
        private List<IScene> m_scenes = new List<IScene>();
        private List<IDataSnapshotProvider> m_dataproviders = new List<IDataSnapshotProvider>();

        //Various internal objects
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal object m_syncInit = new object();

        //DataServices and networking
        private string m_dataServices = "noservices";
        public string m_listener_port = "";
        public string m_hostname = "127.0.0.1";

        //Update timers
        private int m_period = 20; // in seconds
        private int m_maxStales = 500;
        private int m_stales = 0;
        private int m_lastUpdate = 0;

        //Program objects
        private SnapshotStore m_snapStore = null;

        #endregion

        #region Properties

        public string ExposureLevel
        {
            get { return m_exposure_level; }
        }

        #endregion

        #region IRegionModule

        public void Initialise(IConfigSource config)
        {
            if (!m_configLoaded) 
            {
                m_configLoaded = true;
                //m_log.Debug("[DATASNAPSHOT]: Loading configuration");
                //Read from the config for options
                lock (m_syncInit)
                {
                    if (config.Configs["DataSnapshot"] == null)
                        return;
                    try
                    {
                        m_enabled = config.Configs["DataSnapshot"].GetBoolean("index_sims", m_enabled);
                        IConfig conf = config.Configs["GridService"];
                        if (conf != null)
                            m_gridinfo.Add("gridserverURL", conf.GetString("GridServerURI", "http://127.0.0.1:8003"));

                        m_gridinfo.Add(
                            "Name", config.Configs["DataSnapshot"].GetString("gridname", "the lost continent of hippo"));
                        m_exposure_level = config.Configs["DataSnapshot"].GetString("data_exposure", m_exposure_level);
                        m_period = config.Configs["DataSnapshot"].GetInt("default_snapshot_period", m_period);
                        m_maxStales = config.Configs["DataSnapshot"].GetInt("max_changes_before_update", m_maxStales);
                        m_snapsDir = config.Configs["DataSnapshot"].GetString("snapshot_cache_directory", m_snapsDir);
                        m_dataServices = config.Configs["DataSnapshot"].GetString("data_services", m_dataServices);
                        m_listener_port = config.Configs["Network"].GetString("http_listener_port", m_listener_port);

                        String[] annoying_string_array = config.Configs["DataSnapshot"].GetString("disable_modules", "").Split(".".ToCharArray());
                        foreach (String bloody_wanker in annoying_string_array)
                        {
                            m_disabledModules.Add(bloody_wanker);
                        }
                        m_lastUpdate = Environment.TickCount;
                    }
                    catch (Exception)
                    {
                        m_enabled = false;
                        return;
                    }
                }
            }
        }

        public void AddRegion (IScene scene)
        {
            if (m_enabled)
            {
                //Hand it the first scene, assuming that all scenes have the same BaseHTTPServer
                new DataRequestHandler(scene, this);

                m_hostname = scene.RegionInfo.ExternalHostName;
                m_snapStore = new SnapshotStore(m_snapsDir, m_gridinfo, m_listener_port, m_hostname);

                MakeEverythingStale();

                if (m_dataServices != "" && m_dataServices != "noservices")
                    NotifyDataServices(m_dataServices, "online");
            }

            if (m_enabled)
            {
                //m_log.Info("[DATASNAPSHOT]: Scene added to module.");

                m_snapStore.AddScene(scene);
                m_scenes.Add(scene);

                Assembly currentasm = Assembly.GetExecutingAssembly();

                foreach (Type pluginType in currentasm.GetTypes())
                {
                    if (pluginType.IsPublic)
                    {
                        if (!pluginType.IsAbstract)
                        {
                            if (pluginType.GetInterface("IDataSnapshotProvider") != null)
                            {
                                IDataSnapshotProvider module = (IDataSnapshotProvider)Activator.CreateInstance(pluginType);
                                module.Initialize(scene, this);
                                module.OnStale += MarkDataStale;

                                m_dataproviders.Add(module);
                                m_snapStore.AddProvider(module);

                                //m_log.Info("[DATASNAPSHOT]: Added new data provider type: " + pluginType.Name);
                            }
                        }
                    }
                }

                //scene.OnRestart += OnSimRestart;
                //scene.EventManager.OnShutdown += delegate() { OnSimRestart(scene.RegionInfo); };
                scene.RegisterModuleInterface<IDataSnapshot>(this);
            }
            else
            {
                //m_log.Debug("[DATASNAPSHOT]: Data snapshot disabled, not adding scene to module (or anything else).");
            }
        }

        public void RemoveRegion(IScene scene)
        {
            if (m_enabled && m_dataServices != "" && m_dataServices != "noservices")
                NotifyDataServices(m_dataServices, "offline");
        }

        public void RegionLoaded(IScene scene)
        {

        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void Close() 
        {
        }


        public bool IsSharedModule
        {
            get { return true; }
        }

        public string Name
        {
            get { return "External Data Generator"; }
        }

        public void PostInitialise()
        {

        }

        #endregion

        #region Associated helper functions

        public IScene SceneForName(string name)
        {
            foreach (IScene scene in m_scenes)
                if (scene.RegionInfo.RegionName == name)
                    return scene;

            return null;
        }

        public IScene SceneForUUID(UUID id)
        {
            foreach (IScene scene in m_scenes)
                if (scene.RegionInfo.RegionID == id)
                    return scene;

            return null;
        }

        #endregion

        #region [Public] Snapshot storage functions

        /**
         * Reply to the http request
         */
        public XmlDocument GetSnapshot(string regionName)
        {
            CheckStale();

            XmlDocument requestedSnap = new XmlDocument();
            requestedSnap.AppendChild(requestedSnap.CreateXmlDeclaration("1.0", null, null));
            requestedSnap.AppendChild(requestedSnap.CreateWhitespace("\r\n"));

            XmlNode regiondata = requestedSnap.CreateNode(XmlNodeType.Element, "regiondata", "");
            try
            {
                if (regionName == null || regionName == "")
                {
                    XmlNode timerblock = requestedSnap.CreateNode(XmlNodeType.Element, "expire", "");
                    timerblock.InnerText = m_period.ToString();
                    regiondata.AppendChild(timerblock);

                    regiondata.AppendChild(requestedSnap.CreateWhitespace("\r\n"));
                    foreach (IScene scene in m_scenes)
                    {
                        regiondata.AppendChild(m_snapStore.GetScene(scene, requestedSnap));
                    }
                }
                else
                {
                    IScene scene = SceneForName(regionName);
                    regiondata.AppendChild(m_snapStore.GetScene(scene, requestedSnap));
                }
                requestedSnap.AppendChild(regiondata);
                regiondata.AppendChild(requestedSnap.CreateWhitespace("\r\n"));
            }
            catch (XmlException e)
            {
                m_log.Warn("[DATASNAPSHOT]: XmlException while trying to load snapshot: " + e.ToString());
                requestedSnap = GetErrorMessage(regionName, e);
            }
            catch (Exception e)
            {
                m_log.Warn("[DATASNAPSHOT]: Caught unknown exception while trying to load snapshot: " + e.StackTrace);
                requestedSnap = GetErrorMessage(regionName, e);
            }


            return requestedSnap;
        }

        private XmlDocument GetErrorMessage(string regionName, Exception e)
        {
            XmlDocument errorMessage = new XmlDocument();
            XmlNode error = errorMessage.CreateNode(XmlNodeType.Element, "error", "");
            XmlNode region = errorMessage.CreateNode(XmlNodeType.Element, "region", "");
            region.InnerText = regionName;

            XmlNode exception = errorMessage.CreateNode(XmlNodeType.Element, "exception", "");
            exception.InnerText = e.ToString();

            error.AppendChild(region);
            error.AppendChild(exception);
            errorMessage.AppendChild(error);

            return errorMessage;
        }

        #endregion

        #region External data services
        private void NotifyDataServices(string servicesStr, string serviceName)
        {
            Stream reply = null;
            string delimStr = ";";
            char [] delimiter = delimStr.ToCharArray();

            string[] services = servicesStr.Split(delimiter);

            for (int i = 0; i < services.Length; i++)
            {
                string url = services[i].Trim();
                RestClient cli = new RestClient(url);
                cli.AddQueryParameter("service", serviceName);
                cli.AddQueryParameter("host", m_hostname);
                cli.AddQueryParameter("port", m_listener_port);
                cli.RequestMethod = "GET";
                try
                {
                    reply = cli.Request();
                }
                catch (WebException)
                {
                    m_log.Warn("[DATASNAPSHOT]: Unable to notify " + url);
                }
                catch (Exception e)
                {
                    m_log.Warn("[DATASNAPSHOT]: Ignoring unknown exception " + e.ToString());
                }
                byte[] response = new byte[1024];
                // int n = 0;
                try
                {
                    // n = reply.Read(response, 0, 1024);
                    reply.Read(response, 0, 1024);
                }
                catch (Exception e)
                {
                    m_log.WarnFormat("[DATASNAPSHOT]: Unable to decode reply from data service. Ignoring. {0}", e.StackTrace);
                }
                // This is not quite working, so...
                // string responseStr = Util.UTF8.GetString(response);
                //m_log.Info("[DATASNAPSHOT]: data service notified: " + url);
            }

        }
        #endregion

        #region Latency-based update functions

        public void MarkDataStale(IDataSnapshotProvider provider)
        {
            //Behavior here: Wait m_period seconds, then update if there has not been a request in m_period seconds
            //or m_maxStales has been exceeded
            m_stales++;
        }

        private void CheckStale()
        {
            // Wrap check
            if (Environment.TickCount < m_lastUpdate)
            {
                m_lastUpdate = Environment.TickCount;
            }

            if (m_stales >= m_maxStales)
            {
                if (Environment.TickCount - m_lastUpdate >= 20000)
                {
                    m_stales = 0;
                    m_lastUpdate = Environment.TickCount;
                    MakeEverythingStale();
                }
            }
            else
            {
                if (m_lastUpdate + 1000 * m_period < Environment.TickCount)
                {
                    m_stales = 0;
                    m_lastUpdate = Environment.TickCount;
                    MakeEverythingStale();
                }
            }
        }

        public void MakeEverythingStale()
        {
            //m_log.Debug("[DATASNAPSHOT]: Marking all scenes as stale.");
            foreach (IScene scene in m_scenes)
            {
                m_snapStore.ForceSceneStale(scene);
            }
        }

        #endregion

        public void OnSimRestart(RegionInfo thisRegion)
        {
            //m_log.Info("[DATASNAPSHOT]: Region " + thisRegion.RegionName + " is restarting, removing from indexing");
            IScene restartedScene = SceneForUUID(thisRegion.RegionID);

            m_scenes.Remove(restartedScene);
            m_snapStore.RemoveScene(restartedScene);

            //Getting around the fact that we can't remove objects from a collection we are enumerating over
            List<IDataSnapshotProvider> providersToRemove = new List<IDataSnapshotProvider>();

            foreach (IDataSnapshotProvider provider in m_dataproviders)
            {
                if (provider.GetParentScene == restartedScene)
                {
                    providersToRemove.Add(provider);
                }
            }

            foreach (IDataSnapshotProvider provider in providersToRemove)
            {
                m_dataproviders.Remove(provider);
                m_snapStore.RemoveProvider(provider);
            }

            m_snapStore.RemoveScene(restartedScene);
        }
    }
}
