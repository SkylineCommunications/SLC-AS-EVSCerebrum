namespace GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.SLDataGateway.Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CerebrumFilter
    {
        private readonly DataProvider _dataProvider;
        private readonly MnemonicType _mnemonicType;

        public CerebrumFilter(DataProvider dataProvider, MnemonicType mnemonicType)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _mnemonicType = mnemonicType;
        }

        public List<EndPoint> GetMnemonics()
        {
            var endpointColumnData = _dataProvider.EndpointTable.GetData();
            var endPoints = EndPoint.CreateEndpoints(endpointColumnData);

            var categoryColumnData = _dataProvider.CategoriesTable.GetData();
            var categories = Category.CreateCategories(categoryColumnData);

            UpdateMnemonicCategories(endPoints, categories);

            return endPoints;
        }

        private void UpdateMnemonicCategories(List<EndPoint> endpoints, List<Category> categories)
        {
            foreach (var endpoint in endpoints)
            {
                var matchingCategories = categories.Where(c => c.Mnemonic == endpoint.Mnemonic).Select(c => c.Name).ToList();
                endpoint.Categories = matchingCategories;
            }
        }
    }
}
