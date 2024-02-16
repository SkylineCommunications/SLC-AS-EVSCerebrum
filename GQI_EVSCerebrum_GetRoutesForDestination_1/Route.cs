namespace GQI_EVSCerebrum_GetRoutesForDestination_1
{
    using System;

    internal class Route
    {
        public Route()
        {
        }

        public Route(object[] row)
        {
            Source = Convert.ToString(row[4]);
            SourceLevel = Convert.ToString(row[6]);
            Destination = Convert.ToString(row[8]);
            DestinationLevel = Convert.ToString(row[10]);
        }

        public string Source { get; set; }

        public string SourceLevel { get; set; }

        public string Destination { get; set; }

        public string DestinationLevel { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Source) && !string.IsNullOrWhiteSpace(SourceLevel) && !string.IsNullOrWhiteSpace(Destination) && !string.IsNullOrWhiteSpace(DestinationLevel);
        }
    }
}