using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GQI_EVSCerebrum_GetRoutesForDestination_1;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;

[GQIMetaData(Name = "EVS Cerebrum Get Routes For Destination")]
public class GQI_EVSCerebrum_GetRoutesForDestination : IGQIDataSource, IGQIOnInit, IGQIInputArguments
{
    private readonly GQIStringArgument destinationArgument = new GQIStringArgument("Destination") { IsRequired = true };
    private string destination;

    private GQIDMS dms;

    private int dataminerId;
    private int elementId;

    public GQIColumn[] GetColumns()
    {
        return new GQIColumn[]
        {
            new GQIStringColumn("Destination"),
            new GQIStringColumn("Destination Level"),
            new GQIStringColumn("Source"),
            new GQIStringColumn("Source Level"),
        };
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[] { destinationArgument };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        destination = Convert.ToString(args.GetArgumentValue(destinationArgument));
        return new OnArgumentsProcessedOutputArgs();
    }

    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        var levels = GetLevels();
        var destinationAssociations = GetDestinationAssociations();

        var rows = new List<GQIRow>();
        foreach (var destinationAssociation in destinationAssociations.OrderBy(d => d.Instance))
        {
            var route = GetRouteForDestinationAssociation(destinationAssociation.Instance);
            if (!route.IsValid())
            {
                //levels.TryGetValue(destinationAssociation.LevelInstance, out var level);
                var level = levels.Values.FirstOrDefault(lvl => destinationAssociation.DisplayKey != null && destinationAssociation.DisplayKey.Contains(lvl.Mnemonic));

                // get levels for destination
                route = new Route
                {
                    Destination = destinationAssociation.DestinationName,
                    DestinationLevel = level?.Mnemonic,
                    Source = String.Empty,
                    SourceLevel = String.Empty,
                };
            }

            var row = new GQIRow(
                new[]
                {
                    new GQICell { Value = route.Destination },
                    new GQICell { Value = route.DestinationLevel },
                    new GQICell { Value = route.Source },
                    new GQICell { Value = route.SourceLevel },
                });
            rows.Add(row);
        }

        return new GQIPage(rows.ToArray())
        {
            HasNextPage = false,
        };
    }

    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        dms = args.DMS;
        GetEvsCerebrumArgument();

        return new OnInitOutputArgs();
    }

    private void GetEvsCerebrumArgument()
    {
        dataminerId = -1;
        elementId = -1;

        var infoMessage = new GetInfoMessage { Type = InfoType.ElementInfo };
        var infoMessageResponses = dms.SendMessages(infoMessage);
        foreach (var response in infoMessageResponses)
        {
            var elementInfoEventMessage = (ElementInfoEventMessage)response;
            if (elementInfoEventMessage == null)
            {
                continue;
            }

            if (elementInfoEventMessage?.Protocol == "EVS Cerebrum" && elementInfoEventMessage?.ProtocolVersion == "Production")
            {
                dataminerId = elementInfoEventMessage.DataMinerID;
                elementId = elementInfoEventMessage.ElementID;
                break;
            }
        }
    }

    private List<DestinationAssociation> GetDestinationAssociations()
    {
        var columns = GetDestinationAssociationsTableColumns(destination);
        if (!columns.Any())
        {
            return new List<DestinationAssociation>();
        }

        var destinationAssociations = new List<DestinationAssociation>();

        for (int i = 0; i < columns[0].ArrayValue.Length; i++)
        {
            var destinationAssociationInstance = columns[1].ArrayValue[i]?.CellValue?.GetAsStringValue();
            if (destinationAssociationInstance != destination)
            {
                continue;
            }

            var instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue();
            var displayKey = columns[2].ArrayValue[i]?.CellValue?.GetAsStringValue();

            var levelInstanceRegex = Regex.Match(instance, "^(.*?-\\d+)-\\d+$");
            var destinationNameRegex = Regex.Match(displayKey, "^[^/]+/([^/]+)/");

            var destinationAssociation = new DestinationAssociation
            {
                Instance = instance,
                DisplayKey = displayKey,
                DestinationInstance = destination,
                DestinationName = destinationNameRegex.Success ? destinationNameRegex.Groups[1].Value : String.Empty,
                LevelInstance = levelInstanceRegex.Success ? levelInstanceRegex.Groups[1].Value : String.Empty,
            };

            destinationAssociations.Add(destinationAssociation);
        }

        return destinationAssociations;
    }

    private ParameterValue[] GetDestinationAssociationsTableColumns(string destinationInstance)
    {
        // "forceFullTable=true"
        // "value=15302 == {destinationInstance}"

        var tableId = 15300;
        var getPartialTableMessage = new GetPartialTableMessage(
            dataminerId,
            elementId,
            tableId,
            new[] { $"forceFullTable=true" });

        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 10)
        {
            return new ParameterValue[0];
        }

        return columns;
    }

    private Route GetRouteForDestinationAssociation(string destinationAssociationInstance)
    {
        try
        {
            object[] details = new object[4];
            details[0] = dataminerId;
            details[1] = elementId;
            details[2] = 12100;
            details[3] = destinationAssociationInstance;

            var message = new SetDataMinerInfoMessage
            {
                DataMinerID = dataminerId,
                ElementID = elementId,
                What = 215,
                Var1 = details,
            };

            var response = (SetDataMinerInfoResponseMessage)dms.SendMessage(message);
            return new Route((object[])response.RawData);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Dictionary<string, Level> GetLevels()
    {
        var columns = GetLevelsTableColumns();
        if (!columns.Any())
        {
            return new Dictionary<string, Level>();
        }

        var levels = new Dictionary<string, Level>();

        for (int i = 0; i < columns[3].ArrayValue.Length; i++)
        {
            var level = new Level
            {
                Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Id = columns[3].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Name = columns[4].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                Mnemonic = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue(),
            };

            levels[level.Instance] = level;
        }

        return levels;
    }

    private ParameterValue[] GetLevelsTableColumns()
    {
        var tableId = 13100;
        var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
        var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
        if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
        {
            return new ParameterValue[0];
        }

        var columns = parameterChangeEventMessage.NewValue.ArrayValue;
        if (columns.Length < 6)
        {
            return new ParameterValue[0];
        }

        return columns;
    }
}