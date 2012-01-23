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
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OpenMetaverse;
using Aurora.DataManager;
using Aurora.Framework;
using Nini.Config;

namespace Aurora.Services.DataService
{
    public class LocalWebStatsDataConnector : IWebStatsDataConnector
	{
        private IGenericData GD = null;

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore simBase, string defaultConnectionString)
        {
            if (source.Configs["AuroraConnectors"].GetString("WebStatsDataConnector", "LocalConnector") == "LocalConnector")
            {
                GD = GenericData;

                if (source.Configs[Name] != null)
                    defaultConnectionString = source.Configs[Name].GetString("ConnectionString", defaultConnectionString);

                GD.ConnectToDatabase(defaultConnectionString, "WebStats", source.Configs["AuroraConnectors"].GetBoolean("ValidateTables", true));

                DataManager.DataManager.RegisterPlugin(Name, this);
            }
        }

        public string Name
        {
            get { return "IWebStatsDataConnector"; }
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Add/Update a user's stats in the database
        /// </summary>
        /// <param name="uid"></param>
        public void UpdateUserStats(UserSessionID uid)
        {
            if (uid.session_id == UUID.Zero)
            {
                return;
            }

            List<string> Keys = new List<string>();
            List<object> Values = new List<object>();

            Dictionary<string, object> row = new Dictionary<string, object>(46);

            row["session_id"] = uid.session_data.session_id;
            row["agent_id"] = uid.session_data.agent_id;
            row["region_id"] = uid.session_data.region_id;
            row["last_updated"] = uid.session_data.last_updated;
            row["remote_ip"] = uid.session_data.remote_ip;
            row["name_f"] = uid.session_data.name_f;
            row["name_l"] = uid.session_data.name_l;
            row["avg_agents_in_view"] = uid.session_data.avg_agents_in_view;
            row["min_agents_in_view"] = uid.session_data.min_agents_in_view;
            row["max_agents_in_view"] = uid.session_data.max_agents_in_view;
            row["mode_agents_in_view"] = uid.session_data.mode_agents_in_view;
            row["avg_fps"] = uid.session_data.avg_fps;
            row["min_fps"] = uid.session_data.min_fps;
            row["max_fps"] = uid.session_data.max_fps;
            row["mode_fps"] = uid.session_data.mode_fps;
            row["a_language"] = uid.session_data.a_language;
            row["mem_use"] = uid.session_data.mem_use;
            row["meters_traveled"] = uid.session_data.meters_traveled;
            row["avg_ping"] = uid.session_data.avg_ping;
            row["min_ping"] = uid.session_data.min_ping;
            row["max_ping"] = uid.session_data.max_ping;
            row["mode_ping"] = uid.session_data.mode_ping;
            row["regions_visited"] = uid.session_data.regions_visited;
            row["run_time"] = uid.session_data.run_time;
            row["avg_sim_fps"] = uid.session_data.avg_sim_fps;
            row["min_sim_fps"] = uid.session_data.min_sim_fps;
            row["max_sim_fps"] = uid.session_data.max_sim_fps;
            row["mode_sim_fps"] = uid.session_data.mode_sim_fps;
            row["start_time"] = uid.session_data.start_time;
            row["client_version"] = uid.session_data.client_version;
            row["s_cpu"] = uid.session_data.s_cpu;
            row["s_gpu"] = uid.session_data.s_gpu;
            row["s_os"] = uid.session_data.s_os;
            row["s_ram"] = uid.session_data.s_ram;
            row["d_object_kb"] = uid.session_data.d_object_kb;
            row["d_texture_kb"] = uid.session_data.d_texture_kb;
            row["n_in_kb"] = uid.session_data.n_in_kb;
            row["n_in_pk"] = uid.session_data.n_in_pk;
            row["n_out_kb"] = uid.session_data.n_out_kb;
            row["n_out_pk"] = uid.session_data.n_out_pk;
            row["f_dropped"] = uid.session_data.f_dropped;
            row["f_failed_resends"] = uid.session_data.f_failed_resends;
            row["f_invalid"] = uid.session_data.f_invalid;
            row["f_off_circuit"] = uid.session_data.f_off_circuit;
            row["f_resent"] = uid.session_data.f_resent;
            row["f_send_packet"] = uid.session_data.f_send_packet;

            GD.Replace("stats_session_data", row);
        }

        /// <summary>
        /// Get info on the sim status
        /// </summary>
        /// <returns></returns>
        public stats_default_page_values GetDefaultPageStats()
        {
            stats_default_page_values stats = new stats_default_page_values();
            List<string> retStr = GD.Query(new string[10]{
                "COUNT(DISTINCT agent_id) as agents",
                "COUNT(*) as sessions",
                "AVG(avg_fps) as client_fps",
                "AVG(avg_sim_fps) as savg_sim_fps",
                "AVG(avg_ping) as sav_ping",
                "SUM(n_out_kb) as num_in_kb",
                "SUM(n_out_pk) as num_in_packets",
                "SUM(n_in_kb) as num_out_kb",
                "SUM(n_in_pk) as num_out_packets",
                "AVG(mem_use) as sav_mem_use"
            }, "stats_session_data", null, null, null, null);

            if (retStr.Count == 0)
                return stats;

            for (int i = 0; i < retStr.Count; i += 8)
            {

                stats.total_num_users = Convert.ToInt32(retStr[i]);
                stats.total_num_sessions = Convert.ToInt32(retStr[i + 1]);
                stats.avg_client_fps = Convert.ToSingle(retStr[i + 2]);
                stats.avg_sim_fps = Convert.ToSingle(retStr[i + 3]);
                stats.avg_ping = Convert.ToSingle(retStr[i + 4]);
                stats.total_kb_out = Convert.ToSingle(retStr[i + 5]);
                stats.total_kb_in = Convert.ToSingle(retStr[i + 6]);
                stats.avg_client_mem_use = Convert.ToSingle(retStr[i + 7]);

            }
            return stats;
        }

        /// <summary>
        /// Get info on all clients that are in the region
        /// </summary>
        /// <returns></returns>
        public List<ClientVersionData> GetClientVersions()
        {
            List<ClientVersionData> clients = new List<ClientVersionData>();

            List<string> retStr = GD.Query(new string[1]{
                "count(distinct region_id) as regcnt"
            }, "stats_session_data", null, null, null, null);

            if (retStr.Count == 0)
                return clients;

            int totalregions = totalregions = Convert.ToInt32(retStr[0]);
            int totalclients = 0;
            if (totalregions > 1)
            {
                retStr = GD.QueryFullData(" group by region_id, client_version order by region_id, count(*) desc;",
                "stats_session_data",
                "region_id, client_version, count(*) as cnt, avg(avg_sim_fps) as simfps");

                for (int i = 0; i < retStr.Count; i += 4)
                {
                    ClientVersionData udata = new ClientVersionData();
                    udata.region_id = UUID.Parse(retStr[i]);
                    udata.version = retStr[i + 1];
                    udata.count = int.Parse(retStr[i + 2]);
                    udata.fps = Convert.ToSingle(retStr[i + 3]);
                    clients.Add(udata);
                }
            }
            else
            {
                retStr = GD.QueryFullData(" group by region_id, client_version order by region_id, count(*) desc;",
                    "stats_session_data",
                    "region_id, client_version, count(*) as cnt, avg(avg_sim_fps) as simfps");

                for (int i = 0; i < retStr.Count; i += 4)
                {
                    ClientVersionData udata = new ClientVersionData();
                    udata.region_id = UUID.Parse(retStr[i]);
                    udata.version = retStr[i + 1];
                    udata.count = int.Parse(retStr[i + 2]);
                    udata.fps = Convert.ToSingle(retStr[i + 3]);
                    clients.Add(udata);
                    totalclients += udata.count;
                }
            }

            return clients;
        }

        /// <summary>
        /// Get a list of all the client sessions in the region
        /// </summary>
        /// <param name="puserUUID"></param>
        /// <param name="clientVersionString"></param>
        /// <returns></returns>
        public List<SessionList> GetSessionList(string puserUUID, string clientVersionString)
        {
            List<SessionList> sessionList = new List<SessionList>();
            string sql = " a LEFT OUTER JOIN stats_session_data b ON a.Agent_ID = b.Agent_ID";
            int queryparams = 0;

            if (puserUUID.Length > 0)
            {
                if (queryparams == 0)
                    sql += " WHERE";
                else
                    sql += " AND";

                sql += " b.agent_id='" + puserUUID + "'";
                queryparams++;
            }

            if (clientVersionString.Length > 0)
            {
                if (queryparams == 0)
                    sql += " WHERE";
                else
                    sql += " AND";

                sql += " b.client_version='" + clientVersionString + "'";
                queryparams++;
            }

            sql += " ORDER BY a.name_f, a.name_l, b.last_updated;";

            IDataReader sdr = GD.QueryData (sql,
                "stats_session_data",
                "distinct a.name_f, a.name_l, a.Agent_ID, b.Session_ID, b.client_version, b.last_updated, b.start_time");
            if (sdr != null && sdr.FieldCount != 0)
            {
                UUID userUUID = UUID.Zero;

                SessionList activeSessionList = new SessionList();
                activeSessionList.user_id = UUID.Random();
                while (sdr.Read())
                {
                    UUID readUUID = UUID.Parse(sdr["agent_id"].ToString());
                    if (readUUID != userUUID)
                    {
                        activeSessionList = new SessionList();
                        activeSessionList.user_id = readUUID;
                        activeSessionList.firstname = sdr["name_f"].ToString();
                        activeSessionList.lastname = sdr["name_l"].ToString();
                        activeSessionList.sessions = new List<ShortSessionData>();
                        sessionList.Add(activeSessionList);
                    }

                    ShortSessionData ssd = new ShortSessionData();

                    ssd.last_update = Utils.UnixTimeToDateTime((uint)Convert.ToInt32(sdr["last_updated"]));
                    ssd.start_time = Utils.UnixTimeToDateTime((uint)Convert.ToInt32(sdr["start_time"]));
                    ssd.session_id = UUID.Parse(sdr["session_id"].ToString());
                    ssd.client_version = sdr["client_version"].ToString();
                    activeSessionList.sessions.Add(ssd);

                    userUUID = activeSessionList.user_id;
                }
            }
            try
            {
                if (sdr != null)
                {
                    sdr.Close ();
                    sdr.Dispose ();
                }
            }
            catch { }

            return sessionList;
        }
	}
}
