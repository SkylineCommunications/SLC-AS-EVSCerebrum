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

dd/mm/2023	1.0.0.1		RRA, Skyline	Initial version
****************************************************************************
*/

namespace CreateRoute_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.ConnectorAPI.EvsCerebrum;
    using Skyline.DataMiner.Utils.ConnectorAPI.EvsCerebrum.IAC.Common.Routes.Messages;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private string device;
        private string source;
        private string destination;
        private string[] levels;
        private bool allLevelTake;

        private InteractiveController interactiveController;

        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(Engine engine)
        {
            device = GetValuesFromInputParameter(engine, "Device").FirstOrDefault();
            source = GetValuesFromInputParameter(engine, "Source").FirstOrDefault();
            destination = GetValuesFromInputParameter(engine, "Destination").FirstOrDefault();
            levels = GetValuesFromInputParameter(engine, "Levels");
            allLevelTake = Convert.ToBoolean(engine.GetScriptParam("AllLevelTake").Value);

            var dms = engine.GetDms();
            var elementEVSCerebrum = dms.GetElements().First(e => e.Protocol.Name == "EVS Cerebrum" && e.Protocol.Version == "Production");

            CreateRoute(engine, elementEVSCerebrum);
        }

        private static string[] GetValuesFromInputParameter(IEngine engine, string parameterName)
        {
            try
            {
                var input = JsonConvert.DeserializeObject<string[]>(engine.GetScriptParam(parameterName).Value);
                return input;
            }
            catch (Exception e)
            {
                engine.Log($"Exception deserializing input parameter {parameterName}: {e}");
                return new string[0];
            }
        }

        private static string[] GatherAllLevels(IDmsElement evsElement)
        {
            var levelsTable = evsElement.GetTable(13100).GetData();

            var levels = new List<string>();
            foreach (var tableEntry in levelsTable.Values)
            {
                string mnemonic = Convert.ToString(tableEntry[5]);
                if (!string.IsNullOrWhiteSpace(mnemonic))
                {
                    levels.Add(mnemonic);
                }
            }

            return levels.ToArray();
        }

        private void CreateRoute(Engine engine, IDmsElement evsElement)
        {
            var evsClient = new EvsCerebrumEngineClient(engine, evsElement.DmsElementId);

            if (allLevelTake || !levels.Any())
            {
                levels = GatherAllLevels(evsElement);
            }

            foreach (var levelMnemonic in levels)
            {
                var route = new CreateRoute
                {
                    DestLevelName = levelMnemonic,
                    DeviceInstance = device,
                    DestName = destination,
                    SourceLevelName = levelMnemonic,
                    SourceName = source,
                    UseTags = false,
                };

                evsClient.CreateRouteAsync(route);
            }

            // Commented out as level amount isn't always matching between source & destination, Task id: "255289" to improve this
            //if (!VerifyCreateRoute(engine, evsElement))
            //{
            //    ShowErrorDialog(engine);
            //}
        }

        private bool VerifyCreateRoute(Engine engine, IDmsElement evsElement)
        {
            int retries = 0;
            bool allEntriesFound = false;
            while (!allEntriesFound && retries < 50)
            {
                engine.Sleep(50);

                var existingDestinationRows = evsElement.GetTable(12100).QueryData(new[]
                {
                    new ColumnFilter { Pid = 12109, Value = destination, ComparisonOperator = ComparisonOperator.Equal },
                }).ToList();

                if (!existingDestinationRows.Any()) continue;

                var filteredRowsBasedOnLevelSelection = existingDestinationRows.Where(row => levels.Contains(Convert.ToString(row[10]))).ToList();
                allEntriesFound = filteredRowsBasedOnLevelSelection.All(row => Convert.ToString(row[4]) == source);

                if (allEntriesFound)
                {
                    return true;
                }

                retries++;
            }

            return false;
        }

        private void ShowErrorDialog(Engine engine)
        {
            //engine.ShowUI();

            interactiveController = new InteractiveController(engine);

            var errorDialog = new ErrorDialog(engine, "Take Failed", $"Could not establish route(s) for Source: {source} and Destination: {destination}.");
            errorDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);

            if (interactiveController.IsRunning) interactiveController.ShowDialog(errorDialog);
            else interactiveController.Run(errorDialog);
        }
    }
}