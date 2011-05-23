using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Reflection;
using OpenSim.Framework;
using OpenSim.Framework.Serialization;
using OpenSim.Framework.Serialization.External;
using Aurora.Framework;
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
using OpenSim.Region.CoreModules.World.Terrain;
using OpenSim.Region.Framework.Scenes.Serialization;

namespace Aurora.StandaloneLoader
{
    public class Loader : IApplicationPlugin, ISimulationDataStore
    {
        #region Declares

        private ISimulationBase m_simulationBase;
        private bool m_enabled = false;
        private bool m_inuse = false;
        private string m_fileName = "";
        private bool m_saveNewArchiveAtClose = false;
        private bool m_useExistingRegionInfo = true;
        private IScene m_scene;
            
        #endregion

        #region IApplicationPlugin Members

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
                //Register the extention
                string ext = ".abackup";
                RegistryKey key = Registry.ClassesRoot.CreateSubKey(ext);
                key.SetValue("", "abackup file");
                key.Close();

                key = Registry.ClassesRoot.CreateSubKey(ext + "\\Shell\\Open\\command");

                key.SetValue("", "\"" + Application.ExecutablePath + "\" \"%L\"");
                key.Close();
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
                regionInfo.RegionName = MainConsole.Instance.CmdPrompt("Region Name: ", regionInfo.RegionName);
                regionInfo.RegionLocX = int.Parse(MainConsole.Instance.CmdPrompt("Region Position X: ", regionInfo.RegionLocX.ToString()));
                regionInfo.RegionLocY = int.Parse(MainConsole.Instance.CmdPrompt("Region Position Y: ", regionInfo.RegionLocY.ToString()));
                regionInfo.HttpPort = uint.Parse(MainConsole.Instance.CmdPrompt("HTTP Port: ", regionInfo.HttpPort.ToString()));

                string externalName = MainConsole.Instance.CmdPrompt("IP: ", "DEFAULT");
                if (externalName == "DEFAULT")
                {
                    externalName = Aurora.Framework.Utilities.GetExternalIp();
                    regionInfo.FindExternalAutomatically = true;
                }
                else
                    regionInfo.FindExternalAutomatically = false;
                regionInfo.ExternalHostName = externalName;
            }

            //ISimulationDataStore simulationStore = sceneManager.SimulationDataService;
            //Hijack the old simulation service and replace it with ours
            sceneManager.SimulationDataService = new OverridenFileBasedSimulationData (m_fileName, m_saveNewArchiveAtClose);

            ///Now load the region!
            sceneManager.AllRegions++;
            sceneManager.CreateRegion (regionInfo, out m_scene);
        }
    }

    public class OverridenFileBasedSimulationData : Aurora.Modules.FileBasedSimulationData.FileBasedSimulationData
    {
        private string fileName;
        private bool saveOnShutdown;
        public OverridenFileBasedSimulationData (string fileName, bool saveOnShutdown)
        {
            this.fileName = fileName;
            this.saveOnShutdown = saveOnShutdown;
        }

        public override ISimulationDataStore Copy ()
        {
            return this;
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
