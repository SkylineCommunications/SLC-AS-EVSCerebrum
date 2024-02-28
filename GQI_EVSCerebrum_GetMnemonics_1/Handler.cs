namespace GQI_EVSCerebrum_GetMnemonics_1
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class Handler
    {
        protected List<EndPoint> CreateEndpoints(ParameterValue[] columns)
        {
            var mnemonics = new List<EndPoint>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var nameValue = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue();
                if (string.IsNullOrWhiteSpace(nameValue) || nameValue == "Not initialized") continue;

                var endPoints = new EndPoint
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Mnemonic = nameValue,
                };

                mnemonics.Add(endPoints);
            }

            return mnemonics;
        }

        protected List<Category> CreateCategories(ParameterValue[] columns)
        {
            var categories = new List<Category>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var category = new Category
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Name = columns[1].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Mnemonic = columns[3].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                };

                categories.Add(category);
            }

            return categories;
        }

        protected void UpdateMnemonicCategories(List<EndPoint> mnemonics, List<Category> categories)
        {
            foreach (var mnemonic in mnemonics)
            {
                var matchingCategories = categories.Where(c => c.Mnemonic == mnemonic.Mnemonic).Select(c => c.Name).ToList();
                mnemonic.Categories = matchingCategories;
            }
        }

        protected GQIPage CreatePage(List<EndPoint> mnemonics)
        {
            var rows = new List<GQIRow>();

            foreach (var mnemonic in mnemonics.OrderBy(d => d.Mnemonic))
            {
                var row = new GQIRow(
                    new[]
                    {
                    new GQICell { Value = mnemonic.Instance },
                    new GQICell { Value = mnemonic.Mnemonic },
                    new GQICell { Value = string.Join(";", mnemonic.Categories) },
                    });

                rows.Add(row);
            }

            return new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };
        }
    }
}
