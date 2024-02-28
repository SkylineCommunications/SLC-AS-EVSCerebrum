namespace GQI_1_EVSCerebrum_GetMnemonics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;
    using GQI_EVSCerebrum_GetMnemonics_1;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.Advanced;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates;

    [GQIMetaData(Name = "EVS Cerebrum Get Mnemonics")]
    public class GQI_EVSCerebrum_GetMnemonics : IGQIDataSource, IGQIOnInit, IGQIInputArguments, IGQIUpdateable
    {
        private readonly GQIStringArgument mnemonicTypeArgument = new GQIStringArgument("Mnemonic Type") { IsRequired = true };
        private MnemonicType mnemonic;

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

            StaticDataProvider.Initialize(args.DMS, dataminerId, elementId, mnemonic);
            _dataProvider = StaticDataProvider.Instance;

            return new OnInitOutputArgs();
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { mnemonicTypeArgument };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            var rawMnemonic = Convert.ToString(args.GetArgumentValue(mnemonicTypeArgument));
            mnemonic = (MnemonicType)Enum.Parse(typeof(MnemonicType), rawMnemonic);

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
            _dataProvider.EndpointTable.Changed += TableData_OnChanged;
            _dataProvider.CategoriesTable.Changed += TableData_OnChanged;
        }

        public void OnStopUpdates()
        {
            _dataProvider.EndpointTable.Changed -= TableData_OnChanged;
            _dataProvider.CategoriesTable.Changed -= TableData_OnChanged;
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

        private void TableData_OnChanged(object sender, ParameterChangeEventMessage e)
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
            var controlSurfaceFilter = new CerebrumFilter(_dataProvider, mnemonic);
            var endPoints = controlSurfaceFilter.GetMnemonics();

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