using System;
using System.Collections.Generic;
using System.Linq;
using GQI_EVSCerebrum_GetLevels_1;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;

[GQIMetaData(Name = "EVS Cerebrum Get Levels")]
public class GQI_EVSCerebrum_GetLevels : IGQIDataSource, IGQIOnInit, IGQIInputArguments
{
    private GQIDMS dms;

    private GQIStringArgument destinationArgument = new GQIStringArgument("Destination") { IsRequired = false };
    private string destination;

    private int dataminerId;
    private int elementId;

    public GQIColumn[] GetColumns()
    {
        return new GQIColumn[]
        {
            new GQIStringColumn("Level ID"),
            new GQIStringColumn("Level Name"),
            new GQIStringColumn("Level Mnemonic"),
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

        var rows = new List<GQIRow>();
        foreach (var level in levels.OrderBy(l => l.Id))
        {
            var row = new GQIRow(
                new GQICell[]
                {
                    new GQICell { Value = level.Id },
                    new GQICell { Value = level.Name },
                    new GQICell { Value = level.Mnemonic },
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

    private List<Level> GetLevels()
    {
        var columns = GetLevelsTableColumns();
        if (!columns.Any())
        {
            return new List<Level>();
        }

        var levels = new List<Level>();

        for (int i = 0; i < columns[3].ArrayValue.Length; i++)
        {
            var level = new Level
            {
                Id = columns[3].ArrayValue[i]?.GetAsStringValue(),
                Name = columns[4].ArrayValue[i]?.GetAsStringValue(),
                Mnemonic = columns[5].ArrayValue[i]?.GetAsStringValue(),
            };

            levels.Add(level);
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