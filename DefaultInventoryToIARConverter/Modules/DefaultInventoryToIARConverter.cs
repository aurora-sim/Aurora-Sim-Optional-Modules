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
                m_MockScene = new Scene();
                m_MockScene.Initialize(regInfo);
                m_MockScene.AddModuleInterfaces(registry.GetInterfaces());
            }

            UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(UUID.Zero, m_service.LibraryOwner);
            //Make the user account for the default IAR
            if (uinfo == null)
            {
                uinfo = new UserAccount(m_service.LibraryOwner);
                uinfo.Name = m_service.LibraryOwnerName;
                uinfo.ServiceURLs = new Dictionary<string, object>();
                m_MockScene.InventoryService.CreateUserInventory(m_service.LibraryOwner, false);
            }

            List<AssetBase> assets = new List<AssetBase> ();
            if (m_MockScene.InventoryService != null)
            {
                //Add the folders to the user's inventory
                InventoryCollection i = m_MockScene.InventoryService.GetFolderContent (m_service.LibraryOwner, UUID.Zero);
                if (i != null)
                {
                    foreach (InventoryItemBase item in i.Items)
                    {
                        AssetBase asset = m_MockScene.RequestModuleInterface<IAssetService> ().Get (item.AssetID.ToString ());
                        if (asset != null)
                            assets.Add (asset);
                    }
                }
            }
            InventoryFolderBase rootFolder = null;
            List<InventoryFolderBase> rootFolders = m_MockScene.InventoryService.GetRootFolders (m_service.LibraryOwner);
            foreach (InventoryFolderBase folder in rootFolders)
            {
                if (folder.Name == "My Inventory")
                    continue;

                rootFolder = folder;
                break;
            }
            if (rootFolder != null)
            {
                //Save the IAR of the default assets
                InventoryArchiveWriteRequest write = new InventoryArchiveWriteRequest (Guid.NewGuid (), null, m_MockScene,
                    uinfo, "/", new GZipStream (new FileStream (IARName, FileMode.Create), CompressionMode.Compress), true, rootFolder, assets);
                write.Execute ();
            }
        }

        public void FinishedStartup()
        {
        }
    }
}