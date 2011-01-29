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

using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.CoreModules.Avatar.Inventory.Archiver;

namespace Aurora.DefaultLibraryLoaders
{
    public class InventoryXMLLoader : IDefaultLibraryLoader
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected ILibraryService m_service;
        protected IInventoryService m_inventoryService;
        protected InventoryFolderImpl m_folder;

        public void LoadLibrary(ILibraryService service, IConfigSource source, IRegistryCore registry)
        {
            m_service = service;
            m_inventoryService = registry.RequestModuleInterface<IInventoryService>();
            m_folder = new InventoryFolderImpl();

            IConfig libConfig = source.Configs["InventoryXMLLoader"];
            string pLibrariesLocation = Path.Combine("inventory", "Libraries.xml");
            if (libConfig != null)
            {
                if (libConfig.GetBoolean("PreviouslyLoaded", false))
                    return; //If it is loaded, don't reload
                pLibrariesLocation = libConfig.GetString("DefaultLibrary", pLibrariesLocation);
                LoadLibraries(pLibrariesLocation);
                m_service.AddToDefaultInventory(m_folder);
            }
        }

        private InventoryItemBase CreateItem(UUID inventoryID, UUID assetID, string name, string description,
                                            int assetType, int invType, UUID parentFolderID)
        {
            InventoryItemBase item = new InventoryItemBase();
            item.Owner = m_service.LibraryOwner;
            item.CreatorId = m_service.LibraryOwner.ToString();
            item.ID = inventoryID;
            item.AssetID = assetID;
            item.Description = description;
            item.Name = name;
            item.AssetType = assetType;
            item.InvType = invType;
            item.Folder = parentFolderID;
            item.BasePermissions = 0x7FFFFFFF;
            item.EveryOnePermissions = 0x7FFFFFFF;
            item.CurrentPermissions = 0x7FFFFFFF;
            item.NextPermissions = 0x7FFFFFFF;
            return item;
        }

        /// <summary>
        /// Use the asset set information at path to load assets
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assets"></param>
        protected void LoadLibraries(string librariesControlPath)
        {
            m_log.InfoFormat("[LIBRARY INVENTORY]: Loading library control file {0}", librariesControlPath);
            LoadFromFile(librariesControlPath, "Libraries control", ReadLibraryFromConfig);
        }

        /// <summary>
        /// Read a library set from config
        /// </summary>
        /// <param name="config"></param>
        protected void ReadLibraryFromConfig(IConfig config, string path)
        {
            string basePath = Path.GetDirectoryName(path);
            string foldersPath
                = Path.Combine(
                    basePath, config.GetString("foldersFile", String.Empty));

            LoadFromFile(foldersPath, "Library folders", ReadFolderFromConfig);

            string itemsPath
                = Path.Combine(
                    basePath, config.GetString("itemsFile", String.Empty));

            LoadFromFile(itemsPath, "Library items", ReadItemFromConfig);
        }

        /// <summary>
        /// Read a library inventory folder from a loaded configuration
        /// </summary>
        /// <param name="source"></param>
        private void ReadFolderFromConfig(IConfig config, string path)
        {
            InventoryFolderImpl folderInfo = new InventoryFolderImpl();

            folderInfo.ID = new UUID(config.GetString("folderID", m_service.LibraryRootFolder.ID.ToString()));
            folderInfo.Name = config.GetString("name", "unknown");
            folderInfo.ParentID = new UUID(config.GetString("parentFolderID", m_service.LibraryRootFolder.ID.ToString()));
            folderInfo.Type = (short)config.GetInt("type", 8);

            folderInfo.Owner = m_service.LibraryOwner;
            folderInfo.Version = 1;

            m_inventoryService.AddFolder(folderInfo);
            m_folder.AddChildFolder(folderInfo);
        }

        /// <summary>
        /// Read a library inventory item metadata from a loaded configuration
        /// </summary>
        /// <param name="source"></param>
        private void ReadItemFromConfig(IConfig config, string path)
        {
            InventoryItemBase item = new InventoryItemBase();
            item.Owner = m_service.LibraryOwner;
            item.CreatorId = m_service.LibraryOwner.ToString();
            item.ID = new UUID(config.GetString("inventoryID", m_service.LibraryRootFolder.ID.ToString()));
            item.AssetID = new UUID(config.GetString("assetID", item.ID.ToString()));
            item.Folder = new UUID(config.GetString("folderID", m_service.LibraryRootFolder.ID.ToString()));
            item.Name = config.GetString("name", String.Empty);
            item.Description = config.GetString("description", item.Name);
            item.InvType = config.GetInt("inventoryType", 0);
            item.AssetType = config.GetInt("assetType", item.InvType);
            item.CurrentPermissions = (uint)config.GetLong("currentPermissions", 0x7FFFFFFF);
            item.NextPermissions = (uint)config.GetLong("nextPermissions", 0x7FFFFFFF);
            item.EveryOnePermissions = (uint)config.GetLong("everyonePermissions", 0x7FFFFFFF);
            item.BasePermissions = (uint)config.GetLong("basePermissions", 0x7FFFFFFF);
            item.Flags = (uint)config.GetInt("flags", 0);

            m_inventoryService.AddItem(item);
            if (item.Folder == m_service.LibraryRootFolder.ID)
                m_folder.Items.Add(item.ID, item);
            else
                m_folder.FindFolder(item.Folder).Items.Add(item.ID, item);
        }

        private delegate void ConfigAction(IConfig config, string path);

        /// <summary>
        /// Load the given configuration at a path and perform an action on each Config contained within it
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileDescription"></param>
        /// <param name="action"></param>
        private void LoadFromFile(string path, string fileDescription, ConfigAction action)
        {
            if (File.Exists(path))
            {
                try
                {
                    XmlConfigSource source = new XmlConfigSource(path);

                    for (int i = 0; i < source.Configs.Count; i++)
                    {
                        action(source.Configs[i], path);
                    }
                }
                catch (XmlException e)
                {
                    m_log.ErrorFormat("[LIBRARY INVENTORY]: Error loading {0} : {1}", path, e);
                }
            }
            else
            {
                m_log.ErrorFormat("[LIBRARY INVENTORY]: {0} file {1} does not exist!", fileDescription, path);
            }
        }
    }
}