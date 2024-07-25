namespace GQI_EVSCerebrum_GetEndpoints_1.RealTimeUpdates
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Protobuf.Shared.IdObjects.v1;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class DataProvider : IDisposable
    {
        private readonly int _dataminerId;
        private readonly int _elementId;

        private readonly GQIDMS _gqiDms;
        private readonly Connection _connection;

        public DataProvider(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId)
        {
            _connection = connection;
            _dataminerId = dataminerId;
            _elementId = elementId;
            _gqiDms = gqiDms;

            InstantiateCache();
        }

        public ElementTableCache SourceTable { get; private set; }

        public ElementTableCache DestinationsTable { get; private set; }

        public ElementTableCache SourceCategoriesTable { get; private set; }

        public ElementTableCache DestinationCategoriesTable { get; private set; }

        public ElementTableCache RoutesTable { get; private set; }

        private void InstantiateCache()
        {
            if (_connection == null)
            {
                throw new ArgumentNullException(nameof(_connection));
            }

            SourceTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 14100, "1");
            SourceCategoriesTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 17300, "3");

            DestinationsTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 15100, "2");
            DestinationCategoriesTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 17400, "4");

            RoutesTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 12100, "5");
        }

        public void Dispose()
        {
            SourceTable?.Dispose();
            SourceCategoriesTable?.Dispose();
            DestinationsTable?.Dispose();
            DestinationCategoriesTable?.Dispose();
            RoutesTable?.Dispose();
        }
    }
}
