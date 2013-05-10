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
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.IO;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using Aurora.Framework;
using Aurora.Simulation.Base;
using Aurora.Framework.Servers.HttpServer;
using Aurora.Framework.Modules;
using Aurora.Framework.SceneInfo;
using Aurora.Framework.Servers.HttpServer.Interfaces;
using Aurora.Framework.ConsoleFramework;

namespace OpenSim.Region.OptionalModules.World.WorldView
{
    public class WorldViewModule : INonSharedRegionModule
    {
        private bool m_Enabled = false;
        private IMapImageGenerator m_Generator;

        public void Initialise(IConfigSource config)
        {
            IConfig moduleConfig = config.Configs["Modules"];
            if (moduleConfig == null)
                return;

            if (moduleConfig.GetString("WorldViewModule", String.Empty) != Name)
                return;

            m_Enabled = true;
        }

        public void AddRegion (IScene scene)
        {
        }

        public void RegionLoaded (IScene scene)
        {
            if (!m_Enabled)
                return;
            m_Generator = scene.RequestModuleInterface<IMapImageGenerator>();
            if (m_Generator == null)
            {
                m_Enabled = false;
                return;
            }

            MainConsole.Instance.Info("[WORLDVIEW]: Configured and enabled");
            ISimulationBase simulationBase = scene.RequestModuleInterface<ISimulationBase>();
            if (simulationBase != null)
            {
                IHttpServer server = simulationBase.GetHttpServer(0);
                server.AddStreamHandler(new WorldViewRequestHandler(this,
                        scene.RegionInfo.RegionID.ToString()));
            }
        }

        public void RemoveRegion (IScene scene)
        {
        }

        public string Name
        {
            get { return "WorldViewModule"; }
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void Close()
        {
        }

        public byte[] GenerateWorldView(Vector3 pos, Vector3 rot, float fov,
                int width, int height, bool usetex)
        {
            if (!m_Enabled)
                return new Byte[0];

            Bitmap bmp = m_Generator.CreateViewImage(pos, rot, fov, width,
                    height, usetex);

            MemoryStream str = new MemoryStream();

            bmp.Save(str, ImageFormat.Jpeg);

            return str.ToArray();
        }
    }
}
