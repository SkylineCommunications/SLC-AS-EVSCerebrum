namespace GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates
{
    using System;
    using System.Threading;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;

    internal static class StaticDataProvider<T>
    {
        private static Lazy<DataProvider<T>> _lazyDataProvider = new Lazy<DataProvider<T>>(CreateInstance);
        private static GQIDMS _gqiDms;
        private static int _dataminerId;
        private static int _elementId;
        private static int _tableId;

        public static DataProvider<T> Instance => _lazyDataProvider.Value;

        public static void Initialize(GQIDMS gqiDms, int dataminerId, int elementId, int tableId)
        {
            _gqiDms = gqiDms ?? throw new ArgumentNullException(nameof(gqiDms));
            _dataminerId = dataminerId;
            _elementId = elementId;
            _tableId = tableId;
        }

        public static void Reset()
        {
            var newLazy = new Lazy<DataProvider<T>>(CreateInstance);
            var oldLazy = Interlocked.Exchange(ref _lazyDataProvider, newLazy);

            if (oldLazy.IsValueCreated &&
                oldLazy.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private static DataProvider<T> CreateInstance()
        {
            if (_gqiDms == null)
            {
                throw new InvalidOperationException("Initialize method should be called first");
            }

            var connection = CreateConnection(_gqiDms);
            var dataProvider = new DataProvider<T>(connection, _dataminerId, _elementId, _tableId);

            return dataProvider;
        }

        private static Connection CreateConnection(GQIDMS gqiDms)
        {
            var connection = ConnectionHelper.CreateConnection(gqiDms, "EvsCerebrum_GQI (GQIDS)");
            connection.OnClose += (reason) => Reset();
            connection.OnAbnormalClose += (s, e) => Reset();
            connection.OnEventsDropped += (s, e) => Reset();
            connection.OnForcedLogout += (s, e) => Reset();

            return connection;
        }
    }
}
