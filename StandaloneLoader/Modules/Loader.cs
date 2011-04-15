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
    public class Loader : IApplicationPlugin
    {
        #region Declares

        private ISimulationBase m_simulationBase;
        private bool m_enabled = false;
        private bool m_inuse = false;
        private string m_fileName = "";

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
                key.SetValue("", "Aurora");
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
            ITerrainChannel terrainChannel = null;
            List<SceneObjectGroup> groups = new List<SceneObjectGroup>();
            List<LandData> parcels = new List<LandData>();
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
                else if (filePath.StartsWith("parcels/"))
                {
                    LandData parcel = new LandData();
                    OSD parcelData = OSDParser.DeserializeLLSDBinary(data);
                    parcel.FromOSD((OSDMap)parcelData);
                    parcels.Add(parcel);
                }
                else if (filePath.StartsWith("terrain/"))
                {
                    ITerrainLoader[] terrainLoaders = Aurora.Framework.AuroraModuleLoader.PickupModules<ITerrainLoader>().ToArray();
                    foreach (ITerrainLoader loader in terrainLoaders)
                    {
                        if (loader.FileExtension == ".r32")
                        {
                            MemoryStream ms = new MemoryStream(data);
                            terrainChannel = loader.LoadStream(ms, null);
                            ms.Close();
                            break;
                        }
                    }
                }
                else if (filePath.StartsWith("entities/"))
                {
                    MemoryStream ms = new MemoryStream(data);
                    groups.Add(SceneObjectSerializer.FromXml2Format(ms, (Scene)fakeScene));
                }
            }

            ISimulationDataStore simulationStore = sceneManager.SimulationDataService;

            //Remove any old information
            simulationStore.RemoveRegion(regionInfo.RegionID);
            parcelService.RemoveLandObject(regionInfo.RegionID);


            simulationStore.StoreTerrain(terrainChannel.GetSerialised(null), regionInfo.RegionID, false);

            foreach (LandData parcel in parcels)
            {
                parcelService.StoreLandObject(parcel);
            }
            foreach (SceneObjectGroup grp in groups)
            {
                simulationStore.StoreObject(grp, regionInfo.RegionID);
                foreach (SceneObjectPart part in grp.ChildrenList)
                {
                    simulationStore.StorePrimInventory(part.UUID, part.Inventory.GetInventoryItems());
                }
            }
            ///Now load the region!
            IScene scene;
            sceneManager.AllRegions++;
            sceneManager.CreateRegion(regionInfo, out scene);
        }
    }
}
