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
using System.Reflection;
using System.Xml;

using Aurora.Framework;

using Nini.Config;
using OpenMetaverse;

using Aurora.Modules;
using Aurora.Framework.Services;
using Aurora.Framework.Modules;
using Aurora.Framework.Services.ClassHelpers.Inventory;
using Aurora.Framework.ConsoleFramework;

namespace Aurora.DefaultLibraryLoaders
{
    public class InventoryXMLLoader : IDefaultLibraryLoader
    {
        protected ILibraryService m_service;
        protected IInventoryService m_inventoryService;

        public void LoadLibrary(ILibraryService service, IConfigSource source, IRegistryCore registry)
        {
            m_service = service;
            m_inventoryService = registry.RequestModuleInterface<IInventoryService>();

            IConfig libConfig = source.Configs["InventoryXMLLoader"];
            string pLibrariesLocation = Path.Combine("inventory", "Libraries.xml");
            if (libConfig != null)
            {
                if (libConfig.GetBoolean("PreviouslyLoaded", false))
                    return; //If it is loaded, don't reload
                pLibrariesLocation = libConfig.GetString("DefaultLibrary", pLibrariesLocation);
                LoadLibraries(pLibrariesLocation);
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
            MainConsole.Instance.InfoFormat("[LIBRARY INVENTORY]: Loading library control file {0}", librariesControlPath);
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

            folderInfo.ID = new UUID(config.GetString("folderID", UUID.Random().ToString()));
            folderInfo.Name = config.GetString("name", "unknown");
            folderInfo.ParentID = new UUID (config.GetString ("parentFolderID", UUID.Zero.ToString ()));
            folderInfo.Type = (short)config.GetInt("type", 8);

            folderInfo.Owner = m_service.LibraryOwner;
            folderInfo.Version = 1;

            m_inventoryService.AddFolder(folderInfo);
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
            item.ID = new UUID (config.GetString ("inventoryID", UUID.Random().ToString()));
            item.AssetID = new UUID(config.GetString("assetID", item.ID.ToString()));
            item.Folder = new UUID (config.GetString ("folderID", UUID.Zero.ToString ()));
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
                    MainConsole.Instance.ErrorFormat("[LIBRARY INVENTORY]: Error loading {0} : {1}", path, e);
                }
            }
            else
            {
                MainConsole.Instance.ErrorFormat("[LIBRARY INVENTORY]: {0} file {1} does not exist!", fileDescription, path);
            }
        }
    }
}
