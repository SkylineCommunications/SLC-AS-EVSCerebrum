using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GQI_EVSCerebrum_GetEndpoints_1.RealTimeUpdates;
using GQI_EVSCerebrum_GetRoutesForDestination_1;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.Advanced;

[GQIMetaData(Name = "EVS Cerebrum Get Routes For Destination")]
public class GQI_EVSCerebrum_GetRoutesForDestination : IGQIDataSource, IGQIOnInit, IGQIInputArguments, IGQIUpdateable
{
    private readonly GQIStringArgument _destinationArgument = new GQIStringArgument("Destination") { IsRequired = false };

    private string destination;

    private GQIDMS dms;

    private int dataminerId;
    private int elementId;

    private DataProvider _dataProvider;
    private CerebrumFilter cerebrumFilter;

    private ICollection<GQIRow> _currentRows = Array.Empty<GQIRow>();
    private IGQIUpdater _updater;

    public OnInitOutputArgs OnInit(OnInitInputArgs args)
    {
        dms = args.DMS;
        GetEvsCerebrumArgument();

        StaticDataProvider.Initialize(dms, dataminerId, elementId);
        _dataProvider = StaticDataProvider.Instance;

        return new OnInitOutputArgs();
    }

    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[]
        {
            _destinationArgument,
        };
    }

    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        destination = Convert.ToString(args.GetArgumentValue(_destinationArgument));

        return new OnArgumentsProcessedOutputArgs();
    }

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

    public GQIPage GetNextPage(GetNextPageInputArgs args)
    {
        try
        {
            var newRows = CalculateNewRows().ToArray();

            try
            {
                return new GQIPage(newRows)
                {
                    HasNextPage = false,
                };
            }
            finally
            {
                _currentRows = newRows;
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }

    public void OnStartUpdates(IGQIUpdater updater)
    {
        _updater = updater;
        _dataProvider.LevelsTable.Changed += TableData_OnChanged;
        _dataProvider.RoutesTable.Changed += TableData_OnChanged;
        _dataProvider.DestinationsAssociationsTable.Changed += TableData_OnChanged;
    }

    public void OnStopUpdates()
    {
        _dataProvider.LevelsTable.Changed -= TableData_OnChanged;
        _dataProvider.RoutesTable.Changed -= TableData_OnChanged;
        _dataProvider.DestinationsAssociationsTable.Changed -= TableData_OnChanged;
        _updater = null;
    }

    private void TableData_OnChanged(object sender, ParameterTableUpdateEventMessage e)
    {
        var newRows = CalculateNewRows().ToList();

        try
        {
            var comparison = new GqiTableComparer(_currentRows, newRows);

            foreach (var row in comparison.RemovedRows)
            {
                _updater.RemoveRow(row.Key);
            }

            foreach (var row in comparison.UpdatedRows)
            {
                _updater.UpdateRow(row);
            }

            foreach (var row in comparison.AddedRows)
            {
                _updater.AddRow(row);
            }
        }
        finally
        {
            _currentRows = newRows;
        }
    }

    private IEnumerable<GQIRow> CalculateNewRows()
    {
        cerebrumFilter = new CerebrumFilter(_dataProvider, destination);

        var routes = cerebrumFilter.GetRoutes();

        return routes.Select(x => x.ToRow());
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
}