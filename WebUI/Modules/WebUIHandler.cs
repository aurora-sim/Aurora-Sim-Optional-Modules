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
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Timers;

using BitmapProcessing;

using Nini.Config;

using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.StructuredData;
using EventFlags = OpenMetaverse.DirectoryManager.EventFlags;

using OpenSim.Services.Interfaces;
using FriendInfo = OpenSim.Services.Interfaces.FriendInfo;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;

using Aurora.Framework;
using Aurora.Framework.Servers.HttpServer;
using Aurora.DataManager;
using Aurora.Services.DataService;
using Aurora.Simulation.Base;
using RegionFlags = Aurora.Framework.RegionFlags;

namespace Aurora.Services
{
    public class WebUIConnector : IAuroraDataPlugin
    {
        private bool m_enabled = false;
        public bool Enabled
        {
            get { return m_enabled; }
        }

        private string m_Handler = string.Empty;
        public string Handler
        {
            get
            {
                return m_Handler;
            }
        }

        private string m_HandlerPassword = string.Empty;
        public string HandlerPassword
        {
            get
            {
                return m_HandlerPassword;
            }
        }

        private uint m_HandlerPort = 0;
        public uint HandlerPort
        {
            get
            {
                return m_HandlerPort;
            }
        }

        private uint m_TexturePort = 0;
        public uint TexturePort
        {
            get
            {
                return m_TexturePort;
            }
        }

        private IGenericData GD;
        private string ConnectionString = "";
        private Timer m_GC_timer;
        private uint m_codes_GC = 24;

        #region console wrappers

        private void Info(object message)
        {
            MainConsole.Instance.Info("[" + Name + "]: " + message.ToString());
        }

        private void Warn(object message)
        {
            MainConsole.Instance.Warn("[" + Name + "]: " + message.ToString());
        }

        #endregion

        #region IAuroraDataPlugin Members

        public string Name
        {
            get
            {
                return "WebUIConnector";
            }
        }

        private void handleConfig(IConfigSource m_config)
        {
            IConfig config = m_config.Configs["Handlers"];
            if (config == null)
            {
                m_enabled = false;
                Warn("not loaded, no configuration found.");
                return;
            }

            m_Handler = config.GetString("WebUIHandler", string.Empty);
            m_HandlerPassword = config.GetString("WebUIHandlerPassword", string.Empty);
            m_HandlerPort = config.GetUInt("WebUIHandlerPort", 0);
            m_TexturePort = config.GetUInt("WebUIHandlerTextureServerPort", 0);
            m_codes_GC = config.GetUInt("WebUIHandlerGC_codes", 24);
            if (m_codes_GC < 1)
            {
                m_codes_GC = 1;
            }

            if (Handler == string.Empty || HandlerPassword == string.Empty || HandlerPort == 0 || TexturePort == 0)
            {
                m_enabled = false;
                Warn("Not loaded, configuration missing.");
                return;
            }

            IConfig dbConfig = m_config.Configs["DatabaseService"];
            if (dbConfig != null)
            {
                ConnectionString = dbConfig.GetString("ConnectionString", String.Empty);
            }

            if (ConnectionString == string.Empty)
            {
                m_enabled = false;
                Warn("not loaded, no storage parameters found");
                return;
            }

            m_enabled = true;
        }

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore simBase, string DefaultConnectionString)
        {
            handleConfig(source);
            if (!Enabled)
            {
                Warn("not loaded, disabled in config.");
                return;
            }
            DataManager.DataManager.RegisterPlugin(this);

            GD = GenericData;
            GD.ConnectToDatabase(ConnectionString, "Wiredux", true);

            m_GC_timer = new Timer(m_codes_GC * 3600000);
            m_GC_timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) => {
                garbageCollection();
            });
            m_GC_timer.Enabled = true;
            garbageCollection();
        }

        #endregion

        public OSDMap WebUIClientImplementationData()
        {
            OSDMap resp = new OSDMap(3);
            List<string> result;
            string[] keys;

            keys = new string[7]{
                "id",
                "lastnames",
                "adress",
                "region",
                "allowRegistrations",
                "verifyUsers",
                "ForceAge"
            };
            result = GD.Query(keys, "wi_adminsetting", null, null, 0, 1);
            if (result.Count == keys.Length)
            {
                OSDMap adminsetting = new OSDMap();
                for (int i = 0; i < result.Count; ++i)
                {
                    uint val;
                    if (!uint.TryParse(result[i], out val))
                    {
                        val = (uint)((result[i] == "True") ? 1 : 0);
                    }
                    adminsetting[keys.GetValue(i).ToString()] = OSD.FromInteger(val);
                }
                resp["adminsetting"] = adminsetting;
            }

            keys = new string[20]{
                "id",
                "displayTopPanelSlider", 
                "displayTemplateSelector",
                "displayStyleSwitcher",
                "displayStyleSizer",
                "displayFontSizer",
                "displayLanguageSelector",
                "displayScrollingText",
                "displayWelcomeMessage",
                "displayLogo",
                "displayLogoEffect",
                "displaySlideShow",
                "displayMegaMenu",
                "displayDate",
                "displayTime",
                "displayRoundedCorner",
                "displayBackgroundColorAnimation",
                "displayPageLoadTime",
                "displayW3c",
                "displayRss"
            };
            result = GD.Query(keys, "wi_adminmodules", null, null, 0, 1);
            if (result.Count == keys.Length)
            {
                OSDMap adminmodules = new OSDMap();
                for (int i = 0; i < result.Count; ++i)
                {
                    adminmodules[keys.GetValue(i).ToString()] = OSD.FromBoolean(uint.Parse(result[i]) == 1);
                }
                resp["adminmodules"] = adminmodules;
            }


            UUID uuid;
            result = GD.Query(new string[1] { "startregion" }, "wi_adminsetting", null, null, 0, 1);
            resp["startregion"] = OSD.FromUUID(result[0].Trim() != string.Empty && UUID.TryParse(result[0], out uuid) ? uuid : UUID.Zero);

            return resp;
        }

        private void garbageCollection()
        {
            int now = (int)Utils.DateTimeToUnixTime(DateTime.Now);

            QueryFilter filter = new QueryFilter();

            filter.andLessThanFilters["time"] = now - (int)(m_codes_GC * 3600);
            filter.andFilters["info"] = "pwreset";
            filter.orMultiFilters["info"] = new List<object>
            {
                "pwreset",
                "confirm",
                "emailconfirm"
            };

            GD.Delete("wi_codetable", filter);
        }
    }

    public class WebUIHandler : IService
    {
        private WebUIConnector m_connector;

        public IHttpServer m_server = null;
        public IHttpServer m_server2 = null;
        string m_servernick = "hippogrid";
        protected IRegistryCore m_registry;

        protected UUID AdminAgentID = UUID.Zero;

        public string Name
        {
            get { return GetType().Name; }
        }

        #region IService

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            m_connector = DataManager.DataManager.RequestPlugin<WebUIConnector>();
            if (m_connector == null || m_connector.Enabled == false || m_connector.Handler != Name)
            {
                return;
            }

            IConfig handlerConfig = config.Configs["Handlers"];
            string name = handlerConfig.GetString(Name, "");
            string Password = handlerConfig.GetString(Name + "Password", String.Empty);
            bool runLocally = handlerConfig.GetBoolean("RunLocally", false);
            uint httpPort = handlerConfig.GetUInt("WebUIHTTPPort", 80);
            string phpBinPath = handlerConfig.GetString("phpBinPath", string.Empty);

            if (name != Name || (!runLocally && Password == string.Empty) || (runLocally && phpBinPath == string.Empty))
            {
                MainConsole.Instance.Warn("[WebUI] module not loaded");
                return;
            }
            MainConsole.Instance.Info("[WebUI] module loaded");

            m_registry = registry;

            IConfig GridInfoConfig = config.Configs["GridInfoService"];
            if (GridInfoConfig != null)
            {
                m_servernick = GridInfoConfig.GetString("gridnick", m_servernick);
            }

            if (runLocally)
            {
                SetUpWebUIPHP(httpPort, phpBinPath);
            }

            OSDMap gridInfo = new OSDMap();
            if (GridInfoConfig != null && (GridInfoConfig.GetString("gridname", "") != "" && GridInfoConfig.GetString("gridnick", "") != ""))
            {
                foreach (string k in GridInfoConfig.GetKeys())
                {
                    gridInfo[k] = GridInfoConfig.GetString(k);
                }
            }

            ISimulationBase simBase = registry.RequestModuleInterface<ISimulationBase>();

            m_server = simBase.GetHttpServer(handlerConfig.GetUInt(Name + "Port", m_connector.HandlerPort));
            //This handler allows sims to post CAPS for their sims on the CAPS server.
            m_server.AddStreamHandler(new WebUIHTTPHandler(this, m_connector.HandlerPassword, registry, gridInfo, AdminAgentID, runLocally, httpPort));
            m_server2 = simBase.GetHttpServer(handlerConfig.GetUInt(Name + "TextureServerPort", m_connector.TexturePort));
            m_server2.AddHTTPHandler("GridTexture", OnHTTPGetTextureImage);
            m_server2.AddHTTPHandler("MapTexture", OnHTTPGetMapImage);
            gridInfo[Name + "TextureServer"] = m_server2.ServerURI;

            MainConsole.Instance.Commands.AddCommand("webui promote user", "Grants the specified user administrative powers within webui.", "webui promote user", PromoteUser);
            MainConsole.Instance.Commands.AddCommand("webui demote user", "Revokes administrative powers for webui from the specified user.", "webui demote user", DemoteUser);
            MainConsole.Instance.Commands.AddCommand("webui add group as news source", "Sets a group as a news source so in-world group notices can be used as a publishing tool for the website.", "webui add group as news source", AddGroupAsNewsSource);
            MainConsole.Instance.Commands.AddCommand("webui remove group as news source", "Removes a group as a news source so it's notices will stop showing up on the news page.", "webui remove group as news source", RemoveGroupAsNewsSource);
        }

        private void SetUpWebUIPHP(uint port, string phpBinPath)
        {
            HttpServer.HttpModules.AdvancedFileModule.CreateHTTPServer(Util.BasePathCombine("data//WebUI//"), "/", @phpBinPath, port, false);
        }

        public void FinishedStartup()
        {
        }

        #endregion

        #region textures

        public Hashtable OnHTTPGetTextureImage(Hashtable keysvals)
        {
            Hashtable reply = new Hashtable();

            if (keysvals["method"].ToString() != "GridTexture")
                return reply;

            MainConsole.Instance.Debug("[WebUI]: Sending image jpeg");
            int statuscode = 200;
            byte[] jpeg = new byte[0];
            IAssetService m_AssetService = m_registry.RequestModuleInterface<IAssetService>();

            MemoryStream imgstream = new MemoryStream();
            Bitmap mapTexture = new Bitmap(1, 1);
            ManagedImage managedImage;
            Image image = (Image)mapTexture;

            try
            {
                // Taking our jpeg2000 data, decoding it, then saving it to a byte array with regular jpeg data

                imgstream = new MemoryStream();

                // non-async because we know we have the asset immediately.
                AssetBase mapasset = m_AssetService.Get(keysvals["uuid"].ToString());

                // Decode image to System.Drawing.Image
                if (OpenJPEG.DecodeToImage(mapasset.Data, out managedImage, out image))
                {
                    // Save to bitmap

                    mapTexture = ResizeBitmap(image, 128, 128);
                    EncoderParameters myEncoderParameters = new EncoderParameters();
                    myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);

                    // Save bitmap to stream
                    mapTexture.Save(imgstream, GetEncoderInfo("image/jpeg"), myEncoderParameters);



                    // Write the stream to a byte array for output
                    jpeg = imgstream.ToArray();
                }
            }
            catch (Exception)
            {
                // Dummy!
                MainConsole.Instance.Warn("[WebUI]: Unable to post image.");
            }
            finally
            {
                // Reclaim memory, these are unmanaged resources
                // If we encountered an exception, one or more of these will be null
                if (mapTexture != null)
                    mapTexture.Dispose();

                if (image != null)
                    image.Dispose();

                if (imgstream != null)
                {
                    imgstream.Close();
                    imgstream.Dispose();
                }
            }


            reply["str_response_string"] = Convert.ToBase64String(jpeg);
            reply["int_response_code"] = statuscode;
            reply["content_type"] = "image/jpeg";

            return reply;
        }

        public Hashtable OnHTTPGetMapImage(Hashtable keysvals)
        {
            Hashtable reply = new Hashtable();

            if (keysvals["method"].ToString() != "MapTexture")
                return reply;

            int zoom = (keysvals.ContainsKey("zoom")) ? int.Parse(keysvals["zoom"].ToString()) : 20;
            int x = (keysvals.ContainsKey("x")) ? (int)float.Parse(keysvals["x"].ToString()) : 0;
            int y = (keysvals.ContainsKey("y")) ? (int)float.Parse(keysvals["y"].ToString()) : 0;

            MainConsole.Instance.Debug("[WebUI]: Sending map image jpeg");
            int statuscode = 200;
            byte[] jpeg = new byte[0];
            
            MemoryStream imgstream = new MemoryStream();
            Bitmap mapTexture = CreateZoomLevel(zoom, x, y);
            EncoderParameters myEncoderParameters = new EncoderParameters();
            myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L);

            // Save bitmap to stream
            mapTexture.Save(imgstream, GetEncoderInfo("image/jpeg"), myEncoderParameters);

            // Write the stream to a byte array for output
            jpeg = imgstream.ToArray();

            // Reclaim memory, these are unmanaged resources
            // If we encountered an exception, one or more of these will be null
            if (mapTexture != null)
            {
                mapTexture.Dispose();
            }

            if (imgstream != null)
            {
                imgstream.Close();
                imgstream.Dispose();
            }

            reply["str_response_string"] = Convert.ToBase64String(jpeg);
            reply["int_response_code"] = statuscode;
            reply["content_type"] = "image/jpeg";

            return reply;
        }

        public Bitmap ResizeBitmap(Image b, int nWidth, int nHeight)
        {
            Bitmap newsize = new Bitmap(nWidth, nHeight);
            Graphics temp = Graphics.FromImage(newsize);
            temp.DrawImage(b, 0, 0, nWidth, nHeight);
            temp.SmoothingMode = SmoothingMode.AntiAlias;
            temp.DrawString(m_servernick, new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.FromArgb(90, 255, 255, 50)), new Point(2, 115));

            return newsize;
        }

        private Bitmap CreateZoomLevel(int zoomLevel, int centerX, int centerY)
        {
            if (!Directory.Exists("MapTiles"))
                Directory.CreateDirectory("MapTiles");

            string fileName = Path.Combine("MapTiles", "Zoom" + zoomLevel + "X" + centerX + "Y" + centerY + ".jpg");
            if (File.Exists(fileName))
            {
                DateTime lastWritten = File.GetLastWriteTime(fileName);
                if ((DateTime.Now - lastWritten).Minutes < 10) //10 min cache
                    return (Bitmap)Bitmap.FromFile(fileName);
            }

            List<GridRegion> regions = m_registry.RequestModuleInterface<IGridService>().GetRegionRange(UUID.Zero,
                    (int)(centerX * (int)Constants.RegionSize - (zoomLevel * (int)Constants.RegionSize)),
                    (int)(centerX * (int)Constants.RegionSize + (zoomLevel * (int)Constants.RegionSize)),
                    (int)(centerY * (int)Constants.RegionSize - (zoomLevel * (int)Constants.RegionSize)),
                    (int)(centerY * (int)Constants.RegionSize + (zoomLevel * (int)Constants.RegionSize)));
            List<Image> bitImages = new List<Image>();
            List<FastBitmap> fastbitImages = new List<FastBitmap>();

            foreach (GridRegion r in regions)
            {
                AssetBase texAsset = m_registry.RequestModuleInterface<IAssetService>().Get(r.TerrainImage.ToString());

                if (texAsset != null)
                {
                    ManagedImage managedImage;
                    Image image;
                    if (OpenJPEG.DecodeToImage(texAsset.Data, out managedImage, out image))
                    {
                        bitImages.Add(image);
                        fastbitImages.Add(new FastBitmap((Bitmap)image));
                    }
                }
            }

            int imageSize = 2560;
            float zoomScale = (imageSize / zoomLevel);
            Bitmap mapTexture = new Bitmap(imageSize, imageSize);
            Graphics g = Graphics.FromImage(mapTexture);
            Color seaColor = Color.FromArgb(29, 71, 95);
            SolidBrush sea = new SolidBrush(seaColor);
            g.FillRectangle(sea, 0, 0, imageSize, imageSize);

            for (int i = 0; i < regions.Count; i++)
            {
                float x = ((regions[i].RegionLocX - (centerX * (float)Constants.RegionSize) + Constants.RegionSize / 2) / (float)Constants.RegionSize);
                float y = ((regions[i].RegionLocY - (centerY * (float)Constants.RegionSize) + Constants.RegionSize / 2) / (float)Constants.RegionSize);

                int regionWidth = regions[i].RegionSizeX / Constants.RegionSize;
                int regionHeight = regions[i].RegionSizeY / Constants.RegionSize;
                float posX = (x * zoomScale) + imageSize / 2;
                float posY = (y * zoomScale) + imageSize / 2;
                g.DrawImage(bitImages[i], posX, imageSize - posY, zoomScale * regionWidth, zoomScale * regionHeight); // y origin is top
            }

            mapTexture.Save(fileName, ImageFormat.Jpeg);

            return mapTexture;
        }

        // From msdn
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (int j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        #endregion

        #region Console Commands

        #region WebUI Admin

        private void PromoteUser (string[] cmd)
        {
            string name = MainConsole.Instance.Prompt ("Name of user");
            UserAccount acc = m_registry.RequestModuleInterface<IUserAccountService> ().GetUserAccount (UUID.Zero, name);
            if (acc == null)
            {
                MainConsole.Instance.Warn ("You must create the user before promoting them.");
                return;
            }
            IAgentConnector agents = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
            if (agents == null)
            {
                MainConsole.Instance.Warn("Could not get IAgentConnector plugin");
                return;
            }
            IAgentInfo agent = agents.GetAgent(acc.PrincipalID);
            if (agent == null)
            {
                MainConsole.Instance.Warn("Could not get IAgentInfo for " + name + ", try logging the user into your grid first.");
                return;
            }
            agent.OtherAgentInformation["WebUIEnabled"] = true;
            Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector> ().UpdateAgent (agent);
            MainConsole.Instance.Warn ("Admin added");
        }

        private void DemoteUser (string[] cmd)
        {
            string name = MainConsole.Instance.Prompt ("Name of user");
            UserAccount acc = m_registry.RequestModuleInterface<IUserAccountService> ().GetUserAccount (UUID.Zero, name);
            if (acc == null)
            {
                MainConsole.Instance.Warn ("User does not exist, no action taken.");
                return;
            }
            IAgentConnector agents = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
            if (agents == null)
            {
                MainConsole.Instance.Warn("Could not get IAgentConnector plugin");
                return;
            }
            IAgentInfo agent = agents.GetAgent(acc.PrincipalID);
            if (agent == null)
            {
                MainConsole.Instance.Warn("Could not get IAgentInfo for " + name + ", try logging the user into your grid first.");
                return;
            }
            agent.OtherAgentInformation["WebUIEnabled"] = false;
            Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector> ().UpdateAgent (agent);
            MainConsole.Instance.Warn ("Admin removed");
        }

        #endregion

        #region Groups

        private void AddGroupAsNewsSource(string[] cmd)
        {
            string name = MainConsole.Instance.Prompt("Name of group");
            GroupRecord group = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>().GetGroupRecord(AdminAgentID, UUID.Zero, name);
            if (group == null)
            {
                MainConsole.Instance.Warn("[WebUI] You must create the group before adding it as a news source");
                return;
            }
            IGenericsConnector generics = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            OSDMap useValue = new OSDMap();
            useValue["Use"] = OSD.FromBoolean(true);
            generics.AddGeneric(group.GroupID, "Group", "WebUI_newsSource", useValue);
            MainConsole.Instance.Warn(string.Format("[WebUI]: \"{0}\" was added as a news source", group.GroupName));
        }

        private void RemoveGroupAsNewsSource(string[] cmd)
        {
            string name = MainConsole.Instance.Prompt("Name of group");
            GroupRecord group = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>().GetGroupRecord(AdminAgentID, UUID.Zero, name);
            if (group == null)
            {
                MainConsole.Instance.Warn(string.Format("[WebUI] \"{0}\" did not appear to be a Group, cannot remove as news source", name));
                return;
            }
            IGenericsConnector generics = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            generics.RemoveGeneric(group.GroupID, "Group", "WebUI_newsSource");
            MainConsole.Instance.Warn(string.Format("[WebUI]: \"{0}\" was removed as a news source", group.GroupName));
        }

        #endregion

        #endregion

        public OSDMap WebUIClientImplementationData()
        {
            return m_connector.WebUIClientImplementationData();
        }
    }

    public class WebUIHTTPHandler : BaseStreamHandler
    {
        protected WebUIHandler WebUI;
        protected string m_password;
        protected IRegistryCore m_registry;
        protected OSDMap GridInfo;
        private UUID AdminAgentID;
        private Dictionary<string, MethodInfo> APIMethods = new Dictionary<string, MethodInfo>();
        private bool m_runLocal = true;
        private uint m_localPort;

        public WebUIHTTPHandler(WebUIHandler webui, string pass, IRegistryCore reg, OSDMap gridInfo, UUID adminAgentID, bool runLocally, uint port)
            : base("POST", "/WEBUI")
        {
            WebUI = webui;
            m_registry = reg;
            m_password = Util.Md5Hash(pass);
            GridInfo = gridInfo;
            AdminAgentID = adminAgentID;
            m_runLocal = runLocally;
            m_localPort = port;
            MethodInfo[] methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (uint i = 0; i < methods.Length; ++i)
            {
                if (methods[i].IsPrivate && methods[i].ReturnType == typeof(OSDMap) && methods[i].GetParameters().Length == 1 && methods[i].GetParameters()[0].ParameterType == typeof(OSDMap))
                {
                    APIMethods[methods[i].Name] = methods[i];
                }
            }
        }

        #region BaseStreamHandler

        public override byte[] Handle(string path, Stream requestData, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            StreamReader sr = new StreamReader(requestData);
            string body = sr.ReadToEnd();
            sr.Close();
            body = body.Trim();

            MainConsole.Instance.TraceFormat("[WebUI]: query String: {0}", body);
            string method = string.Empty;
            OSDMap resp = new OSDMap();
            try
            {
                OSDMap map = (OSDMap)OSDParser.DeserializeJson(body);
                //Make sure that the person who is calling can access the web service
                if (ValidateUser(httpRequest, map))
                {
                    method = map["Method"].AsString();
                    if (method == "Login" || method == "AdminLogin")
                    {
                        resp = Login(map, method == "AdminLogin");
                    }
                    else if (APIMethods.ContainsKey(method))
                    {
                        object[] args = new object[1]{map};
                        resp = (OSDMap)APIMethods[method].Invoke(this, args);
                    }
                    else
                    {
                        MainConsole.Instance.TraceFormat("[WebUI] Unsupported method called ({0})", method);
                    }
                }
                else
                {
                    MainConsole.Instance.Debug("Password does not match");
                }
            }
            catch (Exception e)
            {
                MainConsole.Instance.TraceFormat("[WebUI] Exception thrown: " + e.ToString());
            }
            if(resp.Count == 0){
                resp.Add("response", OSD.FromString("Failed"));
            }
            UTF8Encoding encoding = new UTF8Encoding();
            httpResponse.ContentType = "application/json";
            return encoding.GetBytes(OSDParser.SerializeJsonString(resp, true));
        }

        private bool ValidateUser(OSHttpRequest request, OSDMap map)
        {
            if(!m_runLocal)
                if (map.ContainsKey("WebPassword") && (map["WebPassword"] == m_password))
                    return true;
            if (m_runLocal)
            {
                if (request.RemoteIPEndPoint.Address.Equals(IPAddress.Loopback))
                    return true;
            }
            return false;
        }

        #endregion

        #region WebUI API methods

        #region module-specific

        private OSDMap WebUIClientImplementationData(OSDMap map)
        {
            return WebUI.WebUIClientImplementationData();
        }

        #endregion

        #region Grid

        private OSDMap OnlineStatus(OSDMap map)
        {
            ILoginService loginService = m_registry.RequestModuleInterface<ILoginService>();
            bool LoginEnabled = loginService.MinLoginLevel == 0;

            OSDMap resp = new OSDMap();
            resp["Online"] = OSD.FromBoolean(true);
            resp["LoginEnabled"] = OSD.FromBoolean(LoginEnabled);

            return resp;
        }

        private OSDMap get_grid_info(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["GridInfo"] = GridInfo;
            return resp;
        }

        #endregion

        #region Account

        #region Registration

        private OSDMap CheckIfUserExists(OSDMap map)
        {
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, map["Name"].AsString());

            bool Verified = user != null;
            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(Verified);
            resp["UUID"] = OSD.FromUUID(Verified ? user.PrincipalID : UUID.Zero);
            return resp;
        }

        private OSDMap CreateAccount(OSDMap map)
        {
            bool Verified = false;
            string Name = map["Name"].AsString();
            string PasswordHash = map["PasswordHash"].AsString();
            //string PasswordSalt = map["PasswordSalt"].AsString();
            string HomeRegion = map["HomeRegion"].AsString();
            string Email = map["Email"].AsString();
            string AvatarArchive = map["AvatarArchive"].AsString();
            int userLevel = map["UserLevel"].AsInteger();

            bool activationRequired = map.ContainsKey("ActivationRequired") ? map["ActivationRequired"].AsBoolean() : false;
  

            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            if (accountService == null)
                return null;

            PasswordHash = PasswordHash.StartsWith("$1$") ? PasswordHash.Remove(0, 3) : Util.Md5Hash(PasswordHash); //remove $1$

            accountService.CreateUser(Name, PasswordHash, Email);
            UserAccount user = accountService.GetUserAccount(UUID.Zero, Name);
            IAgentInfoService agentInfoService = m_registry.RequestModuleInterface<IAgentInfoService> ();
            IGridService gridService = m_registry.RequestModuleInterface<IGridService> ();
            if (agentInfoService != null && gridService != null)
            {
                UUID homeRegion;
                GridRegion r = UUID.TryParse(HomeRegion, out homeRegion) ? gridService.GetRegionByUUID(UUID.Zero, homeRegion) : gridService.GetRegionByName (UUID.Zero, HomeRegion);
                if (r != null)
                {
                    agentInfoService.SetHomePosition(user.PrincipalID.ToString(), r.RegionID, new Vector3(r.RegionSizeX / 2, r.RegionSizeY / 2, 20), Vector3.Zero);
                }
                else
                {
                    MainConsole.Instance.DebugFormat("[WebUI]: Could not set home position for user {0}, region \"{1}\" did not produce a result from the grid service", user.PrincipalID.ToString(), HomeRegion);
                }
            }

            Verified = user != null;
            UUID userID = UUID.Zero;

            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(Verified);

            if (Verified)
            {
                userID = user.PrincipalID;
                user.UserLevel = userLevel;

                // could not find a way to save this data here.
                DateTime RLDOB = map["RLDOB"].AsDate();
                string RLFirstName = map["RLFirstName"].AsString();
                string RLLastName = map["RLLastName"].AsString();
                string RLAddress = map["RLAddress"].AsString();
                string RLCity = map["RLCity"].AsString();
                string RLZip = map["RLZip"].AsString();
                string RLCountry = map["RLCountry"].AsString();
                string RLIP = map["RLIP"].AsString();

                IAgentConnector con = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector> ();
                con.CreateNewAgent (userID);

                IAgentInfo agent = con.GetAgent (userID);
                agent.OtherAgentInformation["RLDOB"] = RLDOB;
                agent.OtherAgentInformation["RLFirstName"] = RLFirstName;
                agent.OtherAgentInformation["RLLastName"] = RLLastName;
                agent.OtherAgentInformation["RLAddress"] = RLAddress;
                agent.OtherAgentInformation["RLCity"] = RLCity;
                agent.OtherAgentInformation["RLZip"] = RLZip;
                agent.OtherAgentInformation["RLCountry"] = RLCountry;
                agent.OtherAgentInformation["RLIP"] = RLIP;
                if (activationRequired)
                {
                    UUID activationToken = UUID.Random();
                    agent.OtherAgentInformation["WebUIActivationToken"] = Util.Md5Hash(activationToken.ToString() + ":" + PasswordHash);
                    resp["WebUIActivationToken"] = activationToken;
                }
                con.UpdateAgent (agent);
                
                accountService.StoreUserAccount(user);

                IProfileConnector profileData = Aurora.DataManager.DataManager.RequestPlugin<IProfileConnector>();
                IUserProfileInfo profile = profileData.GetUserProfile(user.PrincipalID);
                if (profile == null)
                {
                    profileData.CreateNewProfile(user.PrincipalID);
                    profile = profileData.GetUserProfile(user.PrincipalID);
                }
                if (AvatarArchive.Length > 0)
                    profile.AArchiveName = AvatarArchive + ".database";

                profile.IsNewUser = true;
                profileData.UpdateUserProfile(profile);
            }

            resp["UUID"] = OSD.FromUUID(userID);
            return resp;
        }

        private OSDMap GetAvatarArchives(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            List<AvatarArchive> temp = Aurora.DataManager.DataManager.RequestPlugin<IAvatarArchiverConnector>().GetAvatarArchives(true);

            OSDArray names = new OSDArray();
            OSDArray snapshot = new OSDArray();

            MainConsole.Instance.DebugFormat("[WebUI] {0} avatar archives found", temp.Count);

            foreach (AvatarArchive a in temp)
            {
                names.Add(OSD.FromString(a.Name));
                snapshot.Add(OSD.FromUUID(UUID.Parse(a.Snapshot)));
            }

            resp["names"] = names;
            resp["snapshot"] = snapshot;

            return resp;
        }

        private OSDMap Authenticated(OSDMap map)
        {
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, map["UUID"].AsUUID());

            bool Verified = user != null;
            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(Verified);

            if (Verified)
            {
                user.UserLevel = map.ContainsKey("value") ? map["value"].AsInteger() : 0;
                accountService.StoreUserAccount(user);
                IAgentConnector con = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
                IAgentInfo agent = con.GetAgent(user.PrincipalID);
                if (agent != null && agent.OtherAgentInformation.ContainsKey("WebUIActivationToken"))
                {
                    agent.OtherAgentInformation.Remove("WebUIActivationToken");
                    con.UpdateAgent(agent);
                }
            }

            return resp;
        }

        private OSDMap ActivateAccount(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(false);

            if (map.ContainsKey("UserName") && map.ContainsKey("PasswordHash") && map.ContainsKey("ActivationToken"))
            {
                IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
                UserAccount user = accountService.GetUserAccount(UUID.Zero, map["UserName"].ToString());
                if (user != null)
                {
                    IAgentConnector con = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
                    IAgentInfo agent = con.GetAgent(user.PrincipalID);
                    if (agent != null && agent.OtherAgentInformation.ContainsKey("WebUIActivationToken"))
                    {
                        UUID activationToken = map["ActivationToken"];
                        string WebUIActivationToken = agent.OtherAgentInformation["WebUIActivationToken"];
                        string PasswordHash = map["PasswordHash"];
                        if (!PasswordHash.StartsWith("$1$"))
                        {
                            PasswordHash = "$1$" + Util.Md5Hash(PasswordHash);
                        }
                        PasswordHash = PasswordHash.Remove(0, 3); //remove $1$

                        bool verified = Utils.MD5String(activationToken.ToString() + ":" + PasswordHash) == WebUIActivationToken;
                        resp["Verified"] = verified;
                        if (verified)
                        {
                            user.UserLevel = 0;
                            accountService.StoreUserAccount(user);
                            agent.OtherAgentInformation.Remove("WebUIActivationToken");
                            con.UpdateAgent(agent);
                        }
                    }
                }
            }

            return resp;
        }

        #endregion

        #region Login

        private OSDMap Login(OSDMap map, bool asAdmin)
        {
            string Name = map["Name"].AsString();
            string Password = map["Password"].AsString();

            ILoginService loginService = m_registry.RequestModuleInterface<ILoginService>();
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount account = null;
            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(false);

            if (accountService == null || CheckIfUserExists(map)["Verified"] != true)
            {
                return resp;
            }

            account = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, Name);

            //Null means it went through without an errorz
            if (loginService.VerifyClient(account.PrincipalID, Name, "UserAccount", Password, account.ScopeID))
            {
                account = m_registry.RequestModuleInterface<IUserAccountService> ().GetUserAccount (UUID.Zero, Name);
                if (asAdmin)
                {
                    IAgentInfo agent = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().GetAgent(account.PrincipalID);
                    if (agent.OtherAgentInformation["WebUIEnabled"].AsBoolean() == false)
                    {
                        return resp;
                    }
                }
                resp["UUID"] = OSD.FromUUID (account.PrincipalID);
                resp["FirstName"] = OSD.FromString (account.FirstName);
                resp["LastName"] = OSD.FromString (account.LastName);
                resp["Email"] = OSD.FromString(account.Email);
                resp["Verified"] = OSD.FromBoolean(true);
                MainConsole.Instance.Trace("Login for " + Name + " was successful");
            }else{
                MainConsole.Instance.Trace("Login for " + Name + " was not successful");
            }

            return resp;
        }

        private OSDMap SetWebLoginKey(OSDMap map)
        {
            OSDMap resp = new OSDMap ();
            UUID principalID = map["PrincipalID"].AsUUID();
            UUID webLoginKey = UUID.Random();
            IAuthenticationService authService = m_registry.RequestModuleInterface<IAuthenticationService>();
            IAuthenticationData authData = Aurora.DataManager.DataManager.RequestPlugin<IAuthenticationData>();
            if (authService != null && authData != null)
            {
                //Remove the old
                authData.Delete(principalID, "WebLoginKey");
                authService.SetPlainPassword(principalID, "WebLoginKey", webLoginKey.ToString());
                resp["WebLoginKey"] = webLoginKey;
            }
            resp["Failed"] = OSD.FromString(String.Format("No auth service, cannot set WebLoginKey for user {0}.", map["PrincipalID"].AsUUID().ToString()));

            return resp;
        }

        #endregion

        #region Email

        /// <summary>
        /// After conformation the email is saved
        /// </summary>
        /// <param name="map">UUID, Email</param>
        /// <returns>Verified</returns>
        private OSDMap SaveEmail(OSDMap map)
        {
            string email = map["Email"].AsString();

            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, map["UUID"].AsUUID());
            OSDMap resp = new OSDMap();

            bool verified = user != null;
            resp["Verified"] = OSD.FromBoolean(verified);
            if (verified)
            {
                user.Email = email;
                user.UserLevel = 0;
                accountService.StoreUserAccount(user);
            }
            return resp;
        }


        private OSDMap ConfirmUserEmailName(OSDMap map)
        {
            string Name = map["Name"].AsString();
            string Email = map["Email"].AsString();

            OSDMap resp = new OSDMap();
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, Name);
            bool verified = user != null;
            resp["Verified"] = OSD.FromBoolean(verified);

            if (verified)
            {
                resp["UUID"] = OSD.FromUUID(user.PrincipalID);
                if (user.UserLevel >= 0)
                {
                    if (user.Email.ToLower() != Email.ToLower())
                    {
                        MainConsole.Instance.TraceFormat("User email for account \"{0}\" is \"{1}\" but \"{2}\" was specified.", Name, user.Email.ToString(), Email);
                        resp["Error"] = OSD.FromString("Email does not match the user name.");
                        resp["ErrorCode"] = OSD.FromInteger(3);
                    }
                }
                else
                {
                    resp["Error"] = OSD.FromString("This account is disabled.");
                    resp["ErrorCode"] = OSD.FromInteger(2);
                }
            }
            else
            {
                resp["Error"] = OSD.FromString("No such user.");
                resp["ErrorCode"] = OSD.FromInteger(1);
            }


            return resp;
        }

        #endregion

        #region password

        private OSDMap ChangePassword(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            string Password = map["Password"].AsString();
            string newPassword = map["NewPassword"].AsString();
            UUID userID = map["UUID"].AsUUID();

            ILoginService loginService = m_registry.RequestModuleInterface<ILoginService>();
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            IAuthenticationService auths = m_registry.RequestModuleInterface<IAuthenticationService>();

            UserAccount account = accountService.GetUserAccount(UUID.Zero, userID);

            //Null means it went through without an error
            bool Verified = loginService.VerifyClient(account.PrincipalID, account.Name, "UserAccount", Password, account.ScopeID);

            if ((auths.Authenticate(userID, "UserAccount", Password.StartsWith("$1$") ? Password.Remove(0, 3) : Util.Md5Hash(Password), 100) != string.Empty) && (Verified))
            {
                auths.SetPasswordHashed(userID, "UserAccount", newPassword.StartsWith("$1$") ? newPassword.Remove(0, 3) : Util.Md5Hash(newPassword));
                resp["Verified"] = OSD.FromBoolean(Verified);
            }

            return resp;
        }

        private OSDMap ForgotPassword(OSDMap map)
        {
            UUID UUDI = map["UUID"].AsUUID();
            string Password = map["Password"].AsString();

            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, UUDI);

            OSDMap resp = new OSDMap();
            bool verified = user != null;
            resp["Verified"] = OSD.FromBoolean(verified);
            resp["UserLevel"] = OSD.FromInteger(0);
            if (verified)
            {
                resp["UserLevel"] = OSD.FromInteger(user.UserLevel);
                if (user.UserLevel >= 0)
                {
                    IAuthenticationService auths = m_registry.RequestModuleInterface<IAuthenticationService>();
                    auths.SetPassword (user.PrincipalID, "UserAccount", Password);
                }
                else
                {
                    resp["Verified"] = OSD.FromBoolean(false);
                }
            }

            return resp;
        }
		
		private OSDMap ChangePassword2(OSDMap map)
		{
		    return ForgotPassword(map);
		}

        #endregion

        /// <summary>
        /// Changes user name
        /// </summary>
        /// <param name="map">UUID, FirstName, LastName</param>
        /// <returns>Verified</returns>
        private OSDMap ChangeName(OSDMap map)
        {
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, map["UUID"].AsUUID());
            OSDMap resp = new OSDMap();

            bool verified = user != null;
            resp["Verified"] = OSD.FromBoolean(verified);
            if (verified)
            {
                user.Name = map["Name"].AsString();
                resp["Stored" ] = OSD.FromBoolean(accountService.StoreUserAccount(user));
            }

            return resp;
        }

        private OSDMap EditUser(OSDMap map)
        {
            bool editRLInfo = (map.ContainsKey("RLName") && map.ContainsKey("RLAddress") && map.ContainsKey("RLZip") && map.ContainsKey("RLCity") && map.ContainsKey("RLCountry"));
            OSDMap resp = new OSDMap();
            resp["agent"] = OSD.FromBoolean(!editRLInfo); // if we have no RLInfo, editing account is assumed to be successful.
            resp["account"] = OSD.FromBoolean(false);
            UUID principalID = map["UserID"].AsUUID();
            UserAccount account = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, principalID);
            if(account != null)
            {
                account.Email = map["Email"];
                if (m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, map["Name"].AsString()) == null)
                {
                    account.Name = map["Name"];
                }
                if (map.ContainsKey("UserLevel"))
                {
                    account.UserLevel = map["UserLevel"].AsInteger();
                }

                if (editRLInfo)
                {
                    IAgentConnector agentConnector = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
                    IAgentInfo agent = agentConnector.GetAgent(account.PrincipalID);
                    if (agent == null)
                    {
                        agentConnector.CreateNewAgent(account.PrincipalID);
                        agent = agentConnector.GetAgent(account.PrincipalID);
                    }
                    if (agent != null)
                    {
                        agent.OtherAgentInformation["RLName"] = map["RLName"];
                        agent.OtherAgentInformation["RLAddress"] = map["RLAddress"];
                        agent.OtherAgentInformation["RLZip"] = map["RLZip"];
                        agent.OtherAgentInformation["RLCity"] = map["RLCity"];
                        agent.OtherAgentInformation["RLCountry"] = map["RLCountry"];
                        agentConnector.UpdateAgent(agent);
                        resp["agent"] = OSD.FromBoolean(true);
                    }
                }
                resp["account"] = OSD.FromBoolean(m_registry.RequestModuleInterface<IUserAccountService>().StoreUserAccount(account));
            }
            return resp;
        }

        private OSDMap ResetAvatar(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            UUID user = UUID.Zero;

            if (!map.ContainsKey("User"))
            {
                resp["Failed"] = new OSDString("User not specified.");
            }
            else if (!UUID.TryParse(map["User"].AsString(), out user))
            {
                resp["Failed"] = new OSDString("User specified but was not valid UUID.");
            }
            else
            {
                IAvatarService avatarService = m_registry.RequestModuleInterface<IAvatarService>();

                if (avatarService == null)
                {
                    resp["Failed"] = new OSDString("Avatar service could not be fetched.");
                }
                else
                {
                    resp["Success"] = new OSDBoolean(avatarService.ResetAvatar(user));
                }
            }


            return resp;
        }

        #endregion

        #region Users

        private OSDMap UserAccount2InfoWebOSD(UserAccount user)
        {
            OSDMap resp = new OSDMap();

            IAgentInfoService agentService = m_registry.RequestModuleInterface<IAgentInfoService>();

            UserInfo userinfo = agentService.GetUserInfo(user.PrincipalID.ToString());
            IGridService gs = m_registry.RequestModuleInterface<IGridService>();
            GridRegion homeRegion = null;
            GridRegion currentRegion = null;
            if (userinfo != null)
            {
                homeRegion = gs.GetRegionByUUID(UUID.Zero, userinfo.HomeRegionID);
                currentRegion = userinfo.CurrentRegionID != UUID.Zero ? gs.GetRegionByUUID(UUID.Zero, userinfo.CurrentRegionID) : null;
            }

            resp["UUID"] = OSD.FromUUID(user.PrincipalID);
            resp["HomeUUID"] = OSD.FromUUID((userinfo == null) ? UUID.Zero : userinfo.HomeRegionID);
            resp["HomeName"] = OSD.FromString((userinfo == null || homeRegion == null) ? "" : homeRegion.RegionName);
            resp["CurrentRegionUUID"] = OSD.FromUUID((userinfo == null) ? UUID.Zero : userinfo.CurrentRegionID);
            resp["CurrentRegionName"] = OSD.FromString((userinfo == null || currentRegion == null) ? "" : currentRegion.RegionName);
            resp["Online"] = OSD.FromBoolean((userinfo == null) ? false : userinfo.IsOnline);
            resp["Email"] = OSD.FromString(user.Email);
            resp["Name"] = OSD.FromString(user.Name);
            resp["FirstName"] = OSD.FromString(user.FirstName);
            resp["LastName"] = OSD.FromString(user.LastName);
            resp["LastLogin"] = userinfo == null ? OSD.FromBoolean(false) : OSD.FromInteger((int)Utils.DateTimeToUnixTime(userinfo.LastLogin));
            resp["LastLogout"] = userinfo == null ? OSD.FromBoolean(false) : OSD.FromInteger((int)Utils.DateTimeToUnixTime(userinfo.LastLogout));

            return resp;
        }

        private OSDMap UserInfo2InfoWebOSD(UserInfo userinfo)
        {
            OSDMap resp = new OSDMap();

            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            IGridService gs = m_registry.RequestModuleInterface<IGridService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, new UUID(userinfo.UserID));

            GridRegion homeRegion = gs.GetRegionByUUID(UUID.Zero, userinfo.HomeRegionID);
            GridRegion currentRegion = userinfo.CurrentRegionID != UUID.Zero ? gs.GetRegionByUUID(UUID.Zero, userinfo.CurrentRegionID) : null;

            resp["UUID"] = OSD.FromUUID(user.PrincipalID);
            resp["HomeUUID"] = OSD.FromUUID((userinfo == null) ? UUID.Zero : userinfo.HomeRegionID);
            resp["HomeName"] = OSD.FromString((userinfo == null || homeRegion == null) ? "" : homeRegion.RegionName);
            resp["CurrentRegionUUID"] = OSD.FromUUID((userinfo == null) ? UUID.Zero : userinfo.CurrentRegionID);
            resp["CurrentRegionName"] = OSD.FromString((userinfo == null || currentRegion == null) ? "" : currentRegion.RegionName);
            resp["Online"] = OSD.FromBoolean((userinfo == null) ? false : userinfo.IsOnline);
            resp["Email"] = OSD.FromString(user.Email);
            resp["Name"] = OSD.FromString(user.Name);
            resp["FirstName"] = OSD.FromString(user.FirstName);
            resp["LastName"] = OSD.FromString(user.LastName);
            resp["LastLogin"] = userinfo == null ? OSD.FromBoolean(false) : OSD.FromInteger((int)Utils.DateTimeToUnixTime(userinfo.LastLogin));
            resp["LastLogout"] = userinfo == null ? OSD.FromBoolean(false) : OSD.FromInteger((int)Utils.DateTimeToUnixTime(userinfo.LastLogout));

            return resp;
        }

        /// <summary>
        /// Gets user information for change user info page on site
        /// </summary>
        /// <param name="map">UUID</param>
        /// <returns>Verified, HomeName, HomeUUID, Online, Email, FirstName, LastName</returns>
        private OSDMap GetGridUserInfo(OSDMap map)
        {
            string uuid = String.Empty;
            uuid = map["UUID"].AsString();

            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount user = accountService.GetUserAccount(UUID.Zero, map["UUID"].AsUUID());
            IAgentInfoService agentService = m_registry.RequestModuleInterface<IAgentInfoService>();

            OSDMap resp = new OSDMap();
            bool verified = user != null;
            resp["Verified"] = OSD.FromBoolean(verified);
            if (verified)
            {
                resp = UserAccount2InfoWebOSD(user);
            }

            return resp;
        }

        private OSDMap GetProfile(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            string Name = map["Name"].AsString();
            UUID userID = map["UUID"].AsUUID();

            UserAccount account = Name != "" ? 
                m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, Name) :
                 m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, userID);
            if (account != null)
            {
                OSDMap accountMap = new OSDMap();

                accountMap["Created"] = account.Created;
                accountMap["Name"] = account.Name;
                accountMap["PrincipalID"] = account.PrincipalID;
                accountMap["Email"] = account.Email;
                accountMap["UserLevel"] = account.UserLevel;

                TimeSpan diff = DateTime.Now - Util.ToDateTime(account.Created);
                int years = (int)diff.TotalDays / 356;
                int days = years > 0 ? (int)diff.TotalDays / years : (int)diff.TotalDays;
                accountMap["TimeSinceCreated"] = years + " years, " + days + " days"; // if we're sending account.Created do we really need to send this string ?

                IProfileConnector profileConnector = Aurora.DataManager.DataManager.RequestPlugin<IProfileConnector>();
                IUserProfileInfo profile = profileConnector.GetUserProfile(account.PrincipalID);
                if (profile != null)
                {
                    resp["profile"] = profile.ToOSD(false);//not trusted, use false

                    if (account.UserFlags == 0)
                        account.UserFlags = 2; //Set them to no info given

                    string flags = ((IUserProfileInfo.ProfileFlags)account.UserFlags).ToString();
                    IUserProfileInfo.ProfileFlags.NoPaymentInfoOnFile.ToString();

                    accountMap["AccountInfo"] = (profile.CustomType != "" ? profile.CustomType :
                        account.UserFlags == 0 ? "Resident" : "Admin") + "\n" + flags;
                    UserAccount partnerAccount = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, profile.Partner);
                    if (partnerAccount != null)
                    {
                        accountMap["Partner"] = partnerAccount.Name;
                        accountMap["PartnerUUID"] = partnerAccount.PrincipalID;
                    }
                    else
                    {
                        accountMap["Partner"] = "";
                        accountMap["PartnerUUID"] = UUID.Zero;
                    }

                }
                IAgentConnector agentConnector = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>();
                IAgentInfo agent = agentConnector.GetAgent(account.PrincipalID);
                if(agent != null)
                {
                    OSDMap agentMap = new OSDMap();
                    agentMap["RLName"] = agent.OtherAgentInformation["RLName"].AsString();
                    agentMap["RLAddress"] = agent.OtherAgentInformation["RLAddress"].AsString();
                    agentMap["RLZip"] = agent.OtherAgentInformation["RLZip"].AsString();
                    agentMap["RLCity"] = agent.OtherAgentInformation["RLCity"].AsString();
                    agentMap["RLCountry"] = agent.OtherAgentInformation["RLCountry"].AsString();
                    resp["agent"] = agentMap;
                }
                resp["account"] = accountMap;
            }

            return resp;
        }

        private OSDMap DeleteUser(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Finished"] = OSD.FromBoolean(true);

            UUID agentID = map["UserID"].AsUUID();
            IAgentInfo GetAgent = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().GetAgent(agentID);

            if (GetAgent != null)
            {
                GetAgent.Flags &= ~IAgentFlags.PermBan;
                Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().UpdateAgent(GetAgent);
            }
            return resp;
        }

        private OSDMap SetHomeLocation(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IAgentInfoService agentService = m_registry.RequestModuleInterface<IAgentInfoService>();
            IGridService gridService = m_registry.RequestModuleInterface<IGridService>();
            UserInfo userinfo = (map.ContainsKey("User") && agentService != null) ? agentService.GetUserInfo(map["User"].AsString()) : null;

            if (!map.ContainsKey("User"))
            {
                resp["Failed"] = new OSDString("No user specified");
            }
            else if (!map.ContainsKey("RegionID") && !map.ContainsKey("Position") && !map.ContainsKey("LookAt"))
            {
                resp["Failed"] = new OSDString("No position info specified");
            }
            else if (agentService == null)
            {
                resp["Failed"] = new OSDString("Could not get IAgentInfoService");
            }
            else if (gridService == null)
            {
                resp["Failed"] = new OSDString("Could not get IGridService");
            }
            else if (userinfo == null)
            {
                resp["Failed"] = new OSDString("Could not find user");
            }
            else
            {
                UUID scopeID = UUID.Zero;
                UUID regionID = UUID.Zero;
                Vector3 position = Vector3.Zero;
                Vector3 lookAt = Vector3.Zero;

                List<string> fail = new List<string>();

                if (map.ContainsKey("ScopeID") && !UUID.TryParse(map["ScopeID"].AsString(), out scopeID))
                {
                    fail.Add("ScopeID was specified but was not a valid UUID");
                }
                if (map.ContainsKey("RegionID") && !UUID.TryParse(map["RegionID"].AsString(), out regionID))
                {
                    fail.Add("RegionID was specified but was not valid UUID");
                }
                if (map.ContainsKey("Position") && !Vector3.TryParse(map["Position"].AsString(), out position))
                {
                    fail.Add("Position was specified but was not valid Vector3");
                }
                if (map.ContainsKey("LookAt") && !Vector3.TryParse(map["LookAt"].AsString(), out lookAt))
                {
                    fail.Add("LookAt was specified but was not valid Vector3");
                }

                if (regionID == UUID.Zero)
                {
                    regionID = userinfo.HomeRegionID;
                }
                if (gridService.GetRegionByUUID(UUID.Zero, regionID) == null)
                {
                    fail.Add("region does not exist");
                }

                if (regionID == UUID.Zero && (map.ContainsKey("Position") || map.ContainsKey("LookAt")))
                {
                    fail.Add("Cannot change home location without specifying a region");
                }

                if (fail.Count > 0)
                {
                    resp["Failed"] = new OSDString(string.Join(". ", fail.ToArray()));
                    return resp;
                }

                userinfo.HomeRegionID = regionID;
                if (map.ContainsKey("Position"))
                {
                    userinfo.HomePosition = position;
                }
                if (map.ContainsKey("LookAt"))
                {
                    userinfo.HomeLookAt = lookAt;
                }

                resp["Success"] = new OSDBoolean(agentService.SetHomePosition(userinfo.UserID, userinfo.HomeRegionID, userinfo.HomePosition, userinfo.HomeLookAt));
            }

            return resp;
        }

        #region banning

        private void doBan(UUID agentID, DateTime? until){
            IAgentInfo GetAgent = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().GetAgent(agentID);
            if (GetAgent != null)
            {
                GetAgent.Flags &= (until.HasValue) ? ~IAgentFlags.TempBan : ~IAgentFlags.PermBan;
                if (until.HasValue)
                {
                    GetAgent.OtherAgentInformation["TemperaryBanInfo"] = until.Value.ToString("s");
                    MainConsole.Instance.TraceFormat("Temp ban for {0} until {1}", agentID, until.Value.ToString("s"));
                }
                Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().UpdateAgent(GetAgent);
            }
        }

        private OSDMap BanUser(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Finished"] = OSD.FromBoolean(true);
            UUID agentID = map["UserID"].AsUUID();
            doBan(agentID,null);

            return resp;
        }

        private OSDMap TempBanUser(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Finished"] = OSD.FromBoolean(true);
            UUID agentID = map["UserID"].AsUUID();
            DateTime until = map["BannedUntil"].AsDate();
            doBan(agentID, until);

            return resp;
        }

        private OSDMap UnBanUser(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Finished"] = OSD.FromBoolean(true);

            UUID agentID = map["UserID"].AsUUID();
            IAgentInfo GetAgent = Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().GetAgent(agentID);

            if (GetAgent != null)
            {
                GetAgent.Flags &= IAgentFlags.PermBan;
                GetAgent.Flags &= IAgentFlags.TempBan;
                if (GetAgent.OtherAgentInformation.ContainsKey("TemperaryBanInfo") == true)
                {
                    GetAgent.OtherAgentInformation.Remove("TemperaryBanInfo");
                }
                Aurora.DataManager.DataManager.RequestPlugin<IAgentConnector>().UpdateAgent(GetAgent);
            }

            return resp;
        }

        #endregion

        private OSDMap FindUsers(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IUserAccountService accountService = m_registry.RequestModuleInterface<IUserAccountService>();
            if (accountService == null)
            {
                map["Failed"] = new OSDString("Could not find IUserAccountService");
            }
            else
            {
                UUID scopeID = map.ContainsKey("ScopeID") ? map["ScopeID"].AsUUID() : UUID.Zero;
                uint start = map.ContainsKey("Start") ? map["Start"].AsUInteger() : 0;
                uint count = map.ContainsKey("Count") ? map["Count"].AsUInteger() : 10;
                string Query = map["Query"].AsString();
                List<UserAccount> accounts = accountService.GetUserAccounts(scopeID, Query, start, count);

                OSDArray users = new OSDArray();
                MainConsole.Instance.TraceFormat("{0} accounts found", accounts.Count);
                foreach (UserAccount acc in accounts)
                {
                    OSDMap userInfo = new OSDMap();
                    userInfo["PrincipalID"] = acc.PrincipalID;
                    userInfo["UserName"] = acc.Name;
                    userInfo["Created"] = acc.Created;
                    userInfo["UserFlags"] = acc.UserFlags;
                    users.Add(userInfo);
                }
                resp["Users"] = users;

                resp["Start"] = OSD.FromInteger(start);
                resp["Count"] = OSD.FromInteger(count);
                resp["Query"] = OSD.FromString(Query);
                resp["Total"] = OSD.FromInteger((int)accountService.NumberOfUserAccounts(scopeID, Query));
            }

            return resp;
        }

        private OSDMap GetFriends(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            if (map.ContainsKey("UserID") == false)
            {
                resp["Failed"] = OSD.FromString("User ID not specified.");
                return resp;
            }

            IFriendsService friendService = m_registry.RequestModuleInterface<IFriendsService>();

            if (friendService == null)
            {
                resp["Failed"] = OSD.FromString("No friend service found.");
                return resp;
            }

            List<FriendInfo> friendsList = new List<FriendInfo>(friendService.GetFriends(map["UserID"].AsUUID()));
            OSDArray friends = new OSDArray(friendsList.Count);
            foreach (FriendInfo friendInfo in friendsList)
            {
                UserAccount account = m_registry.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, UUID.Parse(friendInfo.Friend));
                OSDMap friend = new OSDMap(4);
                friend["PrincipalID"] = friendInfo.Friend;
                friend["Name"] = account.Name;
                friend["MyFlags"] = friendInfo.MyFlags;
                friend["TheirFlags"] = friendInfo.TheirFlags;
                friends.Add(friend);
            }

            resp["Friends"] = friends;

            return resp;
        }

        #region statistics

        private OSDMap NumberOfRecentlyOnlineUsers(OSDMap map)
        {
            uint secondsAgo = map.ContainsKey("secondsAgo") ? uint.Parse(map["secondsAgo"]) : 0;
            bool stillOnline = map.ContainsKey("stillOnline") ? uint.Parse(map["stillOnline"]) == 1 : false;
            IAgentInfoConnector users = DataManager.DataManager.RequestPlugin<IAgentInfoConnector>();

            OSDMap resp = new OSDMap();
            resp["secondsAgo"] = OSD.FromInteger((int)secondsAgo);
            resp["stillOnline"] = OSD.FromBoolean(stillOnline);
            resp["result"] = OSD.FromInteger(users != null ? (int)users.RecentlyOnline(secondsAgo, stillOnline) : 0);

            return resp;
        }

        private OSDMap RecentlyOnlineUsers(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            uint secondsAgo = map.ContainsKey("secondsAgo") ? uint.Parse(map["secondsAgo"]) : 0;
            bool stillOnline = map.ContainsKey("stillOnline") ? uint.Parse(map["stillOnline"]) == 1 : false;
            uint start = map.ContainsKey("Start") ? map["Start"].AsUInteger() : 0;
            uint count = map.ContainsKey("Count") ? map["Count"].AsUInteger() : 10;

            IAgentInfoConnector userInfoService = DataManager.DataManager.RequestPlugin<IAgentInfoConnector>();
            if (userInfoService == null)
            {
                resp["Failed"] = new OSDString("Could not get IAgentInfoConnector");
            }
            else
            {
                resp["Start"] = OSD.FromInteger((int)start);
                resp["Count"] = OSD.FromInteger((int)count);
                resp["Total"] = OSD.FromInteger((int)userInfoService.RecentlyOnline(secondsAgo, stillOnline));

                OSDArray Users = new OSDArray();
                Dictionary<string, bool> sort = new Dictionary<string,bool>(1);
                sort["LastSeen"] = true;
                List<UserInfo> users = userInfoService.RecentlyOnline(secondsAgo, stillOnline, sort, start, count);

                foreach (UserInfo userinfo in users)
                {
                    Users.Add(UserInfo2InfoWebOSD(userinfo));
                }

                resp["Users"] = Users;
            }

            return resp;
        }

        #endregion

        #endregion

        #region IAbuseReports

        private OSDMap GetAbuseReports(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IAbuseReports ar = m_registry.RequestModuleInterface<IAbuseReports>();

            int start = map["Start"].AsInteger();
            int count = map["Count"].AsInteger();
            bool active = map["Active"].AsBoolean();

            List<AbuseReport> lar = ar.GetAbuseReports(start, count, active);
            OSDArray AbuseReports = new OSDArray();
            foreach (AbuseReport tar in lar)
            {
                AbuseReports.Add(tar.ToOSD());
            }

            resp["AbuseReports"] = AbuseReports;
            resp["Start"] = OSD.FromInteger(start);
            resp["Count"] = OSD.FromInteger(count); // we're not using the AbuseReports.Count because client implementations of the WebUI API can check the count themselves. This is just for showing the input.
            resp["Active"] = OSD.FromBoolean(active);

            return resp;
        }

        private OSDMap AbuseReportMarkComplete(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IAbuseReports ar = m_registry.RequestModuleInterface<IAbuseReports>();
            AbuseReport tar = ar.GetAbuseReport(map["Number"].AsInteger(), map["WebPassword"].AsString());
            if (tar != null)
            {
                tar.Active = false;
                ar.UpdateAbuseReport(tar, map["WebPassword"].AsString());
                resp["Finished"] = OSD.FromBoolean(true);
            }
            else
            {
                resp["Finished"] = OSD.FromBoolean(false);
                resp["Failed"] = OSD.FromString(String.Format("No abuse report found with specified number {0}", map["Number"].AsInteger()));
            }

            return resp;
        }

        private OSDMap AbuseReportSaveNotes(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IAbuseReports ar = m_registry.RequestModuleInterface<IAbuseReports>();
            AbuseReport tar = ar.GetAbuseReport(map["Number"].AsInteger(), map["WebPassword"].AsString());
            if (tar != null)
            {
                tar.Notes = map["Notes"].ToString();
                ar.UpdateAbuseReport(tar, map["WebPassword"].AsString());
                resp["Finished"] = OSD.FromBoolean(true);
            }
            else
            {
                resp["Finished"] = OSD.FromBoolean(false);
                resp["Failed"] = OSD.FromString(String.Format("No abuse report found with specified number {0}", map["Number"].AsInteger()));
            }

            return resp;
        }

        #endregion

        #region Places

        #region Estate

        private static OSDMap EstateSettings2WebOSD(EstateSettings ES)
        {
            OSDMap es = ES.ToOSD();

            OSDArray bans = (OSDArray)es["EstateBans"];
            OSDArray Bans = new OSDArray(bans.Count);
            foreach (OSDMap ban in bans)
            {
                Bans.Add(OSD.FromUUID(ban["BannedUserID"]));
            }
            es["EstateBans"] = Bans;

            return es;
        }

        private OSDMap GetEstates(OSDMap map)
        {
            OSDMap resp = new OSDMap(1);
            resp["Estates"] = new OSDArray(0);

            IEstateConnector estates = Aurora.DataManager.DataManager.RequestPlugin<IEstateConnector>();

            if (estates != null && map.ContainsKey("Owner"))
            {
                Dictionary<string, bool> boolFields = new Dictionary<string, bool>();
                if (map.ContainsKey("BoolFields") && map["BoolFields"].Type == OSDType.Map)
                {
                    OSDMap fields = (OSDMap)map["BoolFields"];
                    foreach (string field in fields.Keys)
                    {
                        boolFields[field] = int.Parse(fields[field]) != 0;
                    }
                }

                resp["Estates"] = new OSDArray(estates.GetEstates(map["Owner"].AsUUID(), boolFields).ConvertAll<OSD>(x => EstateSettings2WebOSD(x)));
            }

            return resp;
        }

        private OSDMap GetEstate(OSDMap map)
        {
            OSDMap resp = new OSDMap(1);
            resp["Failed"] = true;

            IEstateConnector estates = Aurora.DataManager.DataManager.RequestPlugin<IEstateConnector>();
            if (estates != null && map.ContainsKey("Estate"))
            {
                int EstateID;
                EstateSettings es = null;
                if (int.TryParse(map["Estate"], out EstateID))
                {
                    es = estates.GetEstateSettings(map["Estate"].AsInteger());
                }
                else
                {
                    es = estates.GetEstateSettings(map["Estate"].AsString());
                }
                if (es != null)
                {
                    resp.Remove("Failed");
                    resp["Estate"] = EstateSettings2WebOSD(es);
                }
            }

            return resp;
        }

        #endregion

        #region Regions

        private static OSDMap GridRegion2WebOSD(GridRegion region){
            OSDMap regionOSD = region.ToOSD();
            regionOSD["EstateID"] = Aurora.DataManager.DataManager.RequestPlugin<IEstateConnector>().GetEstateID(region.RegionID);
            return regionOSD;
        }

        private OSDMap GetRegions(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            RegionFlags includeFlags = map.ContainsKey("RegionFlags") ? (RegionFlags)map["RegionFlags"].AsInteger() : RegionFlags.RegionOnline;
            RegionFlags excludeFlags = map.ContainsKey("ExcludeRegionFlags") ? (RegionFlags)map["ExcludeRegionFlags"].AsInteger() : 0;
            int start = map.Keys.Contains("Start") ? map["Start"].AsInteger() : 0;
            if(start < 0){
                start = 0;
            }
            int count = map.Keys.Contains("Count") ? map["Count"].AsInteger() : 10;
            if(count < 0){
                count = 1;
            }

            IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();

            Dictionary<string, bool> sort = new Dictionary<string, bool>();

            string[] supportedSort = new string[3]{
                "SortRegionName",
                "SortLocX",
                "SortLocY"
            };

            foreach (string sortable in supportedSort)
            {
                if (map.ContainsKey(sortable))
                {
                    sort[sortable.Substring(4)] = map[sortable].AsBoolean();
                }
            }

            List<GridRegion> regions = regiondata.Get(includeFlags, excludeFlags, (uint)start, (uint)count, sort);
            OSDArray Regions = new OSDArray();
            foreach(GridRegion region in regions){
                Regions.Add(GridRegion2WebOSD(region));
            }

            MainConsole.Instance.Trace("Total regions: " + regiondata.Count(includeFlags, excludeFlags));

            resp["Start"] = OSD.FromInteger(start);
            resp["Count"] = OSD.FromInteger(count);
            resp["Total"] = OSD.FromInteger((int)regiondata.Count(includeFlags, excludeFlags));
            resp["Regions"] = Regions;
            return resp;
        }

        private OSDMap GetRegionsByXY(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            if(!map.ContainsKey("X") || !map.ContainsKey("Y")){
                resp["Failed"] = new OSDString("X and Y coordinates not specified");
            }else{
                int x = map["X"].AsInteger();
                int y = map["Y"].AsInteger();
                UUID scope = map.ContainsKey("ScopeID") ? UUID.Parse(map["ScopeID"].AsString()) : UUID.Zero;
                RegionFlags include = map.Keys.Contains("RegionFlags") ? (RegionFlags)map["RegionFlags"].AsInteger() : RegionFlags.RegionOnline;
                RegionFlags? exclude = null;
                if (map.Keys.Contains("ExcludeRegionFlags"))
                {
                    exclude = (RegionFlags)map["ExcludeRegionFlags"].AsInteger();
                }

                IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();

                if (regiondata == null)
                {
                    resp["Failed"] = new OSDString("Could not get IRegionData plugin");
                }else
                {
                    List<GridRegion> regions = regiondata.Get(x, y, scope);
                    OSDArray Regions = new OSDArray();
                    foreach (GridRegion region in regions)
                    {
                        if (((int)region.Flags & (int)include) == (int)include && (!exclude.HasValue || ((int)region.Flags & (int)exclude.Value) != (int)exclude))
                        {
                            Regions.Add(GridRegion2WebOSD(region));
                        }
                    }
                    resp["Total"] = Regions.Count;
                    resp["Regions"] = Regions; 
                }
            }

            return resp;
        }

        private OSDMap GetRegionsInArea(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            if (!map.ContainsKey("StartX") || !map.ContainsKey("StartY") || !map.ContainsKey("EndX") || !map.ContainsKey("EndY"))
            {
                resp["Failed"] = new OSDString("Start and End x/y coordinates must be specified");
            }
            else
            {
                int StartX = map["StartX"].AsInteger();
                int StartY = map["StartY"].AsInteger();
                int EndX = map["EndX"].AsInteger();
                int EndY = map["EndY"].AsInteger();

                UUID scope = UUID.Zero;
                if (map.ContainsKey("ScopeID") && !UUID.TryParse(map["ScopeID"].AsString(), out scope))
                {
                    resp["Failed"] = new OSDString("ScopeID was specified but was not valid.");
                    return resp;
                }

                IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();
                if (regiondata == null)
                {
                    resp["Failed"] = new OSDString("Could not get IRegionData plugin");
                }
                else
                {
                    List<GridRegion> regions = regiondata.Get(StartX, StartY, EndX, EndY, scope);
                    OSDArray Regions = new OSDArray();
                    foreach (GridRegion region in regions)
                    {
                        Regions.Add(GridRegion2WebOSD(region));
                    }
                    resp["Total"] = Regions.Count;
                    resp["Regions"] = Regions;
                }
            }

            return resp;
        }

        private OSDMap GetRegionsInEstate(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            
            RegionFlags flags = map.Keys.Contains("RegionFlags") ? (RegionFlags)map["RegionFlags"].AsInteger() : RegionFlags.RegionOnline;
            uint start = map.Keys.Contains("Start") ? map["Start"].AsUInteger() : 0;
            uint count = map.Keys.Contains("Count") ? map["Count"].AsUInteger() : 10;
            Dictionary<string, bool> sort = new Dictionary<string, bool>();
            if (map.ContainsKey("Sort") && map["Sort"].Type == OSDType.Map)
            {
                OSDMap fields = (OSDMap)map["Sort"];
                foreach (string field in fields.Keys)
                {
                    sort[field] = int.Parse(fields[field]) != 0;
                }
            }

            resp["Start"] = OSD.FromInteger(start);
            resp["Count"] = OSD.FromInteger(count);
            resp["Total"] = OSD.FromInteger(0);
            resp["Regions"] = new OSDArray(0);

            IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();
            if (regiondata != null && map.ContainsKey("Estate"))
            {
                List<GridRegion> regions = regiondata.Get(start, count, map["Estate"].AsUInteger(), flags, sort);
                OSDArray Regions = new OSDArray(regions.Count);
                regions.ForEach(delegate(GridRegion region)
                {
                    Regions.Add(GridRegion2WebOSD(region));
                });
                resp["Total"] = regiondata.Count(map["Estate"].AsUInteger(), flags);
            }

            return resp;
        }

        private OSDMap GetRegion(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();
            if (regiondata != null && (map.ContainsKey("RegionID") || map.ContainsKey("Region")))
            {
                string regionName = map.ContainsKey("Region") ? map["Region"].ToString().Trim() : "";
                UUID regionID = map.ContainsKey("RegionID") ? UUID.Parse(map["RegionID"].ToString()) : UUID.Zero;
                UUID scopeID = map.ContainsKey("ScopeID") ? UUID.Parse(map["ScopeID"].ToString()) : UUID.Zero;
                GridRegion region=null;
                if (regionID != UUID.Zero)
                {
                    region = regiondata.Get(regionID, scopeID);
                }else if(regionName != string.Empty){
                    region = regiondata.Get(regionName, scopeID)[0];
                }
                if (region != null)
                {
                    resp["Region"] = GridRegion2WebOSD(region);
                }
            }
            return resp;
        }

        private OSDMap GetRegionNeighbours(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IRegionData regiondata = Aurora.DataManager.DataManager.RequestPlugin<IRegionData>();
            if (regiondata != null && map.ContainsKey("RegionID"))
            {
                List<GridRegion> regions = regiondata.GetNeighbours(
                    UUID.Parse(map["RegionID"].ToString()),
                    map.ContainsKey("ScopeID") ? UUID.Parse(map["ScopeID"].ToString()) : UUID.Zero,
                    map.ContainsKey("Range") ? uint.Parse(map["Range"].ToString()) : 128
                );
                OSDArray Regions = new OSDArray(regions.Count);
                foreach (GridRegion region in regions)
                {
                    Regions.Add(GridRegion2WebOSD(region));
                }
                resp["Total"] = Regions.Count;
                resp["Regions"] = Regions;
            }
            return resp;
        }

        #endregion

        #region Parcels

        private static OSDMap LandData2WebOSD(LandData parcel){
            OSDMap parcelOSD = parcel.ToOSD();
            parcelOSD["GenericData"] = parcelOSD.ContainsKey("GenericData") ? (parcelOSD["GenericData"].Type == OSDType.Map ? parcelOSD["GenericData"] : (OSDMap)OSDParser.DeserializeLLSDXml(parcelOSD["GenericData"].ToString())) : new OSDMap();
            parcelOSD["Bitmap"] = OSD.FromBinary(parcelOSD["Bitmap"]).ToString();
            parcelOSD["RegionHandle"] = OSD.FromString((parcelOSD["RegionHandle"].AsULong()).ToString());
            parcelOSD["AuctionID"] = OSD.FromInteger((int)parcelOSD["AuctionID"].AsUInteger());
            return parcelOSD;
        }

        private OSDMap GetParcelsByRegion(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Parcels"] = new OSDArray();
            resp["Total"] = OSD.FromInteger(0);

            IDirectoryServiceConnector directory = Aurora.DataManager.DataManager.RequestPlugin<IDirectoryServiceConnector>();

            if (directory != null && map.ContainsKey("Region") == true)
            {
                UUID RegionID = UUID.Parse(map["Region"]);
                UUID ScopeID = map.ContainsKey("ScopeID") ? UUID.Parse(map["ScopeID"].ToString()) : UUID.Zero;
                UUID owner = map.ContainsKey("Owner") ? UUID.Parse(map["Owner"].ToString()) : UUID.Zero;
                uint start = map.ContainsKey("Start") ? uint.Parse(map["Start"].ToString()) : 0;
                uint count = map.ContainsKey("Count") ? uint.Parse(map["Count"].ToString()) : 10;
                ParcelFlags flags = map.ContainsKey("Flags") ? (ParcelFlags)int.Parse(map["Flags"].ToString()) : ParcelFlags.None;
                ParcelCategory category = map.ContainsKey("Category") ? (ParcelCategory)uint.Parse(map["Flags"].ToString()) : ParcelCategory.Any;
                uint total = directory.GetNumberOfParcelsByRegion(RegionID, ScopeID, owner, flags, category);
                if (total > 0)
                {
                    resp["Total"] = OSD.FromInteger((int)total);
                    if(count == 0){
                        return resp;
                    }
                    List<LandData> parcels = directory.GetParcelsByRegion(start, count, RegionID, ScopeID, owner, flags, category);
                    OSDArray Parcels = new OSDArray(parcels.Count);
                    parcels.ForEach(delegate(LandData parcel)
                    {
                        Parcels.Add(LandData2WebOSD(parcel));
                    });
                    resp["Parcels"] = Parcels;
                }
            }

            return resp;
        }

        private OSDMap GetParcel(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            UUID regionID = map.ContainsKey("RegionID") ? UUID.Parse(map["RegionID"].ToString()) : UUID.Zero;
            UUID scopeID = map.ContainsKey("ScopeID") ? UUID.Parse(map["ScopeID"].ToString()) : UUID.Zero;
            UUID parcelID = map.ContainsKey("ParcelInfoUUID") ? UUID.Parse(map["ParcelInfoUUID"].ToString()) : UUID.Zero;
            string parcelName = map.ContainsKey("Parcel") ? map["Parcel"].ToString().Trim() : string.Empty;

            IDirectoryServiceConnector directory = Aurora.DataManager.DataManager.RequestPlugin<IDirectoryServiceConnector>();

            if (directory != null && (parcelID != UUID.Zero || (regionID != UUID.Zero && parcelName != string.Empty)))
            {
                LandData parcel = null;

                if(parcelID != UUID.Zero){
                    parcel = directory.GetParcelInfo(parcelID);
                }else if(regionID != UUID.Zero && parcelName != string.Empty){
                    parcel = directory.GetParcelInfo(regionID, scopeID, parcelName);
                }

                if (parcel != null)
                {
                    resp["Parcel"] = LandData2WebOSD(parcel);
                }
            }

            return resp;
        }

        #endregion

        #endregion

        #region Groups

        #region GroupRecord

        private static OSDMap GroupRecord2OSDMap(GroupRecord group)
        {
            OSDMap resp = new OSDMap();
            resp["GroupID"] = group.GroupID;
            resp["GroupName"] = group.GroupName;
            resp["AllowPublish"] = group.AllowPublish;
            resp["MaturePublish"] = group.MaturePublish;
            resp["Charter"] = group.Charter;
            resp["FounderID"] = group.FounderID;
            resp["GroupPicture"] = group.GroupPicture;
            resp["MembershipFee"] = group.MembershipFee;
            resp["OpenEnrollment"] = group.OpenEnrollment;
            resp["OwnerRoleID"] = group.OwnerRoleID;
            resp["ShowInList"] = group.ShowInList;
            return resp;
        }

        private OSDMap GetGroups(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            uint start = map.ContainsKey("Start") ? map["Start"].AsUInteger() : 0;
            resp["Start"] = start;
            resp["Total"] = 0;

            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();
            OSDArray Groups = new OSDArray();
            if (groups != null)
            {
                if (!map.ContainsKey("GroupIDs"))
                {
                    Dictionary<string, bool> sort = new Dictionary<string, bool>();
                    Dictionary<string, bool> boolFields = new Dictionary<string, bool>();

                    if (map.ContainsKey("Sort") && map["Sort"].Type == OSDType.Map)
                    {
                        OSDMap fields = (OSDMap)map["Sort"];
                        foreach (string field in fields.Keys)
                        {
                            sort[field] = int.Parse(fields[field]) != 0;
                        }
                    }
                    if (map.ContainsKey("BoolFields") && map["BoolFields"].Type == OSDType.Map)
                    {
                        OSDMap fields = (OSDMap)map["BoolFields"];
                        foreach (string field in fields.Keys)
                        {
                            boolFields[field] = int.Parse(fields[field]) != 0;
                        }
                    }
                    List<GroupRecord> reply = groups.GetGroupRecords(
                        AdminAgentID,
                        start,
                        map.ContainsKey("Count") ? map["Count"].AsUInteger() : 10,
                        sort,
                        boolFields
                    );
                    if (reply.Count > 0)
                    {
                        foreach (GroupRecord groupReply in reply)
                        {
                            Groups.Add(GroupRecord2OSDMap(groupReply));
                        }
                    }
                    resp["Total"] = groups.GetNumberOfGroups(AdminAgentID, boolFields);
                }
                else
                {
                    OSDArray groupIDs = (OSDArray)map["Groups"];
                    List<UUID> GroupIDs = new List<UUID>();
                    foreach (string groupID in groupIDs)
                    {
                        UUID foo;
                        if (UUID.TryParse(groupID, out foo))
                        {
                            GroupIDs.Add(foo);
                        }
                    }
                    if (GroupIDs.Count > 0)
                    {
                        List<GroupRecord> reply = groups.GetGroupRecords(AdminAgentID, GroupIDs);
                        if (reply.Count > 0)
                        {
                            foreach (GroupRecord groupReply in reply)
                            {
                                Groups.Add(GroupRecord2OSDMap(groupReply));
                            }
                        }
                        resp["Total"] = Groups.Count;
                    }
                }
            }

            resp["Groups"] = Groups;
            return resp;
        }

        private OSDMap GetNewsSources(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            uint start = map.ContainsKey("Start") ? map["Start"].AsUInteger() : 0;
            uint count = map.ContainsKey("Count") ? map["Count"].AsUInteger() : 10;
            IGenericsConnector generics = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();

            if (generics == null)
            {
                resp["Failed"] = new OSDString("Could not find IGenericsConnector");
            }
            else if (groups == null)
            {
                resp["Failed"] = new OSDString("Could not find IGroupsServiceConnector");
            }
            else
            {
                OSDMap useValue = new OSDMap();
                useValue["Use"] = OSD.FromBoolean(true);
                List<UUID> GroupIDs = generics.GetOwnersByGeneric("Group", "WebUI_newsSource", useValue);
                resp["Total"] = GroupIDs.Count;
                resp["Start"] = (int)start;
                resp["Count"] = (int)count;

                OSDArray Groups = new OSDArray();
                if (start < GroupIDs.Count)
                {
                    int end = (int)count;
                    if (start + count > GroupIDs.Count)
                    {
                        end = GroupIDs.Count - (int)start;
                    }
                    List<UUID> page = GroupIDs.GetRange((int)start, end);
                    if (page.Count > 0)
                    {
                        List<GroupRecord> reply = groups.GetGroupRecords(AdminAgentID, page);
                        if (reply.Count > 0)
                        {
                            foreach (GroupRecord groupReply in reply)
                            {
                                Groups.Add(GroupRecord2OSDMap(groupReply));
                            }
                        }
                    }
                }
                resp["Groups"] = Groups;
            }


            return resp;
        }

        private OSDMap GetGroup(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();
            resp["Group"] = false;
            if (groups != null && (map.ContainsKey("Name") || map.ContainsKey("UUID")))
            {
                UUID groupID = map.ContainsKey("UUID") ? UUID.Parse(map["UUID"].ToString()) : UUID.Zero;
                string name = map.ContainsKey("Name") ? map["Name"].ToString() : "";
                GroupRecord reply = groups.GetGroupRecord(AdminAgentID, groupID, name);
                if (reply != null)
                {
                    resp["Group"] = GroupRecord2OSDMap(reply);
                }
            }
            return resp;
        }

        #endregion

        #region GroupNoticeData

        private OSDMap GroupAsNewsSource(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["Verified"] = OSD.FromBoolean(false);
            IGenericsConnector generics = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            UUID groupID;
            if (generics != null && map.ContainsKey("Group") == true && map.ContainsKey("Use") && UUID.TryParse(map["Group"], out groupID) == true)
            {
                if (map["Use"].AsBoolean())
                {
                    OSDMap useValue = new OSDMap();
                    useValue["Use"] = OSD.FromBoolean(true);
                    generics.AddGeneric(groupID, "Group", "WebUI_newsSource", useValue);
                }
                else
                {
                    generics.RemoveGeneric(groupID, "Group", "WebUI_newsSource");
                }
                resp["Verified"] = OSD.FromBoolean(true);
            }
            return resp;
        }

        private OSDMap GroupNotices(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["GroupNotices"] = new OSDArray();
            resp["Total"] = 0;
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();

            if (map.ContainsKey("Groups") && groups != null && map["Groups"].Type.ToString() == "Array")
            {
                OSDArray groupIDs = (OSDArray)map["Groups"];
                List<UUID> GroupIDs = new List<UUID>();
                foreach (string groupID in groupIDs)
                {
                    UUID foo;
                    if (UUID.TryParse(groupID, out foo))
                    {
                        GroupIDs.Add(foo);
                    }
                }
                if (GroupIDs.Count > 0)
                {
                    uint start = map.ContainsKey("Start") ? uint.Parse(map["Start"]) : 0;
                    uint count = map.ContainsKey("Count") ? uint.Parse(map["Count"]) : 10;
                    List<GroupNoticeData> groupNotices = groups.GetGroupNotices(AdminAgentID, start, count, GroupIDs);
                    OSDArray GroupNotices = new OSDArray(groupNotices.Count);
                    groupNotices.ForEach(delegate(GroupNoticeData GND)
                    {
                        OSDMap gnd = new OSDMap();
                        gnd["GroupID"] = OSD.FromUUID(GND.GroupID);
                        gnd["NoticeID"] = OSD.FromUUID(GND.NoticeID);
                        gnd["Timestamp"] = OSD.FromInteger((int)GND.Timestamp);
                        gnd["FromName"] = OSD.FromString(GND.FromName);
                        gnd["Subject"] = OSD.FromString(GND.Subject);
                        gnd["HasAttachment"] = OSD.FromBoolean(GND.HasAttachment);
                        gnd["ItemID"] = OSD.FromUUID(GND.ItemID);
                        gnd["AssetType"] = OSD.FromInteger((int)GND.AssetType);
                        gnd["ItemName"] = OSD.FromString(GND.ItemName);
                        GroupNoticeInfo notice = groups.GetGroupNotice(AdminAgentID, GND.NoticeID);
                        gnd["Message"] = OSD.FromString(notice.Message);
                        GroupNotices.Add(gnd);
                    });
                    resp["GroupNotices"] = GroupNotices;
                    resp["Total"] = (int)groups.GetNumberOfGroupNotices(AdminAgentID, GroupIDs);
                }
            }

            return resp;
        }

        private OSDMap NewsFromGroupNotices(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            resp["GroupNotices"] = new OSDArray();
            resp["Total"] = 0;
            IGenericsConnector generics = Aurora.DataManager.DataManager.RequestPlugin<IGenericsConnector>();
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();
            if (generics == null || groups == null)
            {
                return resp;
            }
            OSDMap useValue = new OSDMap();
            useValue["Use"] = OSD.FromBoolean(true);
            List<UUID> GroupIDs = generics.GetOwnersByGeneric("Group", "WebUI_newsSource", useValue);
            if (GroupIDs.Count <= 0)
            {
                return resp;
            }
            foreach (UUID groupID in GroupIDs)
            {
                GroupRecord group = groups.GetGroupRecord(AdminAgentID, groupID, "");
                if (!group.ShowInList)
                {
                    GroupIDs.Remove(groupID);
                }
            }

            uint start = map.ContainsKey("Start") ? uint.Parse(map["Start"].ToString()) : 0;
            uint count = map.ContainsKey("Count") ? uint.Parse(map["Count"].ToString()) : 10;

            OSDMap args = new OSDMap();
            args["Start"] = OSD.FromString(start.ToString());
            args["Count"] = OSD.FromString(count.ToString());
            args["Groups"] = new OSDArray(GroupIDs.ConvertAll(x=>OSD.FromString(x.ToString())));

            return GroupNotices(args);
        }

        private OSDMap GetGroupNotice(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            UUID noticeID = map.ContainsKey("NoticeID") ? UUID.Parse(map["NoticeID"]) : UUID.Zero;
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();

            if (noticeID != UUID.Zero && groups != null)
            {
                GroupNoticeData GND = groups.GetGroupNoticeData(AdminAgentID, noticeID);
                if (GND != null)
                {
                    OSDMap gnd = new OSDMap();
                    gnd["GroupID"] = OSD.FromUUID(GND.GroupID);
                    gnd["NoticeID"] = OSD.FromUUID(GND.NoticeID);
                    gnd["Timestamp"] = OSD.FromInteger((int)GND.Timestamp);
                    gnd["FromName"] = OSD.FromString(GND.FromName);
                    gnd["Subject"] = OSD.FromString(GND.Subject);
                    gnd["HasAttachment"] = OSD.FromBoolean(GND.HasAttachment);
                    gnd["ItemID"] = OSD.FromUUID(GND.ItemID);
                    gnd["AssetType"] = OSD.FromInteger((int)GND.AssetType);
                    gnd["ItemName"] = OSD.FromString(GND.ItemName);
                    GroupNoticeInfo notice = groups.GetGroupNotice(AdminAgentID, GND.NoticeID);
                    gnd["Message"] = OSD.FromString(notice.Message);

                    resp["GroupNotice"] = gnd;
                }
            }

            return resp;
        }

        private OSDMap EditGroupNotice(OSDMap map)
        {
            OSDMap resp = new OSDMap();
            UUID noticeID = map.ContainsKey("NoticeID") ? UUID.Parse(map["NoticeID"]) : UUID.Zero;
            IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();
            GroupNoticeData GND = noticeID != UUID.Zero && groups != null ? groups.GetGroupNoticeData(AdminAgentID, noticeID) : null;
            GroupNoticeInfo notice = GND != null ? groups.GetGroupNotice(AdminAgentID, GND.NoticeID) : null;

            if (noticeID == UUID.Zero)
            {
                resp["Failed"] = new OSDString("No notice ID was specified");
            }
            else if (groups == null)
            {
                resp["Failed"] = new OSDString("Could not find IGroupsServiceConnector");
            }
            else if (GND == null || notice == null)
            {
                resp["Failed"] = new OSDString("Could not find group notice with specified ID");
            }
            else if (!map.ContainsKey("Subject") && !map.ContainsKey("Message"))
            {
                resp["Success"] = new OSDBoolean(false);
                resp["Note"] = new OSDString("No changes were made to the group notice");
            }
            else
            {
                resp["Success"] = groups.EditGroupNotice(AdminAgentID, notice.GroupID, GND.NoticeID, map.ContainsKey("Subject") ? map["Subject"].ToString() : GND.Subject, map.ContainsKey("Message") ? map["Message"].ToString() : notice.Message);
            }

            return resp;
        }

        private OSDMap AddGroupNotice(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            if (!map.ContainsKey("GroupID") || !map.ContainsKey("AuthorID") || !map.ContainsKey("Subject") || !map.ContainsKey("Message"))
            {
                resp["Failed"] = new OSDString("Missing required arguments one or more of GroupID, AuthorID, Subject, Message");
            }
            else
            {
                UUID GroupID = UUID.Zero;
                UUID.TryParse(map["GroupID"].ToString(), out GroupID);

                UUID AuthorID = UUID.Zero;
                UUID.TryParse(map["AuthorID"].ToString(), out AuthorID);

                string subject = map["Subject"].ToString().Trim();
                string message = map["Message"].ToString().Trim();

                IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();
                IUserAccountService users = m_registry.RequestModuleInterface<IUserAccountService>();
                UserAccount Author = AuthorID != UUID.Zero && users != null ? users.GetUserAccount(UUID.Zero, AuthorID) : null;

                if (GroupID == UUID.Zero)
                {
                    resp["Failed"] = new OSDString("GroupID was UUID.Zero");
                }
                else if (AuthorID == UUID.Zero)
                {
                    resp["Failed"] = new OSDString("AuthorID was UUID.Zero");
                }
                else if (subject == string.Empty)
                {
                    resp["Failed"] = new OSDString("Subject was empty");
                }
                else if (message == string.Empty)
                {
                    resp["Failed"] = new OSDString("Message was empty");
                }
                else if (groups == null)
                {
                    resp["Failed"] = new OSDString("Could not findIGroupsServiceConnector");
                }
                else if (users == null)
                {
                    resp["Failed"] = new OSDString("Could not find IUserAccountService");
                }
                else if (Author == null)
                {
                    resp["Failed"] = new OSDString(string.Format("Could not find author with ID {0}", AuthorID));
                }
                else
                {
                    UUID noticeID = UUID.Random();
                    try
                    {
                        groups.AddGroupNotice(AuthorID, GroupID, noticeID, Author.Name, subject, message, UUID.Zero, 0, "");
                        resp["NoticeID"] = noticeID;
                    }
                    catch
                    {
                        resp["Failed"] = new OSDString("An exception was thrown.");
                    }
                }
            }

            return resp;
        }

        private OSDMap RemoveGroupNotice(OSDMap map)
        {
            OSDMap resp = new OSDMap();

            if (!map.ContainsKey("GroupID") || !map.ContainsKey("NoticeID"))
            {
                resp["Failed"] = new OSDString("Missing required arguments one or more of GroupID, NoticeID");
            }
            else
            {
                UUID GroupID = UUID.Zero;
                UUID.TryParse(map["GroupID"].ToString(), out GroupID);

                UUID noticeID = UUID.Zero;
                UUID.TryParse(map["NoticeID"].ToString(), out noticeID);

                IGroupsServiceConnector groups = Aurora.DataManager.DataManager.RequestPlugin<IGroupsServiceConnector>();

                if (GroupID == UUID.Zero)
                {
                    resp["Failed"] = new OSDString("GroupID was UUID.Zero");
                }
                else if (noticeID == UUID.Zero)
                {
                    resp["Failed"] = new OSDString("NoticeID was UUID.Zero");
                }
                else if (groups == null)
                {
                    resp["Failed"] = new OSDString("Could not findIGroupsServiceConnector");
                }
                else
                {
                    try
                    {
                        resp["Success"] = groups.RemoveGroupNotice(AdminAgentID, GroupID, noticeID);
                    }
                    catch
                    {
                        resp["Failed"] = new OSDString("An exception was thrown.");
                    }
                }
            }

            return resp;
        }

        #endregion

        #endregion

        #region Events

        private OSDMap GetEvents(OSDMap map)
        {
            uint start = map.ContainsKey("Start") ? map["Start"].AsUInteger() : 0;
            uint count = map.ContainsKey("Count") ? map["Count"].AsUInteger() : 0;
            Dictionary<string, bool> sort = new Dictionary<string, bool>();
            Dictionary<string, object> filter = new Dictionary<string, object>();

            OSDMap resp = new OSDMap();
            resp["Start"] = start;
            resp["Total"] = 0;
            resp["Events"] = new OSDArray(0);

            IDirectoryServiceConnector directory = Aurora.DataManager.DataManager.RequestPlugin<IDirectoryServiceConnector>();
            if (directory != null)
            {
                if (map.ContainsKey("Filter") && map["Filter"].Type == OSDType.Map)
                {
                    OSDMap fields = (OSDMap)map["Filter"];
                    foreach (string field in fields.Keys)
                    {
                        filter[field] = fields[field];
                    }
                }
                if (count > 0)
                {
                    if (map.ContainsKey("Sort") && map["Sort"].Type == OSDType.Map)
                    {
                        OSDMap fields = (OSDMap)map["Sort"];
                        foreach (string field in fields.Keys)
                        {
                            sort[field] = int.Parse(fields[field]) != 0;
                        }
                    }
                    
                    OSDArray Events = new OSDArray();
                    directory.GetEvents(start, count, sort, filter).ForEach(delegate(EventData Event){
                        Events.Add(Event.ToOSD());
                    });
                    resp["Events"] = Events;
                }
                resp["Total"] = (int)directory.GetNumberOfEvents(filter);
            }

            return resp;
        }

        private OSDMap CreateEvent(OSDMap map)
        {
            OSDMap resp = new OSDMap(1);

            IDirectoryServiceConnector directory = Aurora.DataManager.DataManager.RequestPlugin<IDirectoryServiceConnector>();
            if (directory != null && (
                map.ContainsKey("Creator") && 
                map.ContainsKey("Region") && 
                map.ContainsKey("Date") && 
                map.ContainsKey("Cover") && 
                map.ContainsKey("Maturity") && 
                map.ContainsKey("EventFlags") && 
                map.ContainsKey("Duration") && 
                map.ContainsKey("Position") && 
                map.ContainsKey("Name") && 
                map.ContainsKey("Description") && 
                map.ContainsKey("Category")
            )){
                EventData eventData = directory.CreateEvent(
                    map["Creator"].AsUUID(),
                    map["Region"].AsUUID(),
                    map.ContainsKey("Parcel") ? map["Parcel"].AsUUID() : UUID.Zero,
                    map["Date"].AsDate(),
                    map["Cover"].AsUInteger(),
                    (EventFlags)map["Maturity"].AsUInteger(),
                    map["EventFlags"].AsUInteger() | map["Maturity"].AsUInteger(),
                    map["Duration"].AsUInteger(),
                    Vector3.Parse(map["Position"].AsString()),
                    map["Name"].AsString(),
                    map["Description"].AsString(),
                    map["Category"].AsString()
                );

                if (eventData != null)
                {
                    resp["Event"] = eventData.ToOSD();
                }
            }

            return resp;
        }

        #endregion

        #region Textures

        private OSDMap SizeOfHTTPGetTextureImage(OSDMap map)
        {
            OSDMap resp = new OSDMap(1);
            resp["Size"] = OSD.FromUInteger(0);

            if (map.ContainsKey("Texture"))
            {
                Hashtable args = new Hashtable(2);
                args["method"] = "GridTexture";
                args["uuid"] = UUID.Parse(map["Texture"].ToString());
                Hashtable texture = WebUI.OnHTTPGetTextureImage(args);
                if (texture.ContainsKey("str_response_string"))
                {
                    resp["Size"] = OSD.FromInteger(Convert.FromBase64String(texture["str_response_string"].ToString()).Length);
                }
            }

            return resp;
        }

        #endregion

        #endregion
    }
}
