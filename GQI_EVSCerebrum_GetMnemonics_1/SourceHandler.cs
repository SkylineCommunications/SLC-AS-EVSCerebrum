namespace GQI_EVSCerebrum_GetMnemonics_1
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class SourceHandler : Handler
    {
        private GQIDMS dms;

        private int dataminerId;
        private int elementId;

        public SourceHandler(GQIDMS dms, int dataminerId, int elementId)
        {
            this.dms = dms;
            this.dataminerId = dataminerId;
            this.elementId = elementId;
        }

        public GQIPage CreateSourcesPage()
        {
            var sources = GetSources();
            var sourceCategories = GetSourceCategories();
            UpdateMnemonicCategories(sources, sourceCategories);
            var page = CreatePage(sources);

            return page;
        }

        private List<EndPoint> GetSources()
        {
            var columns = GetSourcesTableColumns();
            if (!columns.Any())
            {
                return new List<EndPoint>();
            }

            var sources = CreateEndpoints(columns);

            return sources;
        }

        private ParameterValue[] GetSourcesTableColumns()
        {
            var tableId = 14100;
            var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
            var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
            if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
            {
                return new ParameterValue[0];
            }

            var columns = parameterChangeEventMessage.NewValue.ArrayValue;
            if (columns.Length < 7)
            {
                return new ParameterValue[0];
            }

            return columns;
        }

        private List<Category> GetSourceCategories()
        {
            var columns = GetSourceCategoryTableColumns();
            if (!columns.Any())
            {
                return new List<Category>();
            }

            var categories = CreateCategories(columns);

            return categories;
        }

        private ParameterValue[] GetSourceCategoryTableColumns()
        {
            var tableId = 17300;
            var getPartialTableMessage = new GetPartialTableMessage(dataminerId, elementId, tableId, new[] { "forceFullTable=true" });
            var parameterChangeEventMessage = (ParameterChangeEventMessage)dms.SendMessage(getPartialTableMessage);
            if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
            {
                return new ParameterValue[0];
            }

            var columns = parameterChangeEventMessage.NewValue.ArrayValue;
            if (columns.Length < 4)
            {
                return new ParameterValue[0];
            }

            return columns;
        }
    }
}
