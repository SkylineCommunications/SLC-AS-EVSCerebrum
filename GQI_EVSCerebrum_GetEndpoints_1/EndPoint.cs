namespace GQI_EVSCerebrum_GetEndpoints_1
{
    using System.Collections.Generic;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;

    internal class EndPoint
    {
        public string Instance { get; set; }

        public string Mnemonic { get; set; }

        public string ConnectedSource { get; set; }

        public List<string> Categories { get; set; }

        public static List<EndPoint> CreateEndpoints(ParameterValue[] columns)
        {
            if (columns == null || columns.Length == 0) return new List<EndPoint>();

            var endPoints = new List<EndPoint>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var nameValue = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue();
                if (string.IsNullOrWhiteSpace(nameValue) || nameValue == "Not initialized") continue;

                var endPoint = new EndPoint
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Mnemonic = nameValue,
                };

                endPoints.Add(endPoint);
            }

            return endPoints;
        }

        public GQIRow ToRow()
        {
            var cells = new[]
            {
                new GQICell { Value = Instance ?? string.Empty },
                new GQICell { Value = Mnemonic ?? string.Empty },
                new GQICell { Value = Categories != null ? string.Join(";", Categories) : string.Empty },
                new GQICell { Value = ConnectedSource != null ? ConnectedSource : string.Empty },
            };

            var row = new GQIRow(Instance, cells);

            return row;
        }
    }
}
