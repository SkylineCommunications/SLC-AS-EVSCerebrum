using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GQI_EVSCerebrum_GetMnemonics_1
{
    internal class EndPoint
    {
        public string Instance { get; set; }

        public string Mnemonic { get; set; }

        public List<string> Categories { get; set; }

        public static List<EndPoint> CreateEndpoints(ParameterValue[] columns)
        {
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
            var row = new GQIRow(new[]
            {
                new GQICell { Value = Instance },
                new GQICell { Value = Mnemonic },
                new GQICell { Value = string.Join(";", Categories) },
            });

            return row;
        }
    }
}
