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
using System.IO.Compression;
using System.Text;
using System.Reflection;
using Aurora.Framework;
using Aurora.Framework.Serialization;
using Aurora.DataManager;
using Aurora.Simulation.Base;
using OpenSim.Services.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;
using Nini.Config;
using Microsoft.Win32;
using System.Windows.Forms;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Region.Framework.Scenes.Serialization;

namespace Aurora.StandaloneLoader
{
    public class Loader : IApplicationPlugin
    {
        #region Declares

        private ISimulationBase m_simulationBase;
        private bool m_enabled = false;
        private bool m_inuse = false;
        private string m_fileName = "";
        private bool m_saveNewArchiveAtClose = false;
        private bool m_useExistingRegionInfo = true;
            
        #endregion

        #region IApplicationPlugin Members

        public void PreStartup(ISimulationBase openSim)
        {
        }

        public void Initialize(ISimulationBase openSim)
        {
            m_simulationBase = openSim;
            IConfig config = openSim.ConfigSource.Configs["StandaloneLoader"];
            if (config == null)
                return;
            m_enabled = config.GetBoolean("Enabled", false);
            if (!m_enabled)
                return;

            m_saveNewArchiveAtClose = config.GetBoolean("SaveNewArchiveAtClose", false);
            m_useExistingRegionInfo = config.GetBoolean("UseExistingRegionInfo", false);

            foreach (string f in m_simulationBase.CommandLineParameters)
            {
                if (f.EndsWith(".abackup"))
                {
                    m_inuse = true;
                    m_fileName = f;
                    break;
                }
            }
        }

        public void PostInitialise()
        {
            if (m_enabled)
            {
                try
                {
                    //Register the extention
                    string ext = ".abackup";
                    RegistryKey key = Registry.ClassesRoot.CreateSubKey(ext);
                    key.SetValue("", "abackup file");
                    key.Close();

                    key = Registry.ClassesRoot.CreateSubKey(ext + "\\Shell\\Open\\command");

                    key.SetValue("", "\"" + Application.ExecutablePath + "\" \"%L\"");
                    key.Close();
                }
                catch
                {
                }
            }
        }

        public void Start()
        {
            if (m_inuse)
            {
                OpenSim.CoreApplicationPlugins.LoadRegionsPlugin plugin = m_simulationBase.ApplicationRegistry.RequestModuleInterface<OpenSim.CoreApplicationPlugins.LoadRegionsPlugin>();
                if (plugin != null)
                {
                    //Disable it so that we can load!
                    plugin.Enabled = false;
                }
            }
        }

        public void PostStart()
        {
            if (m_inuse)
            {
                LoadFromFile();
            }
        }

        public void Close()
        {
        }

        public void ReloadConfiguration(IConfigSource m_config)
        {
        }

        public string Name
        {
            get { return "StandaloneLoader"; }
        }

        public void Dispose()
        {
        }

        #endregion

        public void LoadFromFile()
        {
            GZipStream m_loadStream = new GZipStream(ArchiveHelpers.GetStream(m_fileName), CompressionMode.Decompress);
            TarArchiveReader reader = new TarArchiveReader(m_loadStream);

            byte[] data;
            string filePath;
            TarArchiveReader.TarEntryType entryType;


            #region Our Region Info

            IParcelServiceConnector parcelService = Aurora.DataManager.DataManager.RequestPlugin<IParcelServiceConnector>();
            
            SceneManager sceneManager = m_simulationBase.ApplicationRegistry.RequestModuleInterface<SceneManager>();
            RegionInfo regionInfo = new RegionInfo();
            IScene fakeScene = new Scene();
            fakeScene.AddModuleInterfaces(m_simulationBase.ApplicationRegistry.GetInterfaces());

            #endregion

            while ((data = reader.ReadEntry(out filePath, out entryType)) != null)
            {
                if (TarArchiveReader.TarEntryType.TYPE_DIRECTORY == entryType)
                    continue;

                if (filePath.StartsWith("estate/"))
                {
                    string estateData = Encoding.UTF8.GetString(data);
                    EstateSettings settings = new EstateSettings(WebUtils.ParseXmlResponse(estateData));
                    regionInfo.EstateSettings = settings;
                }
                else if (filePath.StartsWith("regioninfo/"))
                {
                    regionInfo.UnpackRegionInfoData((OSDMap)OSDParser.DeserializeLLSDBinary(data));
                }
            }

            if (!m_useExistingRegionInfo)
            {
                regionInfo.RegionID = UUID.Random();
                regionInfo.RegionName = MainConsole.Instance.Prompt("Region Name: ", regionInfo.RegionName);
                regionInfo.RegionLocX = int.Parse(MainConsole.Instance.Prompt("Region Position X: ", regionInfo.RegionLocX.ToString()));
                regionInfo.RegionLocY = int.Parse(MainConsole.Instance.Prompt("Region Position Y: ", regionInfo.RegionLocY.ToString()));
                regionInfo.InternalEndPoint.Port = int.Parse(MainConsole.Instance.Prompt("HTTP Port: ", regionInfo.InternalEndPoint.Port.ToString()));
            }

            //ISimulationDataStore simulationStore = sceneManager.SimulationDataService;
            //Hijack the old simulation service and replace it with ours
            sceneManager.SimulationDataService = new OverridenFileBasedSimulationData (m_fileName, m_saveNewArchiveAtClose);

            ///Now load the region!
            sceneManager.AllRegions++;
            sceneManager.StartNewRegion (regionInfo);
        }
    }

    public class OverridenFileBasedSimulationData : Aurora.Modules.Startup.FileBasedSimulationData.FileBasedSimulationData
    {
        private string fileName;
        private bool saveOnShutdown;
        public OverridenFileBasedSimulationData (string fileName, bool saveOnShutdown)
        {
            this.fileName = fileName;
            this.saveOnShutdown = saveOnShutdown;
        }

        //Not used
        public OverridenFileBasedSimulationData ()
        {
        }

        public override ISimulationDataStore Copy ()
        {
            return this;
        }

        public override string Name
        {
            get
            {
                return "OverridenFileBasedSimulationData";
            }
        }

        protected override void ReadConfig (IScene scene, IConfig config)
        {
            base.ReadConfig (scene, config);
            m_fileName = fileName; //Fix the file name
            m_loadDirectory = "";
        }

        public override void Shutdown ()
        {
            if(saveOnShutdown)
                base.Shutdown ();
        }
    }
}
