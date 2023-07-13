/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

12-07-2023	1.0.0.1		RRA, Skyline	Initial version
****************************************************************************
*/

namespace Next_live_On_Multiviewer_1
{
    using System;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.CommunityLibrary.Automation.Extensions;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.ConnectorAPI.EvsCerebrum;
    using Skyline.DataMiner.Utils.ConnectorAPI.EvsCerebrum.IAC.Common.Routes.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public static class Script
    {
        private const string CerebrumElementName = "{ Active BC SAW Server }";

        private const string DefaultOptionalLevel = "[Optional]";

        /// <summary>
        ///     The Script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public static void Run(Engine engine)
        {
            try
            {
                // Retrieve data
                var nextChannelEvents = GetNextChannelEvents(engine);
                var cerebrumSAW = engine.GetDms().GetElement(CerebrumElementName);

                var labels = GetLabels(nextChannelEvents.ChannelName) ?? throw new NotSupportedException($"Channel '{nextChannelEvents.ChannelName}' not supported.");

                SetNextLiveOnMultiviewer(engine, cerebrumSAW, nextChannelEvents, labels);
            }
            catch (Exception e)
            {
                engine.ExitFail("ERROR|Exception thrown:" + Environment.NewLine + e);
            }
        }

        private static void CreateRoute(Engine engine, IDmsElement element, string source, string destination)
        {
            string sourceName = source == "-" ? "NC" : source;

            var createRoute = new CreateRoute
            {
                DestLevelName = DefaultOptionalLevel,
                DeviceInstance = "0.0.0.0-Router",
                DestName = destination,
                SourceLevelName = DefaultOptionalLevel,
                SourceName = sourceName,
                UseTags = true,
            };

            var evsClient = new EvsCerebrumEngineClient(engine, element.DmsElementId);
            evsClient.CreateRouteAsync(createRoute);
        }

        private static Labels GetLabels(string channelName)
        {
            var labels = new Labels();

            switch (channelName)
            {
                case "ORF1":
                    labels.MainN = "ORF1 MV NL";
                    labels.MainN1 = "ORF1 MV NL+1";
                    labels.MainN2 = "ORF1 MV NL+2";
                    labels.BackupN = String.Empty;
                    labels.BackupN1 = String.Empty;
                    labels.BackupN2 = String.Empty;

                    break;
                case "ORF2":
                    labels.MainN = "ORF2 MV NL";
                    labels.MainN1 = "ORF2 MV NL+1";
                    labels.MainN2 = "ORF2 MV NL+2";
                    labels.BackupN = String.Empty;
                    labels.BackupN1 = String.Empty;
                    labels.BackupN2 = String.Empty;

                    break;
                case "ORF3":
                    labels.MainN = "ORF3 MV NL";
                    labels.MainN1 = "ORF3 MV NL+1";
                    labels.MainN2 = "ORF3 MV NL+2";
                    labels.BackupN = String.Empty;
                    labels.BackupN1 = String.Empty;
                    labels.BackupN2 = String.Empty;

                    break;
                case "OSP":
                    labels.MainN = "OSP MV NL";
                    labels.MainN1 = "OSP MV NL+1";
                    labels.MainN2 = "OSP MV NL+2";
                    labels.BackupN = String.Empty;
                    labels.BackupN1 = String.Empty;
                    labels.BackupN2 = String.Empty;

                    break;

                case "ORF_2nd1":
                    labels.MainN = "2nd1 MV NL";
                    labels.BackupN = String.Empty;

                    break;
                case "ORF_2nd2":
                    labels.MainN = "2nd2 MV NL";
                    labels.BackupN = String.Empty;

                    break;
                case "ORF_2nd3":
                    labels.MainN = "2nd3 MV NL";
                    labels.BackupN = String.Empty;

                    break;
                case "ORF_2nd4":
                    labels.MainN = "2nd4 MV NL";
                    labels.BackupN = String.Empty;

                    break;
                case "ORF_2nd5":
                    labels.MainN = "2nd5 MV NL";
                    labels.BackupN = String.Empty;

                    break;
                case "ORF2E":
                    labels.MainN = "ORF2E MV NL";
                    labels.BackupN = String.Empty;
                    break;
                case "TuS":
                    labels.MainN = "TuS MV NL";
                    labels.MainN1 = "TuS MV NL+1";
                    labels.MainN2 = "TuS MV NL+2";
                    labels.BackupN = String.Empty;
                    labels.BackupN1 = String.Empty;
                    labels.BackupN2 = String.Empty;

                    break;
                default:
                    return null;
            }

            return labels;
        }

        private static NextChannelEvents GetNextChannelEvents(Engine engine)
        {
            // information event content: 'ORF1|SR001|SR002|SR003'
            var pattern = @"([^\|]+)\^([^\|]+)\^([^\|]+)\^([^\|]+)";
            var correlationAlarmInfo = engine.GetCorrelationAlarmInfo();
            var match = Regex.Match(correlationAlarmInfo.AlarmValue, pattern);

            if (!match.Success)
            {
                throw new FormatException($"Input with value'{correlationAlarmInfo.AlarmValue}' has invalid format.");
            }

            var nextChannelEvents = new NextChannelEvents
            {
                ChannelName = match.Groups[1].Value,
                Source_N = match.Groups[2].Value,
                Source_N1 = match.Groups[3].Value,
                Source_N2 = match.Groups[4].Value,
            };

            return nextChannelEvents;
        }

        private static void SetNextLiveOnMultiviewer(Engine engine, IDmsElement element, NextChannelEvents nextChannelEvents, Labels labels)
        {
            CreateRoute(engine, element, nextChannelEvents.Source_N, labels.MainN);
            CreateRoute(engine, element, nextChannelEvents.Source_N1, labels.MainN1);
            CreateRoute(engine, element, nextChannelEvents.Source_N2, labels.MainN2);
        }
    }

    public class NextChannelEvents
    {
        public string ChannelName { get; set; }

        public string Source_N { get; set; }

        public string Source_N1 { get; set; }

        public string Source_N2 { get; set; }
    }

    public class Labels
    {
        public string BackupN { get; set; }

        public string BackupN1 { get; set; }

        public string BackupN2 { get; set; }

        public string MainN { get; set; }

        public string MainN1 { get; set; }

        public string MainN2 { get; set; }
    }
}