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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using OpenSim.Framework;
using OpenSim.Framework.Serialization;
using OpenSim.Framework.Serialization.External;
using OpenSim.Services.Interfaces;

using log4net;
using Nini.Config;
using OpenMetaverse;

using OpenSim.Region.CoreModules.Avatar.Inventory.Archiver;

namespace IARModifierGUI
{
    public partial class IAREditor : Form
    {
        #region Declares/Constructors

        private string m_iarName;
        private Dictionary<string, AssetBase> m_loadedAssets = new Dictionary<string, AssetBase> ();
        private List<InventoryFolderBase> m_rootFolders = new List<InventoryFolderBase>();
        private Dictionary<UUID, List<InventoryItemBase>> m_items = new Dictionary<UUID, List<InventoryItemBase>> ();
        private List<InventoryFolderBase> m_folders = new List<InventoryFolderBase> ();
        private Dictionary<UUID, List<InventoryFolderBase>> m_childFolders = new Dictionary<UUID, List<InventoryFolderBase>> ();

        private Dictionary<UUID, InventoryItemBase> m_itemList = new Dictionary<UUID, InventoryItemBase> ();
        private Dictionary<UUID, InventoryFolderBase> m_folderList = new Dictionary<UUID, InventoryFolderBase> ();

        public IAREditor (string IARName)
        {
            InitializeComponent ();
            m_iarName = IARName;
        }

        public IAREditor ()
        {
            InitializeComponent ();
            m_iarName = SelectTextFile (Environment.CurrentDirectory);
            if (m_iarName == null)
                Environment.Exit(0);
        }

        #endregion

        #region Startup/Rebuild

        private void IAREditor_Load (object sender, EventArgs e)
        {
            LoadIAR (m_iarName);
        }

        private void RebuildTreeView ()
        {
            m_rootFolders.Clear ();
            UUID previouslySelectedID = treeView1.SelectedNode != null ? UUID.Parse (treeView1.SelectedNode.Name) : UUID.Zero;
            TreeNode selectedNode = null;
            TreeNode rootNode = new TreeNode ("My Inventory");
            Dictionary<UUID, TreeNode> nodes = new Dictionary<UUID, TreeNode> ();
            foreach (InventoryFolderBase folder in m_folders)
            {
                if (folder.ParentID == UUID.Zero)
                {
                    TreeNode node = new TreeNode (folder.Name);
                    node.Name = folder.ID.ToString ();
                    if (previouslySelectedID == folder.ID)
                        selectedNode = node;
                    if (m_items.ContainsKey (folder.ID))
                    {
                        foreach (InventoryItemBase item in m_items[folder.ID])
                        {
                            TreeNode inventoryNode = new TreeNode (item.Name);
                            inventoryNode.Name = item.ID.ToString ();
                            if (previouslySelectedID == item.ID)
                                selectedNode = inventoryNode;
                            node.Nodes.Add (inventoryNode);
                        }
                    }
                    nodes.Add (folder.ID, node);
                    rootNode.Nodes.Add (node);

                    m_rootFolders.Add(folder);
                }
                else
                {
                    TreeNode parentNode = nodes[folder.ParentID];
                    TreeNode node = new TreeNode (folder.Name);
                    node.Name = folder.ID.ToString ();
                    if (previouslySelectedID == folder.ID)
                        selectedNode = node;
                    if (m_items.ContainsKey (folder.ID))
                    {
                        foreach (InventoryItemBase item in m_items[folder.ID])
                        {
                            TreeNode inventoryNode = new TreeNode (item.Name);
                            inventoryNode.Name = item.ID.ToString ();
                            if (previouslySelectedID == item.ID)
                                selectedNode = inventoryNode;
                            node.Nodes.Add (inventoryNode);
                        }
                    }
                    nodes.Add (folder.ID, node);
                    parentNode.Nodes.Add (node);
                }
            }

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(rootNode);
            if (selectedNode != null)
            {
                treeView1.Select ();
                List<TreeNode> parentNodes = new List<TreeNode>();
                TreeNode node = selectedNode;
                while (true)
                {
                    if (node.Parent == null)
                        break;
                    node.Parent.Toggle ();
                    node = node.Parent;
                }
                treeView1.Update ();
            }
        }

        #endregion

        #region IAR Loading

        private void LoadIAR (string fileName)
        {
            //Load the iar into memory
            TarArchiveReader archive = new TarArchiveReader (new GZipStream (ArchiveHelpers.GetStream (fileName), CompressionMode.Decompress));

            byte[] data;
            TarArchiveReader.TarEntryType entryType;
            string filePath;

            InventoryFolderBase rootDestFolder = new InventoryFolderBase (UUID.Zero, UUID.Zero);
            Dictionary<string, InventoryFolderBase> resolvedFolders = new Dictionary<string, InventoryFolderBase> ();

            while ((data = archive.ReadEntry (out filePath, out entryType)) != null)
            {
                if (filePath.StartsWith (ArchiveConstants.ASSETS_PATH))
                {
                    LoadAsset (filePath, data);
                }
                else if (filePath.StartsWith (ArchiveConstants.INVENTORY_PATH))
                {
                    filePath = filePath.Substring (ArchiveConstants.INVENTORY_PATH.Length);

                    // Trim off the file portion if we aren't already dealing with a directory path
                    if (TarArchiveReader.TarEntryType.TYPE_DIRECTORY != entryType)
                        filePath = filePath.Remove (filePath.LastIndexOf ("/") + 1);

                    InventoryFolderBase foundFolder
                        = ReplicateArchivePathToUserInventory (
                            filePath, rootDestFolder, ref resolvedFolders);

                    if (TarArchiveReader.TarEntryType.TYPE_DIRECTORY != entryType)
                    {
                        LoadItem (data, foundFolder);
                    }
                }
            }

            archive.Close ();


            //Got the .iar loaded into memory now
            // Time to put it into the GUI

            RebuildTreeView ();
        }

        /// <summary>
        /// Replicate the inventory paths in the archive to the user's inventory as necessary.
        /// </summary>
        /// <param name="iarPath">The item archive path to replicate</param>
        /// <param name="rootDestinationFolder">The root folder for the inventory load</param>
        /// <param name="resolvedFolders">
        /// The folders that we have resolved so far for a given archive path.
        /// This method will add more folders if necessary
        /// </param>
        /// <param name="loadedNodes">
        /// Track the inventory nodes created.
        /// </param>
        /// <returns>The last user inventory folder created or found for the archive path</returns>
        public InventoryFolderBase ReplicateArchivePathToUserInventory (
            string iarPath,
            InventoryFolderBase rootDestFolder,
            ref Dictionary<string, InventoryFolderBase> resolvedFolders)
        {
            string iarPathExisting = iarPath;

            //            m_log.DebugFormat(
            //                "[INVENTORY ARCHIVER]: Loading folder {0} {1}", rootDestFolder.Name, rootDestFolder.ID);

            InventoryFolderBase destFolder
                = ResolveDestinationFolder (rootDestFolder, ref iarPathExisting, ref resolvedFolders);

            //            m_log.DebugFormat(
            //                "[INVENTORY ARCHIVER]: originalArchivePath [{0}], section already loaded [{1}]", 
            //                iarPath, iarPathExisting);

            string iarPathToCreate = iarPath.Substring (iarPathExisting.Length);
            CreateFoldersForPath (destFolder, iarPathExisting, iarPathToCreate, ref resolvedFolders);

            return destFolder;
        }

        /// <summary>
        /// Resolve a destination folder
        /// </summary>
        /// 
        /// We require here a root destination folder (usually the root of the user's inventory) and the archive
        /// path.  We also pass in a list of previously resolved folders in case we've found this one previously.
        /// 
        /// <param name="archivePath">
        /// The item archive path to resolve.  The portion of the path passed back is that
        /// which corresponds to the resolved desintation folder.
        /// <param name="rootDestinationFolder">
        /// The root folder for the inventory load
        /// </param>
        /// <param name="resolvedFolders">
        /// The folders that we have resolved so far for a given archive path.
        /// </param>
        /// <returns>
        /// The folder in the user's inventory that matches best the archive path given.  If no such folder was found
        /// then the passed in root destination folder is returned.
        /// </returns>
        protected InventoryFolderBase ResolveDestinationFolder (
            InventoryFolderBase rootDestFolder,
            ref string archivePath,
            ref Dictionary<string, InventoryFolderBase> resolvedFolders)
        {
            //            string originalArchivePath = archivePath;

            while (archivePath.Length > 0)
            {
                //                m_log.DebugFormat("[INVENTORY ARCHIVER]: Trying to resolve destination folder {0}", archivePath);

                if (resolvedFolders.ContainsKey (archivePath))
                {
                    //                    m_log.DebugFormat(
                    //                        "[INVENTORY ARCHIVER]: Found previously created folder from archive path {0}", archivePath);
                    return resolvedFolders[archivePath];
                }
                else
                {
                    // Don't include the last slash so find the penultimate one
                    int penultimateSlashIndex = archivePath.LastIndexOf ("/", archivePath.Length - 2);

                    if (penultimateSlashIndex >= 0)
                    {
                        // Remove the last section of path so that we can see if we've already resolved the parent
                        archivePath = archivePath.Remove (penultimateSlashIndex + 1);
                    }
                    else
                    {
                        //                        m_log.DebugFormat(
                        //                            "[INVENTORY ARCHIVER]: Found no previously created folder for archive path {0}",
                        //                            originalArchivePath);
                        archivePath = string.Empty;
                        return rootDestFolder;
                    }
                }
            }

            return rootDestFolder;
        }

        /// <summary>
        /// Create a set of folders for the given path.
        /// </summary>
        /// <param name="destFolder">
        /// The root folder from which the creation will take place.
        /// </param>
        /// <param name="iarPathExisting">
        /// the part of the iar path that already exists
        /// </param>
        /// <param name="iarPathToReplicate">
        /// The path to replicate in the user's inventory from iar
        /// </param>
        /// <param name="resolvedFolders">
        /// The folders that we have resolved so far for a given archive path.
        /// </param>
        /// <param name="loadedNodes">
        /// Track the inventory nodes created.
        /// </param>
        protected void CreateFoldersForPath (
            InventoryFolderBase destFolder,
            string iarPathExisting,
            string iarPathToReplicate,
            ref Dictionary<string, InventoryFolderBase> resolvedFolders)
        {
            string[] rawDirsToCreate = iarPathToReplicate.Split (new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < rawDirsToCreate.Length; i++)
            {
                //                m_log.DebugFormat("[INVENTORY ARCHIVER]: Creating folder {0} from IAR", rawDirsToCreate[i]);

                if (!rawDirsToCreate[i].Contains (ArchiveConstants.INVENTORY_NODE_NAME_COMPONENT_SEPARATOR))
                    continue;

                int identicalNameIdentifierIndex
                    = rawDirsToCreate[i].LastIndexOf (
                        ArchiveConstants.INVENTORY_NODE_NAME_COMPONENT_SEPARATOR);

                string newFolderName = rawDirsToCreate[i].Remove (identicalNameIdentifierIndex);

                newFolderName = InventoryArchiveUtils.UnescapeArchivePath (newFolderName);
                UUID newFolderId = UUID.Random ();

                // Asset type has to be Unknown here rather than Folder, otherwise the created folder can't be
                // deleted once the client has relogged.
                // The root folder appears to be labelled AssetType.Folder (shows up as "Category" in the client)
                // even though there is a AssetType.RootCategory
                destFolder
                    = new InventoryFolderBase (
                        newFolderId, newFolderName, UUID.Zero,
                        (short)AssetType.Unknown, destFolder.ID, 1);

                // Record that we have now created this folder
                iarPathExisting += rawDirsToCreate[i] + "/";
                resolvedFolders[iarPathExisting] = destFolder;

                m_folders.Add (destFolder);
                if(!m_childFolders.ContainsKey(destFolder.ParentID))
                    m_childFolders.Add (destFolder.ParentID, new List<InventoryFolderBase>());
                m_childFolders[destFolder.ParentID].Add (destFolder);
                m_folderList.Add (destFolder.ID, destFolder);
            }
        }

        /// <summary>
        /// Load an item from the archive
        /// </summary>
        /// <param name="filePath">The archive path for the item</param>
        /// <param name="data">The raw item data</param>
        /// <param name="rootDestinationFolder">The root destination folder for loaded items</param>
        /// <param name="nodesLoaded">All the inventory nodes (items and folders) loaded so far</param>
        protected InventoryItemBase LoadItem (byte[] data, InventoryFolderBase loadFolder)
        {
            InventoryItemBase item = UserInventoryItemSerializer.Deserialize (data);
            item.Folder = loadFolder.ID;
            if(!m_items.ContainsKey(item.Folder))
                m_items.Add (item.Folder, new List<InventoryItemBase>());
            m_items[item.Folder].Add (item);
            m_itemList[item.ID] = item;
            return item;
        }

        /// <summary>
        /// Load an asset
        /// </summary>
        /// <param name="assetFilename"></param>
        /// <param name="data"></param>
        /// <returns>true if asset was successfully loaded, false otherwise</returns>
        private bool LoadAsset (string assetPath, byte[] data)
        {
            //IRegionSerialiser serialiser = scene.RequestModuleInterface<IRegionSerialiser>();
            // Right now we're nastily obtaining the UUID from the filename
            string filename = assetPath.Remove (0, ArchiveConstants.ASSETS_PATH.Length);
            int i = filename.LastIndexOf (ArchiveConstants.ASSET_EXTENSION_SEPARATOR);

            if (i == -1)
            {
                return false;
            }

            string extension = filename.Substring (i);
            string uuid = filename.Remove (filename.Length - extension.Length);

            if (ArchiveConstants.EXTENSION_TO_ASSET_TYPE.ContainsKey (extension))
            {
                AssetType assetType = ArchiveConstants.EXTENSION_TO_ASSET_TYPE[extension];

                //m_log.DebugFormat("[INVENTORY ARCHIVER]: Importing asset {0}, type {1}", uuid, assetType);

                AssetBase asset = new AssetBase (new UUID (uuid), "RandomName", assetType, UUID.Zero);
                asset.Data = data;
                asset.Flags = AssetFlags.Normal;

                m_loadedAssets[asset.IDString] = asset;

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Click actions

        private void delete_Click (object sender, EventArgs e)
        {
            //First see if it is a folder to delete
            UUID id = UUID.Parse(treeView1.SelectedNode.Name);
            if(m_folderList.ContainsKey(id))
            {
                RemoveFolder (id);
            }
            else
            {
                //Its an item!
                InventoryItemBase item = m_itemList[id];
                //Remove it from the lists
                m_itemList.Remove (id);
                m_items[item.Folder].Remove (item);
            }
            //Update the GUI now
            RebuildTreeView ();
        }

        private void RemoveFolder (UUID id)
        {
            InventoryFolderBase folder = m_folderList[id];
            //Remove all the items of the folder
            if (m_items.ContainsKey (id))
            {
                foreach (InventoryItemBase item in m_items[id])
                {
                    m_itemList.Remove (item.ID);
                }
            }
            //Go recursive on the folders
            if (m_childFolders.ContainsKey (id))
            {
                foreach (InventoryFolderBase childFolder in new List<InventoryFolderBase>(m_childFolders[id]))
                {
                    RemoveFolder (childFolder.ID);
                }
            }
            m_items.Remove (id);
            //Remove from the lists
            m_folders.Remove (folder);
            m_folderList.Remove (id);
            m_childFolders.Remove (id);
            m_childFolders[folder.ParentID].Remove (folder);
        }

        private void rename_Click (object sender, EventArgs e)
        {
            string value = "";
            Aurora.Framework.Utilities.InputBox ("Rename", "What should we rename this object to?", ref value);
            UUID id = UUID.Parse (treeView1.SelectedNode.Name);
            if (m_folderList.ContainsKey (id))
            {
                InventoryFolderBase folder = m_folderList[id];
                folder.Name = value;
            }
            else
            {
                //Its an item!
                InventoryItemBase item = m_itemList[id];
                item.Name = value;
            }
            //Update the GUI now
            RebuildTreeView ();
        }

        private bool m_nameChanged = false;
        private void textBox1_TextChanged (object sender, EventArgs e)
        {
            m_nameChanged = true;
        }

        private void textBox1_Leave (object sender, EventArgs e)
        {
            if (!m_nameChanged)
                return;
            m_nameChanged = false;
            string value = textBox1.Text;
            UUID id = UUID.Parse (treeView1.SelectedNode.Name);
            if (m_folderList.ContainsKey (id))
            {
                InventoryFolderBase folder = m_folderList[id];
                folder.Name = value;
            }
            else
            {
                //Its an item!
                InventoryItemBase item = m_itemList[id];
                item.Name = value;
            }
            //Update the GUI now
            RebuildTreeView ();
        }

        private bool m_typeChanged = false;
        private void textBox2_TextChanged (object sender, EventArgs e)
        {
            m_typeChanged = true;
        }

        private void textBox2_Leave (object sender, EventArgs e)
        {
            if (!m_typeChanged)
                return;
            m_typeChanged = false;
            bool isitem = false;
            try
            {
                UUID id = UUID.Parse (treeView1.SelectedNode.Name);
                if (m_folderList.ContainsKey (id))
                {
                    AssetType value = (AssetType)Enum.Parse (typeof (AssetType), textBox2.Text);
                    InventoryFolderBase folder = m_folderList[id];
                    folder.Type = (short)value;
                }
                else
                {
                    InventoryType value = (InventoryType)Enum.Parse (typeof (InventoryType), textBox2.Text);
                    isitem = true;
                    //Its an item!
                    InventoryItemBase item = m_itemList[id];
                    item.InvType = (int)value;
                }
            }
            catch
            {
                if(isitem)
                    MessageBox.Show (@"Valid types are:
        Unknown
        Texture
        Sound
        CallingCard
        Landmark
        Object
        Notecard
        Category
        Folder
        RootCategory
        LSL
        Snapshot
        Attachment
        Wearable
        Animation
        Gesture
        Mesh");
                else
                    MessageBox.Show (@"Valid types are:
        Unknown
        Texture
        Sound
        CallingCard
        Landmark
        Clothing
        Object
        Notecard
        Folder
        RootFolder
        LSLText
        TextureTGA
        Bodypart
        TrashFolder
        SnapshotFolder
        LostAndFoundFolder
        Animation
        Gesture
        Simstate
        FavoriteFolder
        LinkFolder
        CurrentOutfitFolder
        OutfitFolder
        MyOutfitsFolder
        Mesh");
            }
        }

        private void treeView1_NodeMouseDoubleClick (object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                UUID id = UUID.Parse (e.Node.Name);
                if (m_folderList.ContainsKey (id))
                {
                    InventoryFolderBase folder = m_folderList[id];
                    textBox1.Text = folder.Name;
                    textBox2.Text = ((AssetType)folder.Type).ToString();
                }
                else
                {
                    //Its an item!
                    if (m_itemList.ContainsKey (id))
                    {
                        InventoryItemBase item = m_itemList[id];
                        textBox1.Text = item.Name;
                        textBox2.Text = ((InventoryType)item.InvType).ToString ();
                    }
                }
            }
            catch
            {
            }
        }

        private void treeView1_NodeMouseClick_1 (object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1_NodeMouseDoubleClick (sender, e);
        }

        private UUID m_movingObject = UUID.Zero;
        private bool m_moveContents = false;

        private void move_Click (object sender, EventArgs e)
        {
            m_movingObject = UUID.Parse (treeView1.SelectedNode.Name);
            m_moveContents = false;
            MessageBox.Show ("Select the folder you wish to put this object in.");
            //Move the object to the next place the user clicks
            treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler (treeView1_NodeMouseClick);
        }

        private void move_contents_Click (object sender, EventArgs e)
        {
            m_movingObject = UUID.Parse (treeView1.SelectedNode.Name);
            if (!m_folderList.ContainsKey (m_movingObject))
            {
                MessageBox.Show ("Select a valid folder to move it's contents.");
                return;
            }
            m_moveContents = true;
            MessageBox.Show ("Select the folder you wish to put the contents in.");
            //Move the object to the next place the user clicks
            treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler (treeView1_NodeMouseClick);
        }

        private void cancelMove_Click (object sender, EventArgs e)
        {
            treeView1.NodeMouseClick -= new TreeNodeMouseClickEventHandler (treeView1_NodeMouseClick);
            MessageBox.Show ("Move canceled.");
        }

        void treeView1_NodeMouseClick (object sender, TreeNodeMouseClickEventArgs e)
        {
            //Only move once!
            treeView1.NodeMouseClick -= treeView1_NodeMouseClick;

            UUID id = UUID.Parse (e.Node.Name);
            if (m_folderList.ContainsKey (id))
            {
                InventoryFolderBase newfolder = m_folderList[id];

                if (!m_moveContents)
                {
                    if (m_folderList.ContainsKey (m_movingObject))
                    {
                        InventoryFolderBase folder = m_folderList[m_movingObject];
                        //We don't have to move any items, since they all reference the parentID, which is the folder
                        // and the folder is changing its parentID, which doesn't affect the items
                        folder.ParentID = id;
                    }
                    else
                    {
                        //Its an item!
                        InventoryItemBase item = m_itemList[m_movingObject];
                        //Remove us from the old folder
                        m_items[item.Folder].Remove (item);
                        //Set the new folderID
                        item.Folder = id;
                        //Add it in the right place now
                        if (!m_items.ContainsKey (id))
                            m_items.Add (id, new List<InventoryItemBase> ());
                        m_items[id].Add (item);
                    }
                }
                else
                {
                    //We need to move all items in this folder, but not the folder itself
                    MoveFolder (m_movingObject, id);
                }

                //Update the GUI now
                RebuildTreeView ();
            }
            else
            {
                //Its an item!
                MessageBox.Show ("Select a valid folder.");
                treeView1.NodeMouseClick += treeView1_NodeMouseClick;
                return;
            }
        }

        private void MoveFolder (UUID id, UUID newID)
        {
            if (m_items.ContainsKey (id))
            {
                foreach (InventoryItemBase item in m_items[id])
                {
                    //Set the new folderID
                    item.Folder = newID;
                    //Add it in the right place now
                    if (!m_items.ContainsKey (newID))
                        m_items.Add (newID, new List<InventoryItemBase> ());
                    m_items[newID].Add (item);
                }
                //Remove all items from the original folder
                m_items.Remove (m_movingObject);
            }
            if (m_childFolders.ContainsKey (id))
            {
                foreach (InventoryFolderBase folder in m_childFolders[id])
                {
                    //Set the new folderID
                    folder.ParentID = newID;
                    //Add it in the right place now
                    if (!m_childFolders.ContainsKey (newID))
                        m_childFolders.Add (newID, new List<InventoryFolderBase> ());
                    m_childFolders[newID].Add (folder);
                }
                //Remove all items from the original folder
                m_childFolders.Remove (id);
            }
        }

        private void merge_Click (object sender, EventArgs e)
        {
            string fileName = SelectTextFile (Environment.CurrentDirectory);
            if (fileName != null)
                LoadIAR (fileName);
        }

        private string SelectTextFile (string initialDirectory)
        {
            OpenFileDialog dialog = new OpenFileDialog ();
            dialog.Filter =
               "iar files (*.iar)|*.iar|All files (*.*)|*.*";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select an iar file";
            return (dialog.ShowDialog () == DialogResult.OK)
                   ? dialog.FileName : null;
        }

        private string SaveTextFile (string initialDirectory)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog ();
            saveFileDialog1.Filter = "iar files (*.iar)|*.iar|All files (*.*)|*.*";
            saveFileDialog1.InitialDirectory = initialDirectory;
            saveFileDialog1.Title = "Save an iar file";
            return (saveFileDialog1.ShowDialog () == DialogResult.OK)
                   ? saveFileDialog1.FileName : null;
        }

        private void save_Click (object sender, EventArgs e)
        {
            string fileName = SaveTextFile (Environment.CurrentDirectory);
            if (fileName != null)
                Execute (fileName);
        }

        private void button2_Click (object sender, EventArgs e)
        {
            AddAsset assetFinder = new AddAsset (this);
            assetFinder.ShowDialog ();
        }

        public void AddAsset (string id, byte[] data)
        {
            AssetBase a = new AssetBase (id, "Unknown asset", (sbyte)AssetType.Texture, UUID.Zero);
            a.Data = data;
            m_loadedAssets[id] = a;
        }

        #endregion

        #region Save IAR

        protected TarArchiveWriter m_archiveWriter;

        protected void SaveInvItem (InventoryItemBase inventoryItem, string path)
        {
            string filename = path + CreateArchiveItemName (inventoryItem);

            InventoryItemBase saveItem = (InventoryItemBase)inventoryItem.Clone ();
            
            string serialization = UserInventoryItemSerializer.Serialize (saveItem);
            m_archiveWriter.WriteFile (filename, serialization);
        }

        /// <summary>
        /// Save an inventory folder
        /// </summary>
        /// <param name="inventoryFolder">The inventory folder to save</param>
        /// <param name="path">The path to which the folder should be saved</param>
        /// <param name="saveThisFolderItself">If true, save this folder itself.  If false, only saves contents</param>
        protected void SaveInvFolder (InventoryFolderBase inventoryFolder, string path, bool saveThisFolderItself)
        {
            if (saveThisFolderItself)
            {
                path += CreateArchiveFolderName (inventoryFolder);

                // We need to make sure that we record empty folders
                m_archiveWriter.WriteDir (path);
            }

            InventoryCollection contents = FindInventoryCollection (inventoryFolder.Owner, inventoryFolder.ID);

            foreach (InventoryFolderBase childFolder in contents.Folders)
            {
                SaveInvFolder (childFolder, path, true);
            }

            foreach (InventoryItemBase item in contents.Items)
            {
                SaveInvItem (item, path);
            }
        }

        private InventoryCollection FindInventoryCollection (UUID userID, UUID folderID)
        {
            InventoryCollection collection = new InventoryCollection();
            collection.UserID = userID;
            collection.Items = new List<InventoryItemBase> ();
            if (m_items.ContainsKey (folderID))
                collection.Items = m_items[folderID];
            collection.Folders = new List<InventoryFolderBase> ();
            if (m_childFolders.ContainsKey (folderID))
                collection.Folders = m_childFolders[folderID];
            return collection;
        }

        /// <summary>
        /// Execute the inventory write request
        /// </summary>
        public void Execute (string fileName)
        {
            Stream m_saveStream = new GZipStream (new FileStream (fileName, FileMode.Create), CompressionMode.Compress);
            try
            {
                InventoryFolderBase inventoryFolder = new InventoryFolderBase (UUID.Zero, UUID.Zero);
                if (m_rootFolders.Count == 1)
                    inventoryFolder = m_rootFolders[0];

                bool saveFolderContentsOnly = false;

                m_archiveWriter = new TarArchiveWriter (m_saveStream);

                if (inventoryFolder != null)
                {
                    //recurse through all dirs getting dirs and files
                    SaveInvFolder (inventoryFolder, ArchiveConstants.INVENTORY_PATH, !saveFolderContentsOnly);
                }
            }
            catch (Exception)
            {
                m_saveStream.Close ();
                throw;
            }
            foreach (AssetBase asset in m_loadedAssets.Values)
            {
                WriteData (asset);
            }
            m_saveStream.Close ();
            MessageBox.Show ("Save complete!");
        }

        protected void WriteData (AssetBase asset)
        {
            // It appears that gtar, at least, doesn't need the intermediate directory entries in the tar
            //archive.AddDir("assets");

            string extension = string.Empty;

            if (ArchiveConstants.ASSET_TYPE_TO_EXTENSION.ContainsKey (asset.Type))
            {
                extension = ArchiveConstants.ASSET_TYPE_TO_EXTENSION[asset.Type];
            }
            else
            {
            }

            m_archiveWriter.WriteFile (
                ArchiveConstants.ASSETS_PATH + asset.IDString + extension,
                asset.Data);
        }

        /// <summary>
        /// Create the archive name for a particular folder.
        /// </summary>
        ///
        /// These names are prepended with an inventory folder's UUID so that more than one folder can have the
        /// same name
        /// 
        /// <param name="folder"></param>
        /// <returns></returns>
        public static string CreateArchiveFolderName (InventoryFolderBase folder)
        {
            return CreateArchiveFolderName (folder.Name, folder.ID);
        }

        /// <summary>
        /// Create the archive name for a particular item.
        /// </summary>
        ///
        /// These names are prepended with an inventory item's UUID so that more than one item can have the
        /// same name
        /// 
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateArchiveItemName (InventoryItemBase item)
        {
            return CreateArchiveItemName (item.Name, item.ID);
        }

        /// <summary>
        /// Create an archive folder name given its constituent components
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string CreateArchiveFolderName (string name, UUID id)
        {
            return string.Format (
                "{0}{1}{2}/",
                InventoryArchiveUtils.EscapeArchivePath (name),
                ArchiveConstants.INVENTORY_NODE_NAME_COMPONENT_SEPARATOR,
                id);
        }

        /// <summary>
        /// Create an archive item name given its constituent components
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string CreateArchiveItemName (string name, UUID id)
        {
            return string.Format (
                "{0}{1}{2}.xml",
                InventoryArchiveUtils.EscapeArchivePath (name),
                ArchiveConstants.INVENTORY_NODE_NAME_COMPONENT_SEPARATOR,
                id);
        }

        /// <summary>
        /// Create the control file for a 0.1 version archive
        /// </summary>
        /// <returns></returns>
        public static string Create0p1ControlFile ()
        {
            StringWriter sw = new StringWriter ();
            XmlTextWriter xtw = new XmlTextWriter (sw);
            xtw.Formatting = Formatting.Indented;
            xtw.WriteStartDocument ();
            xtw.WriteStartElement ("archive");
            xtw.WriteAttributeString ("major_version", "0");
            xtw.WriteAttributeString ("minor_version", "1");
            xtw.WriteEndElement ();

            xtw.Flush ();
            xtw.Close ();

            String s = sw.ToString ();
            sw.Close ();

            return s;
        }

        #endregion
    }
}
