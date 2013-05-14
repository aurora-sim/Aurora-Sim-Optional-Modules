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
using Aurora.Simulation.Base;
using Aurora.Framework.Serialization;

using Aurora.Modules.Archivers;
using Aurora.Framework.Services;
using Aurora.Framework.Modules;
using Aurora.Framework.SceneInfo;
using Aurora.Framework.Services.ClassHelpers.Assets;
using Aurora.Framework.Services.ClassHelpers.Inventory;
using Aurora.Region;
using Aurora.Framework.ConsoleFramework;
using Aurora.Framework.Utilities;

namespace OpenSim.Services.InventoryService
{
    /// <summary>
    /// This plugin changes the default asset and inventory folders over into IARs so they can be loaded easier.
    /// </summary>
    public class DefaultInventoryToIARConverter : IService
    {
        protected ILibraryService m_service;
        protected IConfigSource m_config;
        protected IRegistryCore m_registry;

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            m_config = config;
            m_registry = registry;
        }

        public void PostInitialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
        }

        public void FinishedStartup()
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
            IConfig libConfig = m_config.Configs["DefaultAssetsIARCreator"];
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
            m_service = m_registry.RequestModuleInterface<ILibraryService>();

            RegionInfo regInfo = new RegionInfo();
            IScene m_MockScene = null;
            //Make the scene for the IAR loader
            if (m_registry is IScene)
                m_MockScene = (IScene)m_registry;
            else
            {
                m_MockScene = new Scene();
                m_MockScene.Initialize(regInfo);
                m_MockScene.AddModuleInterfaces(m_registry.GetInterfaces());
            }

            UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(null, m_service.LibraryOwner);
            //Make the user account for the default IAR
            if (uinfo == null)
            {
                uinfo = new UserAccount(m_service.LibraryOwner);
                uinfo.Name = m_service.LibraryOwnerName;
                //m_MockScene.InventoryService.CreateUserInventory(m_service.LibraryOwner, false);
                MainConsole.Instance.InfoFormat("[DefaultInventoryToIARConverter]: 1,1");
                InventoryFolderBase newFolder = new InventoryFolderBase
                                                    {
                                                	Name = "My Inventory",
                                                	Type = 9,
                                                	Version = 1,
                                                	ID = new UUID("00000112-000f-0000-0000-000100bba000"),
                                                	Owner = m_service.LibraryOwner,
                                                	ParentID = UUID.Zero
                                                    };
                MainConsole.Instance.InfoFormat("[DefaultInventoryToIARConverter]: 1,3");
            }

            MainConsole.Instance.InfoFormat("[DefaultInventoryToIARConverter]: 1,4");
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
    }
}
