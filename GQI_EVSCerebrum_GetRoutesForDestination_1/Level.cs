using Skyline.DataMiner.Net.Messages;
using System.Collections.Generic;

namespace GQI_EVSCerebrum_GetRoutesForDestination_1
{
    internal class Level
    {
        public string Instance { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Mnemonic { get; set; }

        public static List<Level> CreateLevels(ParameterValue[] columns)
        {
            if (columns == null || columns.Length == 0) return new List<Level>();

            var levels = new List<Level>();
            for (int i = 0; i < columns[3].ArrayValue.Length; i++)
            {
                var level = new Level
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Id = columns[3].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Name = columns[4].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Mnemonic = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                };

                levels.Add(level);
            }

            return levels;
        }
    }
}