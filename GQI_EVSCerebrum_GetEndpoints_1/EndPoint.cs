using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GQI_EVSCerebrum_GetEndpoints_1
{
    internal class EndPoint
    {
        public string Instance { get; set; }

        public string Mnemonic { get; set; }

        public List<string> Categories { get; set; }

        public static List<EndPoint> CreateEndpoints(ParameterValue[] columns)
        {
            if (columns == null || columns.Length == 0) return new List<EndPoint>();

            var endPoints = new List<EndPoint>();

            Log(columns);

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
            var row = new GQIRow(new[]
            {
                new GQICell { Value = Instance ?? string.Empty },
                new GQICell { Value = Mnemonic ?? string.Empty },
                new GQICell { Value = Categories != null ? string.Join(";", Categories) : string.Empty },
            });

            return row;
        }

        private static void Log(ParameterValue[] columns)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(@"C:\Skyline_Data\RealTimeUpdates.txt"))
                {
                    sw.WriteLine($"Endpoint class: Colums length: {columns.Length}");
                }
            }
            catch (Exception)
            {

            }
            
        }
    }
}
