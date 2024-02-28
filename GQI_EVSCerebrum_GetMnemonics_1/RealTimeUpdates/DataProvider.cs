namespace GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Protobuf.Shared.IdObjects.v1;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class DataProvider : IDisposable
    {
        public DataProvider(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId, MnemonicType mnemonicType)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            int endpointTableToSubscribeOn = mnemonicType == MnemonicType.Sources ? 14100 : 15100;
            int categoryTableToSubscribeOn = mnemonicType == MnemonicType.Sources ? 17300 : 17400;

            EndpointTable = new ElementTableCache(connection, gqiDms, dataminerId, elementId, endpointTableToSubscribeOn);
            CategoriesTable = new ElementTableCache(connection, gqiDms, dataminerId, elementId, categoryTableToSubscribeOn);
        }

        public ElementTableCache EndpointTable { get; }

        public ElementTableCache CategoriesTable { get; }

        public void Dispose()
        {
            EndpointTable.Dispose();
            CategoriesTable.Dispose();
        }
    }
}
