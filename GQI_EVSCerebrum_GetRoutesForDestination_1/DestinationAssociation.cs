using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Skyline.DataMiner.Core.DataMinerSystem.Common.Selectors.Data;
using Skyline.DataMiner.Net.Messages;
using System.Linq;

namespace GQI_EVSCerebrum_GetRoutesForDestination_1
{
    internal class DestinationAssociation
    {
        public string Instance { get; set; }

        public string DisplayKey { get; set; }

        public string DestinationInstance { get; set; }

        public string DestinationName { get; set; }

        public string LevelInstance { get; set; }

        public static List<DestinationAssociation> CreateDestinationAssociations(ParameterValue[] columns)
        {
            if (!columns.Any())
            {
                return new List<DestinationAssociation>();
            }

            var destinationAssociations = new List<DestinationAssociation>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var destinationInstance = columns[1].ArrayValue[i]?.CellValue?.GetAsStringValue();

                var instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue();
                var displayKey = columns[2].ArrayValue[i]?.CellValue?.GetAsStringValue();

                var levelInstanceRegex = Regex.Match(instance, "^(.*?-\\d+)-\\d+$");
                var destinationNameRegex = Regex.Match(displayKey, "^[^/]+/([^/]+)/");

                var destinationAssociation = new DestinationAssociation
                {
                    Instance = instance,
                    DisplayKey = displayKey,
                    DestinationInstance = destinationInstance,
                    DestinationName = destinationNameRegex.Success ? destinationNameRegex.Groups[1].Value : String.Empty,
                    LevelInstance = levelInstanceRegex.Success ? levelInstanceRegex.Groups[1].Value : String.Empty,
                };

                destinationAssociations.Add(destinationAssociation);
            }

            return destinationAssociations;
        }
    }
}