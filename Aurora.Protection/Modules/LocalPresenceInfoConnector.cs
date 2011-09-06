/*
 *  Copyright 2011 Matthew Beardmore
 *
 *  This file is part of Aurora.Addon.Protection.
 *  Aurora.Addon.Protection is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *  Aurora.Addon.Protection is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *  You should have received a copy of the GNU General Public License along with Aurora.Addon.Protection. If not, see http://www.gnu.org/licenses/.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OpenMetaverse;
using Aurora.DataManager;
using Aurora.Framework;
using log4net;
using Nini.Config;
using OpenSim.Framework;

namespace Aurora.OptionalModules
{
    public class LocalPresenceInfoConnector : IPresenceInfo, IAuroraDataPlugin
	{
		private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IGenericData GD = null;
        private string DatabaseToAuthTable = "auth";

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore registry, string DefaultConnectionString)
        {
            if (source.Configs["AuroraConnectors"].GetString("PresenceInfoConnector", "LocalConnector") == "LocalConnector")
            {
                GD = GenericData;

                if (source.Configs[Name] != null)
                {
                    DefaultConnectionString = source.Configs[Name].GetString("ConnectionString", DefaultConnectionString);
                    DatabaseToAuthTable = source.Configs[Name].GetString("DatabasePathToAuthTable", DatabaseToAuthTable);
                }
                GD.ConnectToDatabase(DefaultConnectionString, "PresenceInfo", true);
                DataManager.DataManager.RegisterPlugin(Name, this);
            }
        }

        public string Name
        {
            get { return "IPresenceInfo"; }
        }

        public void Dispose()
        {
        }

        public PresenceInfo GetPresenceInfo(UUID agentID)
		{
            PresenceInfo agent = new PresenceInfo();
            List<string> query = GD.Query("one", agentID, "presenceinfo", "*");

			if (query.Count == 0)
				//Couldn't find it, return null then.
				return null;

            agent.AgentID = agentID;
            if(query[1] != "")
                agent.Flags = (PresenceInfo.PresenceInfoFlags)Enum.Parse(typeof(PresenceInfo.PresenceInfoFlags),query[1]);
            agent.KnownAlts = ConvertToList(query[2]);
            agent.KnownID0s = ConvertToList(query[3]);
            agent.KnownIPs = ConvertToList(query[4]);
            agent.KnownMacs = ConvertToList(query[5]);
            agent.KnownViewers = ConvertToList(query[6]);
            agent.LastKnownID0 = query[7];
            agent.LastKnownIP = query[8];
            agent.LastKnownMac = query[9];
            agent.LastKnownViewer = query[10];
            agent.Platform = query[11];
            
			return agent;
		}

        public void UpdatePresenceInfo(PresenceInfo agent)
		{
			List<object> SetValues = new List<object>();
            List<string> SetRows = new List<string>();
            SetRows.Add("one"/*"AgentID"*/);
            SetRows.Add("two"/*"Flags"*/);
            SetRows.Add("three"/*"KnownAlts"*/);
            SetRows.Add("four"/*"KnownID0s"*/);
            SetRows.Add("five"/*"KnownIPs"*/);
            SetRows.Add("six"/*"KnownMacs"*/);
            SetRows.Add("seven"/*"KnownViewers"*/);
            SetRows.Add("eight"/*"LastKnownID0"*/);
            SetRows.Add("nine"/*"LastKnownIP"*/);
            SetRows.Add("ten"/*"LastKnownMac"*/);
            SetRows.Add("eleven"/*"LastKnownViewer"*/);
            SetRows.Add("twelve"/*"Platform"*/);
            SetValues.Add(agent.AgentID);
            SetValues.Add(agent.Flags);
            SetValues.Add(ConvertToString(agent.KnownAlts));
            SetValues.Add(ConvertToString(agent.KnownID0s));
            SetValues.Add(ConvertToString(agent.KnownIPs));
            SetValues.Add(ConvertToString(agent.KnownMacs));
            SetValues.Add(ConvertToString(agent.KnownViewers));
            SetValues.Add(agent.LastKnownID0);
            SetValues.Add(agent.LastKnownIP);
            SetValues.Add(agent.LastKnownMac);
            SetValues.Add(agent.LastKnownViewer);
            SetValues.Add(agent.Platform);
            GD.Replace("presenceInfo", SetRows.ToArray(), SetValues.ToArray());
        }

        private string ConvertToString(List<string> list)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string val in list)
            {
                builder.Append(val + ",");
            }
            return builder.ToString();
        }

        private List<string> ConvertToList(string listAsString)
        {
            List<string> value = new List<string>(listAsString.Split(new string[]{","},StringSplitOptions.RemoveEmptyEntries));
            return value;
        }

        public void Check(List<string> bannedViewers)
        {
            //Get all UUIDS
            List<string> query = GD.Query("", "", "presenceinfo", "one"/*"AgentID"*/);
            foreach (string ID in query)
            {
                //Check all
                Check(GetPresenceInfo(UUID.Parse(ID)), bannedViewers);
            }
        }

        public void Check (PresenceInfo info, List<string> bannedViewers)
        {
            //
            //Check passwords
            //Check IPs, Mac's, etc
            //

            bool needsUpdated = false;

            #region Check Password

            List<string> query = GD.Query("UUID", info.AgentID, DatabaseToAuthTable, "passwordHash");
            if (query.Count != 0)
            {
                string password = query[0];
                query = GD.Query("passwordHash", password, DatabaseToAuthTable, "UUID");
                foreach (string ID in query)
                {
                    PresenceInfo suspectedInfo = GetPresenceInfo(UUID.Parse(ID));
                    if (suspectedInfo.AgentID == info.AgentID)
                        continue;

                    CoralateLists (info, suspectedInfo);

                    needsUpdated = true;
                }
            }

            #endregion

            #region Check ID0, IP, Mac, etc

            //Only check suspected and known offenders in this scan
            // 2 == Flags
            query = GD.Query(" (two = 'SuspectedAltAccountOfKnown'" +
                " or two = 'Known' or two = 'SuspectedAltAccountOfSuspected'" +
                " or two = 'Banned'" +
                " or two = 'Suspected')", "presenceinfo", "one");
            foreach (string ID in query)
            {
                PresenceInfo suspectedInfo = GetPresenceInfo(UUID.Parse(ID));
                if (suspectedInfo.AgentID == info.AgentID)
                    continue;
                foreach (string ID0 in suspectedInfo.KnownID0s)
                {
                    if (info.KnownID0s.Contains(ID0))
                    {
                        CoralateLists (info, suspectedInfo);
                        needsUpdated = true;
                    }
                }
                foreach (string IP in suspectedInfo.KnownIPs)
                {
                    if (info.KnownIPs.Contains(IP.Split(':')[0]))
                    {
                        CoralateLists (info, suspectedInfo);
                        needsUpdated = true;
                    }
                }
                foreach (string Mac in suspectedInfo.KnownMacs)
                {
                    if (info.KnownMacs.Contains(Mac))
                    {
                        CoralateLists (info, suspectedInfo);
                        needsUpdated = true;
                    }
                }
            }

            foreach (string viewer in info.KnownViewers)
            {
                if (bannedViewers.Contains(viewer.StartsWith(" ") ? viewer.Remove(1) : viewer))
                {
                    if ((info.Flags & PresenceInfo.PresenceInfoFlags.Clean) == PresenceInfo.PresenceInfoFlags.Clean)
                    {
                        //Update them to suspected for their viewer
                        AddFlag (ref info, PresenceInfo.PresenceInfoFlags.Suspected);
                        //And update them later
                        needsUpdated = true;
                    }
                    else if ((info.Flags & PresenceInfo.PresenceInfoFlags.Suspected) == PresenceInfo.PresenceInfoFlags.Suspected)
                    {
                        //Suspected, we don't really want to move them higher than this...
                    }
                    else if ((info.Flags & PresenceInfo.PresenceInfoFlags.Known) == PresenceInfo.PresenceInfoFlags.Known)
                    {
                        //Known, can't update anymore
                    }
                }
            }
            if (DoGC(info) & !needsUpdated)//Clean up all info
                needsUpdated = true;

            #endregion

            //Now update ours
            if (needsUpdated)
                UpdatePresenceInfo(info);
        }

        private bool DoGC(PresenceInfo info)
        {
            bool update = false;
            List<string> newIPs = new List<string>();
            foreach (string ip in info.KnownIPs)
            {
                string[] split;
                string newIP = ip;
                if ((split = ip.Split(':')).Length > 1)
                {
                    //Remove the port if it exists and force an update
                    newIP = split[0];
                    update = true;
                }
                if (!newIPs.Contains(newIP))
                    newIPs.Add(newIP);
            }
            if (info.KnownIPs.Count != newIPs.Count)
                update = true;
            info.KnownIPs = newIPs;

            return update;
        }

        private void CoralateLists (PresenceInfo info, PresenceInfo suspectedInfo)
        {
            bool addedFlag = false;
            PresenceInfo.PresenceInfoFlags Flag = 0;

            if ((suspectedInfo.Flags & PresenceInfo.PresenceInfoFlags.Clean) == PresenceInfo.PresenceInfoFlags.Clean &&
                    (info.Flags & PresenceInfo.PresenceInfoFlags.Clean) == PresenceInfo.PresenceInfoFlags.Clean)
            {
                //They are both clean, do nothing
            }
            else if ((suspectedInfo.Flags & PresenceInfo.PresenceInfoFlags.Suspected) == PresenceInfo.PresenceInfoFlags.Suspected ||
                (info.Flags & PresenceInfo.PresenceInfoFlags.Suspected) == PresenceInfo.PresenceInfoFlags.Suspected)
            {
                //Suspected, update them both
                addedFlag = true;
                AddFlag (ref info, PresenceInfo.PresenceInfoFlags.Suspected);
                AddFlag (ref suspectedInfo, PresenceInfo.PresenceInfoFlags.Suspected);
            }
            else if ((suspectedInfo.Flags & PresenceInfo.PresenceInfoFlags.Known) == PresenceInfo.PresenceInfoFlags.Known ||
                (info.Flags & PresenceInfo.PresenceInfoFlags.Known) == PresenceInfo.PresenceInfoFlags.Known)
            {
                //Known, update them both
                addedFlag = true;
                AddFlag (ref info, PresenceInfo.PresenceInfoFlags.Known);
                AddFlag (ref suspectedInfo, PresenceInfo.PresenceInfoFlags.Known);
            }

            //Add the alt account flag
            AddFlag (ref info, PresenceInfo.PresenceInfoFlags.SuspectedAltAccount);
            AddFlag (ref suspectedInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccount);

            if (suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.Suspected ||
                suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected ||
                info.Flags == PresenceInfo.PresenceInfoFlags.Suspected ||
                info.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected)
            {
                //They might be an alt, but the other is clean, so don't bother them too much
                AddFlag (ref info, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected);
                AddFlag (ref suspectedInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected);
            }
            else if (suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.Known ||
                suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown ||
                info.Flags == PresenceInfo.PresenceInfoFlags.Known ||
                info.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown)
            {
                //Flag 'em
                AddFlag (ref info, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown);
                AddFlag (ref suspectedInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown);
            }

            //Add each user to the list of alts, then add the lists of both together
            info.KnownAlts.Add (suspectedInfo.AgentID.ToString ());
            suspectedInfo.KnownAlts.Add (info.AgentID.ToString ());

            //Add the lists together
            List<string> alts = new List<string> ();
            foreach (string alt in info.KnownAlts)
            {
                if (!alts.Contains (alt))
                    alts.Add (alt);
            }
            foreach (string alt in suspectedInfo.KnownAlts)
            {
                if (!alts.Contains (alt))
                    alts.Add (alt);
            }

            //If we have added a flag, we need to update ALL alts as well
            if (addedFlag && alts.Count != 0)
            {
                foreach (string alt in alts)
                {
                    PresenceInfo altInfo = GetPresenceInfo (UUID.Parse (alt));
                    if (altInfo != null)
                    {
                        //Give them the flag as well
                        AddFlag (ref altInfo, Flag);

                        //Add the alt account flag
                        AddFlag (ref altInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccount);

                        //Also give them the flags for alts
                        if (suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.Suspected ||
                            suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected ||
                            info.Flags == PresenceInfo.PresenceInfoFlags.Suspected ||
                            info.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected)
                        {
                            //They might be an alt, but the other is clean, so don't bother them too much
                            AddFlag (ref altInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfSuspected);
                        }
                        else if (suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.Known ||
                            suspectedInfo.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown ||
                            info.Flags == PresenceInfo.PresenceInfoFlags.Known ||
                            info.Flags == PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown)
                        {
                            //Flag 'em
                            AddFlag (ref altInfo, PresenceInfo.PresenceInfoFlags.SuspectedAltAccountOfKnown);
                        }

                        //And update them in the db
                        UpdatePresenceInfo (suspectedInfo);
                    }
                }
            }

            //Replace both lists now that they are merged
            info.KnownAlts = alts;
            suspectedInfo.KnownAlts = alts;

            //Update them, as we changed their info, we get updated below
            UpdatePresenceInfo (suspectedInfo);
        }

        private void AddFlag (ref PresenceInfo info, PresenceInfo.PresenceInfoFlags presenceInfoFlags)
        {
            if (presenceInfoFlags == 0)
                return;
            info.Flags &= PresenceInfo.PresenceInfoFlags.Clean; //Remove clean
            if (presenceInfoFlags == PresenceInfo.PresenceInfoFlags.Known)
                info.Flags &= PresenceInfo.PresenceInfoFlags.Clean; //Remove suspected as well
            info.Flags |= presenceInfoFlags; //Add the flag
        }
    }
}
