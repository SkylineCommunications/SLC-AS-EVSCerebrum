namespace GQI_EVSCerebrum_GetEndpoints_1.RealTimeUpdates
{
    using GQI_EVSCerebrum_GetRoutesForDestination_1;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class CerebrumFilter
    {
        private readonly DataProvider _dataProvider;
        private readonly string _destination;

        public CerebrumFilter(DataProvider dataProvider, string destination)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _destination = destination;

            Initialize();
        }

        public List<Level> AllLevels { get; set; } = new List<Level>();

        public List<Route> AllRoutes { get; set; } = new List<Route>();

        public List<DestinationAssociation> AllDestinationAssociations { get; set; } = new List<DestinationAssociation>();

        public List<Route> GetRoutes()
        {
            Initialize();

            return FilterRoutesBasedOnDestinationAssociation();
        }

        private void Initialize()
        {
            CreateLevels();
            CreateDestinationAssociations();
            CreateRoutes();
        }

        private void CreateRoutes()
        {
            var routesColumnData = _dataProvider.RoutesTable.GetData();
            AllRoutes = Route.CreateRoutes(routesColumnData);
        }

        private void CreateDestinationAssociations()
        {
            var destinationAssoiationsColumnData = _dataProvider.DestinationsAssociationsTable.GetData();
            AllDestinationAssociations = DestinationAssociation.CreateDestinationAssociations(destinationAssoiationsColumnData);
        }

        private void CreateLevels()
        {
            var levelColumnData = _dataProvider.LevelsTable.GetData();
            AllLevels = Level.CreateLevels(levelColumnData);
        }

        private List<Route> FilterRoutesBasedOnDestinationAssociation()
        {
            var associationsLinkedToDestinations = AllDestinationAssociations.Where(a => a.DestinationInstance == _destination).ToList();

            var routesAlignedWithDestinationAssociations = new List<Route>();
            foreach (var destinationAssociation in associationsLinkedToDestinations.OrderBy(d => d.Instance))
            {
                var route = AllRoutes.FirstOrDefault(r => r.Instance == destinationAssociation.Instance);
                if (route is null || !route.IsValid())
                {
                    var level = AllLevels.FirstOrDefault(lvl => destinationAssociation.DisplayKey != null && destinationAssociation.DisplayKey.Contains(lvl.Mnemonic));

                    // get levels for destination
                    route = new Route
                    {
                        Instance = level?.Instance,
                        Destination = destinationAssociation.DestinationName,
                        DestinationLevel = level?.Mnemonic,
                        Source = String.Empty,
                        SourceLevel = String.Empty,
                    };
                }

                routesAlignedWithDestinationAssociations.Add(route);
            }

            return routesAlignedWithDestinationAssociations.OrderByDescending(x => x.DestinationLevel).ToList();
        }

        private void Log(int items)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(@"C:\Skyline_Data\RealTimeUpdates.txt"))
                {
                    sw.WriteLine($"Category: {_destination}, Filtered items: {items}");
                }
            }
            catch (Exception)
            {

            }

        }
    }
}
