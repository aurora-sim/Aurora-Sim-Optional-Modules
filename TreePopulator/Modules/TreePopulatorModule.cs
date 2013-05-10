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
using System.Reflection;
using System.Timers;
using OpenMetaverse;
using Nini.Config;
using Aurora.Framework;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Aurora.Framework.SceneInfo;
using Aurora.Framework.SceneInfo.Entities;
using Aurora.Framework.Modules;
using Aurora.Framework.PresenceInfo;
using Aurora.Framework.Utilities;
using Aurora.Framework.ConsoleFramework;

namespace OpenSim.Region.OptionalModules.World.TreePopulator
{
    /// <summary>
    /// Version 2.02 - Still hacky 
    /// </summary>
    public class TreePopulatorModule : INonSharedRegionModule, IVegetationModule
    {
        private IScene m_scene;

        [XmlRootAttribute(ElementName = "Copse", IsNullable = false)]
        public class Copse
        {
            public string m_name;
            public Boolean m_frozen;
            public Tree m_tree_type;
            public int m_tree_quantity; 
            public float m_treeline_low;
            public float m_treeline_high;
            public Vector3 m_seed_point;
            public double m_range;
            public Vector3 m_initial_scale;
            public Vector3 m_maximum_scale;
            public Vector3 m_rate;

            [XmlIgnore]
            public Boolean m_planted;
            [XmlIgnore]
            public List<UUID> m_trees;

            public Copse()
            {
            }

            public Copse(string fileName, Boolean planted) 
            {
                Copse cp = (Copse)DeserializeObject(fileName);

                this.m_name = cp.m_name;
                this.m_frozen = cp.m_frozen;
                this.m_tree_quantity = cp.m_tree_quantity;
                this.m_treeline_high = cp.m_treeline_high;
                this.m_treeline_low = cp.m_treeline_low;
                this.m_range = cp.m_range;
                this.m_tree_type = cp.m_tree_type;
                this.m_seed_point = cp.m_seed_point;
                this.m_initial_scale = cp.m_initial_scale;
                this.m_maximum_scale = cp.m_maximum_scale;
                this.m_initial_scale = cp.m_initial_scale;
                this.m_rate = cp.m_rate;
                this.m_planted = planted;
                this.m_trees = new List<UUID>();
            }

            public Copse(string copsedef)
            {
                char[] delimiterChars = {':', ';'};
                string[] field = copsedef.Split(delimiterChars);

                this.m_name = field[1].Trim();
                this.m_frozen = (copsedef[0] == 'F');
                this.m_tree_quantity = int.Parse(field[2]);
                this.m_treeline_high = float.Parse(field[3], Culture.NumberFormatInfo);
                this.m_treeline_low = float.Parse(field[4], Culture.NumberFormatInfo);
                this.m_range = double.Parse(field[5], Culture.NumberFormatInfo);
                this.m_tree_type = (Tree) Enum.Parse(typeof(Tree),field[6]);
                this.m_seed_point = Vector3.Parse(field[7]);
                this.m_initial_scale = Vector3.Parse(field[8]);
                this.m_maximum_scale = Vector3.Parse(field[9]);
                this.m_rate = Vector3.Parse(field[10]);
                this.m_planted = true;
                this.m_trees = new List<UUID>();
            }

            public Copse(string name, int quantity, float high, float low, double range, Vector3 point, Tree type, Vector3 scale, Vector3 max_scale, Vector3 rate, List<UUID> trees)
            {
                this.m_name = name;
                this.m_frozen = false;
                this.m_tree_quantity = quantity;
                this.m_treeline_high = high;
                this.m_treeline_low = low;
                this.m_range = range;
                this.m_tree_type = type;
                this.m_seed_point = point;
                this.m_initial_scale = scale;
                this.m_maximum_scale = max_scale;
                this.m_rate = rate;
                this.m_planted = false;
                this.m_trees = trees;
            }

            public override string ToString()
            {
                string frozen = (this.m_frozen ? "F" : "A");

                return string.Format("{0}TPM: {1}; {2}; {3:0.0}; {4:0.0}; {5:0.0}; {6}; {7:0.0}; {8:0.0}; {9:0.0}; {10:0.00};", 
                    frozen,
                    this.m_name,
                    this.m_tree_quantity,
                    this.m_treeline_high,
                    this.m_treeline_low,
                    this.m_range,
                    this.m_tree_type,
                    this.m_seed_point.ToString(),
                    this.m_initial_scale.ToString(),
                    this.m_maximum_scale.ToString(),
                    this.m_rate.ToString());
            }
        }

        private List<Copse> m_copse;

        private double m_update_ms = 1000.0; // msec between updates 
        private bool m_active_trees = false;

        Timer CalculateTrees;

        #region IRegionModule Members

        public void Initialise(IConfigSource config)
        {
            // ini file settings
            try
            {
                m_active_trees = config.Configs["Trees"].GetBoolean("active_trees", m_active_trees);
            }
            catch (Exception)
            {
                MainConsole.Instance.Debug("[TREES]: ini failure for active_trees - using default");
            }

            try
            {
                m_update_ms = config.Configs["Trees"].GetDouble("update_rate", m_update_ms);
            }
            catch (Exception)
            {
                MainConsole.Instance.Debug("[TREES]: ini failure for update_rate - using default");
            }
        }

        public void AddRegion (IScene scene)
        {
            if (m_active_trees)
            {
                m_scene = scene;
                m_scene.RegisterModuleInterface<IVegetationModule>(this);
                m_scene.SceneGraph.RegisterEntityCreatorModule(this);

                InstallCommands();
            }
        }

        public void RemoveRegion (IScene scene)
        {
        }

        public void RegionLoaded (IScene scene)
        {
            if (m_active_trees)
            {
                ReloadCopse();
                if (m_copse.Count > 0)
                    MainConsole.Instance.Info("[TREES]: Copse load complete");

                activeizeTreeze(true);
            }
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void PostInitialise()
        {
        }

        public void Close()
        {
        }

        public string Name
        {
            get { return "TreePopulatorModule"; }
        }

        #endregion

        //--------------------------------------------------------------

        #region ICommandableModule Members

        private void HandleTreeActive(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            if (Boolean.Parse(cmd[2]) && !m_active_trees)
            {
                MainConsole.Instance.InfoFormat("[TREES]: Activating Trees");
                m_active_trees = true;
                activeizeTreeze(m_active_trees);
            }
            else if (!Boolean.Parse(cmd[2]) && m_active_trees)
            {
                MainConsole.Instance.InfoFormat("[TREES]: Trees module is no longer active");
                m_active_trees = false;
                activeizeTreeze(m_active_trees);
            }
            else
            {
                MainConsole.Instance.InfoFormat("[TREES]: Trees module is already in the required state");
            }
        }

        private void HandleTreeFreeze(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            string copsename = cmd[2].Trim();
            Boolean freezeState = Boolean.Parse(cmd[3]);

            foreach (Copse cp in m_copse)
            {
                if (cp.m_name == copsename && (!cp.m_frozen && freezeState || cp.m_frozen && !freezeState))
                {
                    cp.m_frozen = freezeState;
                    foreach (UUID tree in cp.m_trees)
                    {
                        IEntity ent;
                        if(m_scene.Entities.TryGetValue(tree, out ent) && ent is ISceneEntity)
                        {
                            ISceneChildEntity sop = ((ISceneEntity)ent).RootChild;
                            sop.Name = (freezeState ? sop.Name.Replace ("ATPM", "FTPM") : sop.Name.Replace ("FTPM", "ATPM"));
                            sop.ParentEntity.HasGroupChanged = true;
                        }
                    }

                    MainConsole.Instance.InfoFormat("[TREES]: Activity for copse {0} is frozen {1}", copsename, freezeState);
                    return;
                }
                else if (cp.m_name == copsename && (cp.m_frozen && freezeState || !cp.m_frozen && !freezeState))
                {
                    MainConsole.Instance.InfoFormat("[TREES]: Copse {0} is already in the requested freeze state", copsename);
                    return;
                }
            }
            MainConsole.Instance.InfoFormat("[TREES]: Copse {0} was not found - command failed", copsename);
        }

        private void HandleTreeLoad(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            Copse copse;

            MainConsole.Instance.InfoFormat("[TREES]: Loading copse definition....");

            copse = new Copse(cmd[2], false);
            foreach (Copse cp in m_copse)
            {
                if (cp.m_name == copse.m_name)
                {
                    MainConsole.Instance.InfoFormat("[TREES]: Copse: {0} is already defined - command failed", copse.m_name);
                    return;
                }
            }

            m_copse.Add(copse);
            MainConsole.Instance.InfoFormat("[TREES]: Loaded copse: {0}", copse.ToString());
        }

        private void HandleTreePlant(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            string copsename = cmd[0].Trim();

            MainConsole.Instance.InfoFormat("[TREES]: New tree planting for copse {0}", copsename);
            UUID uuid = m_scene.RegionInfo.EstateSettings.EstateOwner;

            foreach (Copse copse in m_copse)
            {
                if (copse.m_name == copsename)
                {
                    if (!copse.m_planted)
                    {
                        // The first tree for a copse is created here
                        CreateTree(uuid, copse, copse.m_seed_point);
                        copse.m_planted = true;
                        return;
                    }
                    else
                    {
                        MainConsole.Instance.InfoFormat("[TREES]: Copse {0} has already been planted", copsename);
                    }
                }
            }
            MainConsole.Instance.InfoFormat("[TREES]: Copse {0} not found for planting", copsename);
        }

        private void HandleTreeRate(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            m_update_ms = double.Parse(cmd[2]);
            if (m_update_ms >= 1000.0)
            {
                if (m_active_trees)
                {
                    activeizeTreeze(false);
                    activeizeTreeze(true);
                }
                MainConsole.Instance.InfoFormat("[TREES]: Update rate set to {0} mSec", m_update_ms);
            }
            else
            {
                MainConsole.Instance.InfoFormat("[TREES]: minimum rate is 1000.0 mSec - command failed");
            }
        }

        private void HandleTreeReload(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            if (m_active_trees)
            {
                CalculateTrees.Stop();
            }

            ReloadCopse();

            if (m_active_trees)
            {
                CalculateTrees.Start();
            }
        }

        private void HandleTreeRemove(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            string copsename = (cmd[2]).Trim();
            Copse copseIdentity = null;

            foreach (Copse cp in m_copse)
            {
                if (cp.m_name == copsename)
                {
                    copseIdentity = cp;
                }
            }

            if (copseIdentity != null)
            {
                List<ISceneEntity> groups = new List<ISceneEntity>();
                foreach (UUID tree in copseIdentity.m_trees)
                {
                    IEntity entity;
                    if (m_scene.Entities.TryGetValue (tree, out entity))
                    {
                        if(entity is ISceneEntity)
                            groups.Add((ISceneEntity)entity);
                    }
                }
                IBackupModule backup = m_scene.RequestModuleInterface<IBackupModule>();
                if (backup != null)
                {
                    backup.DeleteSceneObjects(groups.ToArray(), true, true);
                }
                copseIdentity.m_trees = new List<UUID>();
                m_copse.Remove(copseIdentity);
                MainConsole.Instance.InfoFormat("[TREES]: Copse {0} has been removed", copsename);
            }
            else
            {
                MainConsole.Instance.InfoFormat("[TREES]: Copse {0} was not found - command failed", copsename);
            }
        }

        private void HandleTreeStatistics(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            MainConsole.Instance.InfoFormat("[TREES]: Activity State: {0};  Update Rate: {1}", m_active_trees, m_update_ms);
            foreach (Copse cp in m_copse)
            {
                MainConsole.Instance.InfoFormat("[TREES]: Copse {0}; {1} trees; frozen {2}", cp.m_name, cp.m_trees.Count, cp.m_frozen);
            }
        }

        private void HandleTreeHelp(string[] cmd)
        {
            if (MainConsole.Instance.ConsoleScene != m_scene)
                return;
            MainConsole.Instance.Info("tree active <activeTF> - Change activity state for the trees module. "
                               + "\n activeTF: The required activity state");
            MainConsole.Instance.Info("tree freeze <copse> <freezeTF> - Freeze/Unfreeze activity for a defined copse. "
                               + "\n copse: The required copse"
                               + "\n freezeTF: The required freeze state");
            MainConsole.Instance.Info("tree load <filename> - Load a copse definition from an xml file. "
                               + "\n filename: The (xml) file you wish to load");
            MainConsole.Instance.Info("tree plant <copse> - Start the planting on a copse. "
                               + "\n copse: The required copse");
            MainConsole.Instance.Info("tree rate <updateRate> - Reset the tree update rate (mSec). "
                               + "\n updateRate: The required update rate (minimum 1000.0)");
            MainConsole.Instance.Info("tree reload - Reload copse definitions from the in-scene trees.");
            MainConsole.Instance.Info("tree remove <copse> - Remove a copse definition and all its in-scene trees. "
                               + "\n copse: The required copse");
            MainConsole.Instance.Info("tree statistics - Log statistics about the trees.");
        }

        private void InstallCommands()
        {
            if (MainConsole.Instance != null)
            {
                MainConsole.Instance.Commands.AddCommand ("tree active",
                "tree active <activeTF>", "Change activity state for the trees module. "
                               + "\n activeTF: The required activity state", HandleTreeActive);
                MainConsole.Instance.Commands.AddCommand ("tree freeze <",
                    "tree freeze <copse> <freezeTF>", "Freeze/Unfreeze activity for a defined copse. "
                                   + "\n copse: The required copse"
                                   + "\n freezeTF: The required freeze state", HandleTreeFreeze);
                MainConsole.Instance.Commands.AddCommand ("tree load",
                    "tree load <filename>", "Load a copse definition from an xml file. "
                                   + "\n filename: The (xml) file you wish to load", HandleTreeLoad);
                MainConsole.Instance.Commands.AddCommand ("tree plant",
                    "tree plant <copse>", "Start the planting on a copse. "
                                   + "\n copse: The required copse", HandleTreePlant);
                MainConsole.Instance.Commands.AddCommand ("tree rate",
                    "tree rate <updateRate>", "Reset the tree update rate (mSec). "
                                   + "\n updateRate: The required update rate (minimum 1000.0)", HandleTreeRate);
                MainConsole.Instance.Commands.AddCommand ("tree reload",
                    "tree reload", "Reload copse definitions from the in-scene trees.", HandleTreeReload);

                MainConsole.Instance.Commands.AddCommand ("tree remove",
                    "tree remove <copse>", "Remove a copse definition and all its in-scene trees. "
                                   + "\n copse: The required copse", HandleTreeRemove);
                MainConsole.Instance.Commands.AddCommand ("tree statistics",
                    "tree statistics", "Log statistics about the trees.", HandleTreeStatistics);
                MainConsole.Instance.Commands.AddCommand ("tree help",
                    "tree help", "Help about the trees.", HandleTreeHelp);
            }
        }

        #endregion

        #region IVegetationModule Members

        public ISceneEntity AddTree (
            UUID uuid, UUID groupID, Vector3 scale, Quaternion rotation, Vector3 position, Tree treeType, bool newTree)
        {
            PrimitiveBaseShape treeShape = new PrimitiveBaseShape();
            treeShape.PathCurve = 16;
            treeShape.PathEnd = 49900;
            treeShape.PCode = newTree ? (byte)PCode.NewTree : (byte)PCode.Tree;
            treeShape.Scale = scale;
            treeShape.State = (byte)treeType;

            return m_scene.SceneGraph.AddNewPrim(uuid, groupID, position, rotation, treeShape);
        }

        #endregion

        #region IEntityCreator Members

        protected static readonly PCode[] creationCapabilities = new PCode[] { PCode.NewTree, PCode.Tree };
        public PCode[] CreationCapabilities { get { return creationCapabilities; } }

        public ISceneEntity CreateEntity(
            ISceneEntity sceneObject, UUID ownerID, UUID groupID, Vector3 pos, Quaternion rot, PrimitiveBaseShape shape)
        {
            if (Array.IndexOf(creationCapabilities, (PCode)shape.PCode) < 0)
            {
                MainConsole.Instance.DebugFormat("[VEGETATION]: PCode {0} not handled by {1}", shape.PCode, Name);
                return null;
            }

            ISceneChildEntity rootPart = sceneObject.GetChildPart(sceneObject.UUID);

            rootPart.AddFlag(PrimFlags.Phantom);
            if (rootPart.Shape.PCode != (byte)PCode.Grass)
            {
                // Tree size has to be adapted depending on its type
                switch ((Tree)rootPart.Shape.State)
                {
                    case Tree.Cypress1:
                    case Tree.Cypress2:
                    case Tree.Palm1:
                    case Tree.Palm2:
                    case Tree.WinterAspen:
                        rootPart.Scale = new Vector3(4, 4, 10);
                        break;
                    case Tree.WinterPine1:
                    case Tree.WinterPine2:
                        rootPart.Scale = new Vector3(4, 4, 20);
                        break;

                    case Tree.Dogwood:
                        rootPart.Scale = new Vector3(6.5f, 6.5f, 6.5f);
                        break;

                    // case... other tree types
                    // tree.Scale = new Vector3(?, ?, ?);
                    // break;

                    default:
                        rootPart.Scale = new Vector3(4, 4, 4);
                        break;
                }
            }

            sceneObject.SetGroup(groupID, UUID.Zero, false);
            m_scene.SceneGraph.AddPrimToScene(sceneObject);
            sceneObject.ScheduleGroupUpdate(PrimUpdateFlags.ForcedFullUpdate);

            return sceneObject;
        }

        #endregion

        //--------------------------------------------------------------

        #region Tree Utilities
        static public void SerializeObject(string fileName, Object obj)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Copse));

                using (XmlTextWriter writer = new XmlTextWriter(fileName, Util.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    xs.Serialize(writer, obj);
                }
            }
            catch (SystemException ex)
            {
                throw new ApplicationException("Unexpected failure in Tree serialization", ex);
            }
        }

        static public object DeserializeObject(string fileName)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Copse));

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    return xs.Deserialize(fs);
            }
            catch (SystemException ex)
            {
                throw new ApplicationException("Unexpected failure in Tree de-serialization", ex);
            }
        }

        private void ReloadCopse()
        {
            m_copse = new List<Copse>();

            ISceneEntity[] objs = m_scene.Entities.GetEntities ();
            foreach(ISceneEntity grp in objs)
            {
                if(grp.Name.Length > 5 && (grp.Name.Substring(0, 5) == "ATPM:" || grp.Name.Substring(0, 5) == "FTPM:"))
                {
                    // Create a new copse definition or add uuid to an existing definition
                    try
                    {
                        Boolean copsefound = false;
                        Copse copse = new Copse(grp.Name);

                        foreach(Copse cp in m_copse)
                        {
                            if(cp.m_name == copse.m_name)
                            {
                                copsefound = true;
                                cp.m_trees.Add(grp.UUID);
                                //MainConsole.Instance.DebugFormat("[TREES]: Found tree {0}", grp.UUID);
                            }
                        }

                        if(!copsefound)
                        {
                            MainConsole.Instance.InfoFormat("[TREES]: Found copse {0}", grp.Name);
                            m_copse.Add(copse);
                            copse.m_trees.Add(grp.UUID);
                        }
                    }
                    catch
                    {
                        MainConsole.Instance.InfoFormat("[TREES]: Ill formed copse definition {0} - ignoring", grp.Name);
                    }
                }
            }
        }
        #endregion

        private void activeizeTreeze(bool activeYN)
        {
            if (activeYN)
            {
                CalculateTrees = new Timer(m_update_ms);
                CalculateTrees.Elapsed += CalculateTrees_Elapsed;
                CalculateTrees.Start();
            }
            else 
            {
                 CalculateTrees.Stop();
            }
        } 

        private void growTrees()
        {
            foreach (Copse copse in m_copse)
            {
                if (!copse.m_frozen)
                {
                    foreach (UUID tree in copse.m_trees)
                    {
                        IEntity ent;
                        if (m_scene.Entities.TryGetValue(tree, out ent))
                        {
                            ISceneChildEntity s_tree = ((ISceneEntity)ent).RootChild;

                            if (s_tree.Scale.X < copse.m_maximum_scale.X && s_tree.Scale.Y < copse.m_maximum_scale.Y && s_tree.Scale.Z < copse.m_maximum_scale.Z)
                            {
                                s_tree.Scale += copse.m_rate;
                                s_tree.ParentEntity.HasGroupChanged = true;
                                s_tree.ScheduleUpdate(PrimUpdateFlags.FindBest);
                            }
                        }
                        else
                        {
                            MainConsole.Instance.DebugFormat("[TREES]: Tree not in scene {0}", tree);
                        }
                    }
                }
            }
        }

        private void seedTrees()
        {
            foreach (Copse copse in m_copse)
            {
                if (!copse.m_frozen)
                {
                    foreach (UUID tree in copse.m_trees)
                    {
                        IEntity entity;
                        if(m_scene.Entities.TryGetValue(tree, out entity) && entity is ISceneEntity)
                        {
                            ISceneChildEntity s_tree = ((ISceneEntity)entity).RootChild;

                            if (copse.m_trees.Count < copse.m_tree_quantity)
                            {
                                // Tree has grown enough to seed if it has grown by at least 25% of seeded to full grown height
                                if (s_tree.Scale.Z > copse.m_initial_scale.Z + (copse.m_maximum_scale.Z - copse.m_initial_scale.Z) / 4.0) 
                                {
                                    if (Util.RandomClass.NextDouble() > 0.75)
                                    {
                                        SpawnChild(copse, s_tree);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MainConsole.Instance.DebugFormat("[TREES]: Tree not in scene {0}", tree);
                        }
                    }
                }
            }
        }

        private void killTrees()
        {
            foreach (Copse copse in m_copse)
            {
                if (!copse.m_frozen && copse.m_trees.Count >= copse.m_tree_quantity)
                {
                    List<ISceneEntity> groups = new List<ISceneEntity>();
                    foreach (UUID tree in copse.m_trees)
                    {
                        double killLikelyhood = 0.0;

                        IEntity entity;
                        if(m_scene.Entities.TryGetValue(tree, out entity) && entity is ISceneEntity)
                        {
                            ISceneChildEntity selectedTree = ((ISceneEntity)entity).RootChild;
                            double selectedTreeScale = Math.Sqrt(Math.Pow(selectedTree.Scale.X, 2) +
                                                                 Math.Pow(selectedTree.Scale.Y, 2) +
                                                                 Math.Pow(selectedTree.Scale.Z, 2));

                            foreach (UUID picktree in copse.m_trees)
                            {
                                if (picktree != tree)
                                {
                                    IEntity ent;
                                    if(m_scene.Entities.TryGetValue(tree, out ent) && ent is ISceneEntity)
                                    {
                                        ISceneChildEntity pickedTree = ((ISceneEntity)ent).RootChild;

                                        double pickedTreeScale = Math.Sqrt (Math.Pow (pickedTree.Scale.X, 2) +
                                                                           Math.Pow (pickedTree.Scale.Y, 2) +
                                                                           Math.Pow (pickedTree.Scale.Z, 2));

                                        double pickedTreeDistance = Vector3.Distance (pickedTree.AbsolutePosition, selectedTree.AbsolutePosition);

                                        killLikelyhood += (selectedTreeScale / (pickedTreeScale * pickedTreeDistance)) * 0.1;
                                    }
                                }
                            }

                            if (Util.RandomClass.NextDouble() < killLikelyhood)
                            {
                                groups.Add(selectedTree.ParentEntity);
                                copse.m_trees.Remove(selectedTree.ParentEntity.UUID);

                                break;
                            }
                        }
                        else
                        {
                            MainConsole.Instance.DebugFormat("[TREES]: Tree not in scene {0}", tree);
                        }
                    }
                    IBackupModule backup = m_scene.RequestModuleInterface<IBackupModule>();
                    if (backup != null)
                    {
                        backup.DeleteSceneObjects(groups.ToArray(), true, true);
                    }
                }
            }
        }

        private void SpawnChild (Copse copse, ISceneChildEntity s_tree)
        {
            Vector3 position = new Vector3();

            double randX = ((Util.RandomClass.NextDouble() * 2.0) - 1.0) * (s_tree.Scale.X * 3);
            double randY = ((Util.RandomClass.NextDouble() * 2.0) - 1.0) * (s_tree.Scale.X * 3);

            position.X = s_tree.AbsolutePosition.X + (float)randX;
            position.Y = s_tree.AbsolutePosition.Y + (float)randY;

            if (!(position.X < 0f || position.Y < 0f ||
                position.X > m_scene.RegionInfo.RegionSizeX || position.Y > m_scene.RegionInfo.RegionSizeY) &&
                Util.GetDistanceTo(position, copse.m_seed_point) <= copse.m_range)
            {
                UUID uuid = m_scene.RegionInfo.EstateSettings.EstateOwner;

                CreateTree(uuid, copse, position);
            }
        }

        private void CreateTree(UUID uuid, Copse copse, Vector3 position)
        {

            position.Z = m_scene.RequestModuleInterface<ITerrainChannel>()[(int)position.X, (int)position.Y];
            if (position.Z >= copse.m_treeline_low && position.Z <= copse.m_treeline_high)
            {
                ISceneEntity tree = AddTree(uuid, UUID.Zero, copse.m_initial_scale, Quaternion.Identity, position, copse.m_tree_type, false);

                tree.Name = copse.ToString();
                copse.m_trees.Add(tree.UUID);
                tree.ScheduleGroupUpdate(PrimUpdateFlags.FindBest);
            }
        }

        private void CalculateTrees_Elapsed(object sender, ElapsedEventArgs e)
        {
            growTrees();
            seedTrees();
            killTrees();
        }
    }
}

