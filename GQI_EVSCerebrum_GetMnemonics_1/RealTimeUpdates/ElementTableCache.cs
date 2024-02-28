namespace GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using SLDataGateway.API.Querying;

    public sealed class ElementTableCache : IDisposable
    {
        private readonly ElementTableWatcher _watcher;

        private readonly ConcurrentDictionary<int, ParameterValue[]> _tableObjectsById = new ConcurrentDictionary<int, ParameterValue[]>();

        private readonly int _dataminerId;
        private readonly int _elementId;
        private readonly int _tableId;

        private readonly GQIDMS _gqiDms;

        public ElementTableCache(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId, int tableId)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (gqiDms is null)
            {
                throw new ArgumentNullException(nameof(gqiDms));
            }

            if (dataminerId == default || elementId == default || tableId == default)
            {
                throw new ArgumentException($"Dataminer ID, element ID and table ID cannot contain any default value.");
            }

            _dataminerId = dataminerId;
            _elementId = elementId;
            _tableId = tableId;
            _gqiDms = gqiDms;

            _watcher = new ElementTableWatcher(connection, dataminerId, elementId, tableId);
            _watcher.Changed += Watcher_OnChanged;

            FillCache();
        }

        public event EventHandler<ParameterChangeEventMessage> Changed;

        public ParameterValue[] GetData()
        {
            return _tableObjectsById.Values.First();
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        private void FillCache()
        {
            var columns = GetTableColumns();
            if (!columns.Any())
            {
                return;
            }

            UpdateCache(columns);
        }

        private void UpdateCache(ParameterValue[] columns)
        {
            _tableObjectsById.Clear();
            _tableObjectsById[_tableId] = columns;
        }

        private ParameterValue[] GetTableColumns()
        {
            var getPartialTableMessage = new GetPartialTableMessage(_dataminerId, _elementId, _tableId, new[] { "forceFullTable=true" });
            var parameterChangeEventMessage = (ParameterChangeEventMessage)_gqiDms.SendMessage(getPartialTableMessage);
            if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
            {
                return new ParameterValue[0];
            }

            var columns = parameterChangeEventMessage.NewValue.ArrayValue;

            int lengthCheck = (_tableId == 14100 || _tableId == 15100) ? 7 : 4;
            if (columns.Length < lengthCheck)
            {
                return new ParameterValue[0];
            }

            return columns;
        }

        private void Watcher_OnChanged(object sender, ParameterChangeEventMessage e)
        {
            UpdateCache(e.NewValue.ArrayValue);

            Changed?.Invoke(this, e);
        }
    }
}
