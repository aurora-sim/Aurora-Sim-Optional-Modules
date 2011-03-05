using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;

using OpenSim.Framework;
using OpenSim.Services.Interfaces;

using log4net;
using Nini.Config;
using OpenMetaverse;
using Aurora.Simulation.Base;

using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.CoreModules.Avatar.Inventory.Archiver;

namespace OpenSim.Services.InventoryService
{
    /// <summary>
    /// This plugin changes the default asset and inventory folders over into IARs so they can be loaded easier.
    /// </summary>
    public class DefaultInventoryToIARConverter : IService
    {
        protected static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected ILibraryService m_service;

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void PostInitialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            string IARName = "DefaultInventory.iar";
            IniConfigSource iniSource = null;
            try
            {
                iniSource = new IniConfigSource("DefaultInventory/Inventory.ini", Nini.Ini.IniFileType.AuroraStyle);
            }
            catch
            {
            }
            IConfig libConfig = config.Configs["DefaultAssetsIARCreator"];
            if (libConfig == null)
                libConfig = iniSource.Configs["DefaultAssetsIARCreator"];
            if (libConfig != null)
            {
                if (!libConfig.GetBoolean("Enabled", false))
                    return;
                IARName = libConfig.GetString("NameOfIAR", IARName);
            }
            else
                return;
            m_service = registry.RequestModuleInterface<ILibraryService>();

            RegionInfo regInfo = new RegionInfo();
            Scene m_MockScene = null;
            //Make the scene for the IAR loader
            if (registry is Scene)
                m_MockScene = (Scene)registry;
            else
            {
                m_MockScene = new Scene(regInfo);
                m_MockScene.AddModuleInterfaces(registry.GetInterfaces());
            }

            UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(UUID.Zero, m_service.LibraryOwner);
            //Make the user account for the default IAR
            if (uinfo == null)
            {
                uinfo = new UserAccount(m_service.LibraryOwner);
                uinfo.Name = m_service.LibraryOwnerName;
                uinfo.ServiceURLs = new Dictionary<string, object>();
                m_MockScene.InventoryService.CreateUserInventory(m_service.LibraryOwner);
            }

            if (m_MockScene.InventoryService != null)
            {
                //Add the folders to the user's inventory
                InventoryFolderBase i = m_MockScene.InventoryService.GetFolder(m_service.LibraryRootFolder);
                if (i == null)
                {
                    BuildInventoryFolder(m_MockScene, m_service.LibraryRootFolder);
                }
            }

            List<AssetBase> assets = new List<AssetBase>();
            IConfig assetConfig = config.Configs["DefaultXMLAssetLoader"];
            if (assetConfig == null)
                assetConfig = iniSource.Configs["DefaultXMLAssetLoader"];
            if (assetConfig != null)
            {
                string loaderArgs = assetConfig.GetString("AssetLoaderArgs",
                            String.Empty);
                ForEachDefaultXmlAsset(loaderArgs, delegate(AssetBase asset)
                {
                    assets.Add(asset);
                });
            }

            //Save the IAR of the default assets
            InventoryArchiveWriteRequest write = new InventoryArchiveWriteRequest(Guid.NewGuid(), null, m_MockScene,
                uinfo, "/", new GZipStream(new FileStream(IARName, FileMode.Create), CompressionMode.Compress), true, m_service.LibraryRootFolder, assets);
            write.Execute();
        }

        public void FinishedStartup()
        {
        }

        protected AssetBase CreateAsset(string assetIdStr, string name, string path, sbyte type)
        {
            AssetBase asset = new AssetBase(new UUID(assetIdStr), name, type, m_service.LibraryOwner.ToString());

            if (!String.IsNullOrEmpty(path))
            {
                //m_log.InfoFormat("[ASSETS]: Loading: [{0}][{1}]", name, path);

                LoadAsset(asset, path);
            }
            else
            {
                m_log.InfoFormat("[ASSETS]: Instantiated: [{0}]", name);
            }

            return asset;
        }

        protected static void LoadAsset(AssetBase info, string path)
        {
            //            bool image =
            //               (info.Type == (sbyte)AssetType.Texture ||
            //                info.Type == (sbyte)AssetType.TextureTGA ||
            //                info.Type == (sbyte)AssetType.ImageJPEG ||
            //                info.Type == (sbyte)AssetType.ImageTGA);

            FileInfo fInfo = new FileInfo(path);
            long numBytes = fInfo.Length;
            if (fInfo.Exists)
            {
                FileStream fStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] idata = new byte[numBytes];
                BinaryReader br = new BinaryReader(fStream);
                idata = br.ReadBytes((int)numBytes);
                br.Close();
                fStream.Close();
                info.Data = idata;
                //info.loaded=true;
            }
            else
            {
                m_log.ErrorFormat("[ASSETS]: file: [{0}] not found !", path);
            }
        }

        protected void ForEachDefaultXmlAsset(string assetSetFilename, Action<AssetBase> action)
        {
            List<AssetBase> assets = new List<AssetBase>();
            if (File.Exists(assetSetFilename))
            {
                string assetSetPath = "ERROR";
                string assetRootPath = "";
                try
                {
                    DateTime start = DateTime.Now;
                    XmlConfigSource source = new XmlConfigSource(assetSetFilename);
                    assetRootPath = Path.GetFullPath(source.SavePath);
                    assetRootPath = Path.GetDirectoryName(assetRootPath);

                    for (int i = 0; i < source.Configs.Count; i++)
                    {
                        assetSetPath = source.Configs[i].GetString("file", String.Empty);

                        LoadXmlAssetSet(Path.Combine(assetRootPath, assetSetPath), assets);
                    }
                    m_log.Warn((DateTime.Now - start).Milliseconds);
                }
                catch (XmlException e)
                {
                    m_log.ErrorFormat("[ASSETS]: Error loading {0} : {1}", assetSetPath, e);
                }
            }
            else
            {
                m_log.ErrorFormat("[ASSETS]: Asset set control file {0} does not exist!  No assets loaded.", assetSetFilename);
            }

            DateTime start2 = DateTime.Now;
            assets.ForEach(action);
            m_log.Warn((DateTime.Now - start2).Milliseconds);
        }

        /// <summary>
        /// Use the asset set information at path to load assets
        /// </summary>
        /// <param name="assetSetPath"></param>
        /// <param name="assets"></param>
        protected void LoadXmlAssetSet(string assetSetPath, List<AssetBase> assets)
        {
            //m_log.InfoFormat("[ASSETS]: Loading asset set {0}", assetSetPath);

            if (File.Exists(assetSetPath))
            {
                try
                {
                    XmlConfigSource source = new XmlConfigSource(assetSetPath);
                    String dir = Path.GetDirectoryName(assetSetPath);

                    for (int i = 0; i < source.Configs.Count; i++)
                    {
                        string assetIdStr = source.Configs[i].GetString("assetID", UUID.Random().ToString());
                        string name = source.Configs[i].GetString("name", String.Empty);
                        sbyte type = (sbyte)source.Configs[i].GetInt("assetType", 0);
                        string assetPath = Path.Combine(dir, source.Configs[i].GetString("fileName", String.Empty));

                        AssetBase newAsset = CreateAsset(assetIdStr, name, assetPath, type);

                        newAsset.Type = type;
                        assets.Add(newAsset);
                    }
                }
                catch (XmlException e)
                {
                    m_log.ErrorFormat("[ASSETS]: Error loading {0} : {1}", assetSetPath, e);
                }
            }
            else
            {
                m_log.ErrorFormat("[ASSETS]: Asset set file {0} does not exist!", assetSetPath);
            }
        }

        /// <summary>
        /// Add the folders to the user's inventory
        /// </summary>
        /// <param name="m_MockScene"></param>
        /// <param name="folder"></param>
        private void BuildInventoryFolder(Scene m_MockScene, InventoryFolderImpl folder)
        {
            InventoryFolderBase folderBase = new InventoryFolderBase();
            folderBase.ID = folder.ID;
            folderBase.Name = folder.Name;
            folderBase.Owner = folder.Owner;
            folderBase.ParentID = folder.ParentID;
            folderBase.Type = folder.Type;
            folderBase.Version = folder.Version;

            m_MockScene.InventoryService.AddFolder(folderBase);
            foreach (InventoryFolderImpl childFolder in folder.RequestListOfFolderImpls())
            {
                BuildInventoryFolder(m_MockScene, childFolder);
            }

            foreach (InventoryItemBase item in folder.RequestListOfItems())
            {
                m_MockScene.InventoryService.AddItem(item);
            }
        }
    }
}