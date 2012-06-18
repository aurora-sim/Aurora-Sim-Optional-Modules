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
using Aurora.DataManager;
using Aurora.Framework;
using Aurora.DataManager.Migration;
using C5;

namespace Aurora.DataManager
{
    public class WebStatsMigrator_0 : Migrator
    {
        public WebStatsMigrator_0()
        {
            Version = new Version(0, 0, 0);
            MigrationName = "WebStats";

            schema = new List<Rec<string, ColumnDefinition[]>>();

            AddSchema("stats_session_data", ColDefs(ColDef("session_id", ColumnTypes.String50, true),
                 ColDef("agent_id", ColumnTypes.String50),
                 ColDef("region_id", ColumnTypes.String50),
                 ColDef("last_updated", ColumnTypes.Integer11),
                 ColDef("remote_ip", ColumnTypes.String45),
                 ColDef("name_f", ColumnTypes.String50),
                 ColDef("name_l", ColumnTypes.String50),
                 ColDef("avg_agents_in_view", ColumnTypes.String50),
                 ColDef("min_agents_in_view", ColumnTypes.String50),
                 ColDef("max_agents_in_view", ColumnTypes.String50),
                 ColDef("mode_agents_in_view", ColumnTypes.String50),
                 ColDef("avg_fps", ColumnTypes.String50),
                 ColDef("min_fps", ColumnTypes.String50),
                 ColDef("max_fps", ColumnTypes.String50),
                 ColDef("mode_fps", ColumnTypes.String50),
                 ColDef("a_language", ColumnTypes.String50),
                 ColDef("mem_use", ColumnTypes.String50),
                 ColDef("meters_traveled", ColumnTypes.String50),
                 ColDef("avg_ping", ColumnTypes.String50),
                 ColDef("min_ping", ColumnTypes.String50),
                 ColDef("max_ping", ColumnTypes.String50),
                 ColDef("mode_ping", ColumnTypes.String50),
                 ColDef("regions_visited", ColumnTypes.String50),
                 ColDef("run_time", ColumnTypes.String50),
                 ColDef("avg_sim_fps", ColumnTypes.String50),
                 ColDef("min_sim_fps", ColumnTypes.String50),
                 ColDef("max_sim_fps", ColumnTypes.String50),
                 ColDef("mode_sim_fps", ColumnTypes.String50),
                 ColDef("start_time", ColumnTypes.Integer11),
                 ColDef("client_version", ColumnTypes.String50),
                 ColDef("s_cpu", ColumnTypes.String50),
                 ColDef("s_gpu", ColumnTypes.String50),
                 ColDef("s_os", ColumnTypes.String50),
                 ColDef("s_ram", ColumnTypes.String50),
                 ColDef("d_object_kb", ColumnTypes.String50),
                 ColDef("d_texture_kb", ColumnTypes.String50),
                 ColDef("d_world_kb", ColumnTypes.String50),
                 ColDef("n_in_kb", ColumnTypes.String50),
                 ColDef("n_in_pk", ColumnTypes.String50),
                 ColDef("n_out_kb", ColumnTypes.String50),
                 ColDef("n_out_pk", ColumnTypes.String50),
                 ColDef("f_dropped", ColumnTypes.String50),
                 ColDef("f_failed_resends", ColumnTypes.String50),
                 ColDef("f_invalid", ColumnTypes.String50),
                 ColDef("f_off_circuit", ColumnTypes.String50),
                 ColDef("f_resent", ColumnTypes.String50),
                 ColDef("f_send_packet", ColumnTypes.String50)));
        }

        protected override void DoCreateDefaults(IDataConnector genericData)
        {
            EnsureAllTablesInSchemaExist(genericData);
        }

        protected override bool DoValidate(IDataConnector genericData)
        {
            return TestThatAllTablesValidate(genericData);
        }

        protected override void DoMigrate(IDataConnector genericData)
        {
            DoCreateDefaults(genericData);
        }

        protected override void DoPrepareRestorePoint(IDataConnector genericData)
        {
            CopyAllTablesToTempVersions(genericData);
        }
    }
}
