namespace GQI_EVSCerebrum_GetEndpoints_1.RealTimeUpdates
{
    using GQI_EVSCerebrum_GetRoutesForDestination_1;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.SLDataGateway.Types;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class CerebrumFilter
    {
        private readonly DataProvider _dataProvider;
        private readonly MnemonicType _mnemonicType;
        private readonly string _category;

        public CerebrumFilter(DataProvider dataProvider, MnemonicType mnemonicType, string category)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _mnemonicType = mnemonicType;
            _category = category;
        }

        public List<EndPoint> GetEndpoints()
        {
            var endpointColumnData = _mnemonicType == MnemonicType.Sources ? _dataProvider.SourceTable.GetData() : _dataProvider.DestinationsTable.GetData();
            var endPoints = EndPoint.CreateEndpoints(endpointColumnData);

            var categoryColumnData = _mnemonicType == MnemonicType.Sources ? _dataProvider.SourceCategoriesTable.GetData() : _dataProvider.DestinationCategoriesTable.GetData();
            var categories = Category.CreateCategories(categoryColumnData);

            UpdateMnemonicCategories(endPoints, categories);

            if (_mnemonicType == MnemonicType.Destinations)
            {
                var routesColumnData = _dataProvider.RoutesTable.GetData();
                var routes = Route.CreateRoutes(routesColumnData);
                UpdateDestinations(endPoints, routes);
            }

            var filteredEndpoints = FilterEndPointsBasedOnCategory(endPoints);

            return filteredEndpoints.OrderBy(e => e.Mnemonic).ToList();
        }

        private void UpdateMnemonicCategories(List<EndPoint> endpoints, List<Category> categories)
        {
            foreach (var endpoint in endpoints)
            {
                var matchingCategories = categories.Where(c => c.Mnemonic == endpoint.Mnemonic).Select(c => c.Name).ToList();
                endpoint.Categories = matchingCategories;
            }
        }

        private void UpdateDestinations(List<EndPoint> destinations, List<Route> routes)
        {
            foreach (var destination in destinations)
            {
                if (!routes.Any(r => r.Destination == destination.Mnemonic)) continue;

                var firstConnectedSource = routes.Where(r => r.Destination == destination.Mnemonic).OrderByDescending(r => r.DestinationLevel).Select(r => r.Source).FirstOrDefault();
                destination.ConnectedSource = firstConnectedSource;
            }
        }

        private List<EndPoint> FilterEndPointsBasedOnCategory(List<EndPoint> endPoints)
        {
            var result = new List<EndPoint>(endPoints);
            if (!string.IsNullOrWhiteSpace(_category))
            {
                result = endPoints.Where(e => e.Categories != null && e.Categories.Any(c => c == _category)).ToList();
            }

            return result;
        }
    }
}
