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
using System.Reflection;
using System.Text;
using Mono.Data.SqliteClient;
using OpenMetaverse;
using OpenSim.Region.Framework.Scenes;
using Aurora.Framework;
using OpenSim.Framework;

namespace OpenSim.Region.UserStatistics
{
    public class Default_Report : IStatsController
    {
        public string ReportName
        {
            get { return "Home"; }
        }

        #region IStatsController Members

        public Hashtable ProcessModel(Hashtable pParams)
        {
            List<IScene> m_scene = (List<IScene>)pParams["Scenes"];

            stats_default_page_values mData = rep_DefaultReport_data(m_scene);
            mData.sim_stat_data = (Dictionary<UUID,USimStatsData>)pParams["SimStats"];
            mData.stats_reports = (Dictionary<string, IStatsController>) pParams["Reports"];

            Hashtable nh = new Hashtable();
            nh.Add("hdata", mData);
            nh.Add("Reports", pParams["Reports"]);
            
            return nh;
        }

        public string RenderView(Hashtable pModelResult)
        {
            stats_default_page_values mData = (stats_default_page_values) pModelResult["hdata"];
            return rep_Default_report_view(mData);
        }

        #endregion

        public string rep_Default_report_view(stats_default_page_values values)
        {
            StringBuilder output = new StringBuilder();



            const string TableClass = "defaultr";
            const string TRClass = "defaultr";
            const string TDHeaderClass = "header";
            const string TDDataClass = "content";
            //const string TDDataClassRight = "contentright";
            const string TDDataClassCenter = "contentcenter";

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
            HTMLUtil.HtmlHeaders_O(ref output);
            
            HTMLUtil.InsertProtoTypeAJAX(ref output);
            string[] ajaxUpdaterDivs = new string[3];
            int[] ajaxUpdaterSeconds = new int[3];
            string[] ajaxUpdaterReportFragments = new string[3];

            ajaxUpdaterDivs[0] = "activeconnections";
            ajaxUpdaterSeconds[0] = 10;
            ajaxUpdaterReportFragments[0] = "activeconnectionsajax.html";

            ajaxUpdaterDivs[1] = "activesimstats";
            ajaxUpdaterSeconds[1] = 20;
            ajaxUpdaterReportFragments[1] = "simstatsajax.html";

            ajaxUpdaterDivs[2] = "activelog";
            ajaxUpdaterSeconds[2] = 5;
            ajaxUpdaterReportFragments[2] = "activelogajax.html";

            HTMLUtil.InsertPeriodicUpdaters(ref output, ajaxUpdaterDivs, ajaxUpdaterSeconds, ajaxUpdaterReportFragments);
            
            output.Append(STYLESHEET);
            HTMLUtil.HtmlHeaders_C(ref output);
            HTMLUtil.AddReportLinks(ref output, values.stats_reports, "");
            HTMLUtil.TABLE_O(ref output, TableClass);
            HTMLUtil.TR_O(ref output, TRClass);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("# Users Total");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("# Sessions Total");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("Avg Client FPS");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("Avg Client Mem Use");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("Avg Sim FPS");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("Avg Ping");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("KB Out Total");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDHeaderClass);
            output.Append("KB In Total");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TR_C(ref output);
            HTMLUtil.TR_O(ref output, TRClass);
            HTMLUtil.TD_O(ref output, TDDataClass);
            output.Append(values.total_num_users);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClass);
            output.Append(values.total_num_sessions);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.avg_client_fps);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.avg_client_mem_use);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.avg_sim_fps);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.avg_ping);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.total_kb_out);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, TDDataClassCenter);
            output.Append(values.total_kb_in);
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TR_C(ref output);
            HTMLUtil.TABLE_C(ref output);

            HTMLUtil.HR(ref output, "");
            HTMLUtil.TABLE_O(ref output, "");
            HTMLUtil.TR_O(ref output, "");
            HTMLUtil.TD_O(ref output, "align_top");
            output.Append("<DIV id=\"activeconnections\">Active Connections loading...</DIV>");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TD_O(ref output, "align_top");
            output.Append("<DIV id=\"activesimstats\">SimStats loading...</DIV>");
            output.Append("<DIV id=\"activelog\">ActiveLog loading...</DIV>");
            HTMLUtil.TD_C(ref output);
            HTMLUtil.TR_C(ref output);
            HTMLUtil.TABLE_C(ref output);
            output.Append("</BODY></HTML>");
            // TODO: FIXME: template
            return output.ToString();
        }
        public stats_default_page_values rep_DefaultReport_data(List<IScene> m_scene)
        {
            stats_default_page_values returnstruct = new stats_default_page_values();
            returnstruct.all_scenes = m_scene.ToArray();
            return returnstruct;
        }
    }
}
