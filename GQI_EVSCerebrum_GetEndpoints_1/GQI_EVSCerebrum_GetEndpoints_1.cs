namespace GQI_EVSCerebrum_GetEndpoints_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;
    using GQI_EVSCerebrum_GetEndpoints_1;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.Advanced;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using GQI_EVSCerebrum_GetEndpoints_1.RealTimeUpdates;
    using SLLoggerUtil.LoggerCategoryUtil.DataGateway;
    using System.IO;

    [GQIMetaData(Name = "EVS Cerebrum Get Endpoints")]
    public class GQI_EVSCerebrum_GetEndpoints : IGQIDataSource, IGQIOnInit, IGQIInputArguments, IGQIUpdateable
    {
        private readonly GQIStringArgument mnemonicTypeArgument = new GQIStringArgument("Mnemonic Type") { IsRequired = true };
        private readonly GQIStringArgument _categoryArgument = new GQIStringArgument("Category") { IsRequired = false };
        private MnemonicType mnemonic;
        private string category;

        private GQIDMS dms;

        private int dataminerId;
        private int elementId;

        private DataProvider _dataProvider;

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
                mnemonicTypeArgument,
                _categoryArgument,
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            var rawMnemonic = args.GetArgumentValue(mnemonicTypeArgument);
            mnemonic = (MnemonicType)Enum.Parse(typeof(MnemonicType), rawMnemonic);

            category = args.GetArgumentValue(_categoryArgument);

            return new OnArgumentsProcessedOutputArgs();
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Instance"),
                new GQIStringColumn("Mnemonic"),
                new GQIStringColumn("Categories"),
            };
        }

        public void OnStartUpdates(IGQIUpdater updater)
        {
            _updater = updater;
            _dataProvider.SourceTable.Changed += TableData_OnChanged;
            _dataProvider.DestinationsTable.Changed += TableData_OnChanged;
            _dataProvider.SourceCategoriesTable.Changed += TableData_OnChanged;
            _dataProvider.DestinationCategoriesTable.Changed += TableData_OnChanged;
        }

        public void OnStopUpdates()
        {
            _dataProvider.SourceTable.Changed -= TableData_OnChanged;
            _dataProvider.DestinationsTable.Changed -= TableData_OnChanged;
            _dataProvider.SourceCategoriesTable.Changed -= TableData_OnChanged;
            _dataProvider.DestinationCategoriesTable.Changed -= TableData_OnChanged;
            _updater = null;
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
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
            var controlSurfaceFilter = new CerebrumFilter(_dataProvider, mnemonic, category);
            var endPoints = controlSurfaceFilter.GetEndpoints();

            return endPoints.Select(x => x.ToRow());
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
}