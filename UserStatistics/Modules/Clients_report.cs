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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mono.Data.SqliteClient;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;
using Aurora.Framework;
using OpenSim.Framework;

namespace OpenSim.Region.UserStatistics
{
    public class Clients_report : IStatsController
    {
        #region IStatsController Members

        public string ReportName
        {
            get { return "Client"; }
        }

        public Hashtable ProcessModel(Hashtable pParams)
        {
            List<ClientVersionData> clidata = new List<ClientVersionData>();
            List<ClientVersionData> cliRegData = new List<ClientVersionData>();
            Hashtable regionTotals = new Hashtable();

            Hashtable modeldata = new Hashtable();
            modeldata.Add("Scenes", pParams["Scenes"]);
            modeldata.Add("Reports", pParams["Reports"]);
            int totalclients = 0;

            cliRegData = Aurora.DataManager.DataManager.RequestPlugin<IWebStatsDataConnector>().GetClientVersions();
            
            foreach (ClientVersionData cvd in cliRegData)
            {
                if (regionTotals.ContainsKey(cvd.region_id))
                {
                    int regiontotal = (int)regionTotals[cvd.region_id];
                    regiontotal += cvd.count;
                    regionTotals[cvd.region_id] = regiontotal;
                }
                else
                {
                    regionTotals.Add(cvd.region_id, cvd.count);
                }
            }

            modeldata["ClientData"] = clidata;
            modeldata["ClientRegionData"] = cliRegData;
            modeldata["RegionTotals"] = regionTotals;
            modeldata["Total"] = totalclients;

            return modeldata;
        }

        public string RenderView(Hashtable pModelResult)
        {
            List<ClientVersionData> clidata = (List<ClientVersionData>) pModelResult["ClientData"];
            int totalclients = (int)pModelResult["Total"];
            Hashtable regionTotals = (Hashtable) pModelResult["RegionTotals"];
            List<ClientVersionData> cliRegData = (List<ClientVersionData>) pModelResult["ClientRegionData"];
            List<IScene> m_scenes = (List<IScene>)pModelResult["Scenes"];
            Dictionary<string, IStatsController> reports = (Dictionary<string, IStatsController>)pModelResult["Reports"];

            const string STYLESHEET =
    @"
<STYLE>
body
{
    font-size:15px; font-family:Helvetica, Verdana; color:Black;
}
TABLE.defaultr { }
TR.defaultr { padding: 5px; }
TD.header { font-weight:bold; padding:5px; }
TD.content {}
TD.contentright { text-align: right; }
TD.contentcenter { text-align: center; }
TD.align_top { vertical-align: top; }
</STYLE>
";

            StringBuilder output = new StringBuilder();
            HTMLUtil.HtmlHeaders_O(ref output);
            output.Append(STYLESHEET);
            HTMLUtil.HtmlHeaders_C(ref output);

            HTMLUtil.AddReportLinks(ref output, reports, "");

            HTMLUtil.TABLE_O(ref output, "defaultr");
            HTMLUtil.TR_O(ref output, "");
            HTMLUtil.TD_O(ref output, "header");
            output.Append("ClientVersion");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, "header");
            output.Append("Count/%");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, "header");
            output.Append("SimFPS");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TR_C(ref output);

            foreach (ClientVersionData cvd in clidata)
            {
                HTMLUtil.TR_O(ref output, "");
                HTMLUtil.TD_O(ref output, "content");
                string linkhref = "sessions.report?VersionString=" + cvd.version;
                HTMLUtil.A(ref output, cvd.version, linkhref, "");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TD_O(ref output, "content");
                output.Append(cvd.count);
                output.Append("/");
                if (totalclients > 0)
                    output.Append((((float)cvd.count / (float)totalclients)*100).ToString());
                else
                    output.Append(0);

                output.Append("%");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TD_O(ref output, "content");
                output.Append(cvd.fps);
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TR_C(ref output);
            }
            HTMLUtil.TABLE_C(ref output);

            if (cliRegData.Count > 0)
            {
                HTMLUtil.TABLE_O(ref output, "defaultr");
                HTMLUtil.TR_O(ref output, "");
                HTMLUtil.TD_O(ref output, "header");
                output.Append("Region");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TD_O(ref output, "header");
                output.Append("ClientVersion");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TD_O(ref output, "header");
                output.Append("Count/%");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TD_O(ref output, "header");
                output.Append("SimFPS");
                HTMLUtil.TD_C(ref output);
                HTMLUtil.TR_C(ref output);

                foreach (ClientVersionData cvd in cliRegData)
                {
                    HTMLUtil.TR_O(ref output, "");
                    HTMLUtil.TD_O(ref output, "content");
                    output.Append(regionNamefromUUID(m_scenes, cvd.region_id));
                    HTMLUtil.TD_C(ref output);
                    HTMLUtil.TD_O(ref output, "content");
                    output.Append(cvd.version);
                    HTMLUtil.TD_C(ref output);
                    HTMLUtil.TD_O(ref output, "content");
                    output.Append(cvd.count);
                    output.Append("/");
                    if ((int)regionTotals[cvd.region_id] > 0)
                        output.Append((((float)cvd.count / (float)((int)regionTotals[cvd.region_id])) * 100).ToString());
                    else
                        output.Append(0);

                    output.Append("%");
                    HTMLUtil.TD_C(ref output);
                    HTMLUtil.TD_O(ref output, "content");
                    output.Append(cvd.fps);
                    HTMLUtil.TD_C(ref output);
                    HTMLUtil.TR_C(ref output);
                }
                HTMLUtil.TABLE_C(ref output);

            }

            output.Append("</BODY>");
            output.Append("</HTML>");
            return output.ToString();
        }
        public string regionNamefromUUID(List<IScene> scenes, UUID region_id)
        {
            string returnstring = string.Empty;
            foreach (IScene sn in scenes)
            {
                if (region_id == sn.RegionInfo.RegionID)
                {
                    returnstring = sn.RegionInfo.RegionName;
                    break;
                }
            }

            if (returnstring.Length == 0)
            {
                returnstring = region_id.ToString();
            }

            return returnstring;
        }

        #endregion
    }
}
