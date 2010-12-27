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
    public class DefaultLibraryLoader : IService
    {
        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void PostInitialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
        }

        public void PostStart(IConfigSource config, IRegistryCore registry)
        {
            string IARName = "DefaultInventory.iar";
            IConfig libConfig = config.Configs["DefaultAssetsIARCreator"];
            if (libConfig != null)
            {
                if (!libConfig.GetBoolean("Enabled", false))
                    return;
                IARName = libConfig.GetString("NameOfIAR", IARName);
            }
            else
                return;
            ILibraryService service = registry.Get<ILibraryService>();

            RegionInfo regInfo = new RegionInfo();
            Scene m_MockScene = null;
            //Make the scene for the IAR loader
            if (registry is Scene)
                m_MockScene = (Scene)registry;
            else
            {
                new Scene(regInfo);
                m_MockScene.AddModuleInterfaces(registry.GetInterfaces());
            }

            UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(UUID.Zero, service.LibraryOwner);
            //Make the user account for the default IAR
            if (uinfo == null)
            {
                uinfo = new UserAccount(service.LibraryOwner);
                uinfo.FirstName = service.LibraryOwnerName[0];
                uinfo.LastName = service.LibraryOwnerName[1];
                uinfo.ServiceURLs = new Dictionary<string, object>();
                m_MockScene.InventoryService.CreateUserInventory(service.LibraryOwner);
            }

            if (m_MockScene.InventoryService != null)
            {
                //Add the folders to the user's inventory
                InventoryFolderBase i = m_MockScene.InventoryService.GetFolder(service.LibraryRootFolder);
                if (i == null)
                {
                    BuildInventoryFolder(m_MockScene, service.LibraryRootFolder);
                }
            }

            //Save the IAR of the default assets
            InventoryArchiveWriteRequest write = new InventoryArchiveWriteRequest(Guid.NewGuid(), null, m_MockScene,
                uinfo, "/", new GZipStream(new FileStream(IARName, FileMode.Create), CompressionMode.Compress), true, service.LibraryRootFolder);
            write.Execute();
        }

        /// <summary>
        /// Add the folders to the user's inventory
        /// </summary>
        /// <param name="m_MockScene"></param>
        /// <param name="folder"></param>
        private void BuildInventoryFolder(Scene m_MockScene, InventoryFolderImpl folder)
        {
            m_MockScene.InventoryService.AddFolder(folder);
            foreach (InventoryFolderImpl childFolder in folder.RequestListOfFolderImpls())
            {
                BuildInventoryFolder(m_MockScene, childFolder);
            }

            foreach (InventoryItemBase item in folder.RequestListOfItems())
            {
                m_MockScene.InventoryService.AddItem(item);
            }
        }

        public void AddNewRegistry(IConfigSource config, IRegistryCore registry)
        {
        }
    }
}