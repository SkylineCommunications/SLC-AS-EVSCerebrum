using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GQI_EVSCerebrum_GetMnemonics_1
{
    internal class DestinationHandler : Handler
    {
        private GQIDMS dms;

        private int dataminerId;
        private int elementId;

        public DestinationHandler(GQIDMS dms, int dataminerId, int elementId)
        {
            this.dms = dms;
            this.dataminerId = dataminerId;
            this.elementId = elementId;
        }

        public GQIPage CreateDestinationPage()
        {
            var destinations = GetDestinations();
            var destinationCategories = GetDestinationCategories();
            UpdateMnemonicCategories(destinations, destinationCategories);
            var page = CreatePage(destinations);

            return page;
        }

        private List<EndPoint> GetDestinations()
        {
            var columns = GetDestinationTableColumns();
            if (!columns.Any())
            {
                return new List<EndPoint>();
            }

            var destinations = CreateEndpoints(columns);

            return destinations;
        }

        private ParameterValue[] GetDestinationTableColumns()
        {
            var tableId = 15100;
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

        private List<Category> GetDestinationCategories()
        {
            var columns = GetDestinationCategoryTableColumns();
            if (!columns.Any())
            {
                return new List<Category>();
            }

            var categories = CreateCategories(columns);

            return categories;
        }

        private ParameterValue[] GetDestinationCategoryTableColumns()
        {
            var tableId = 17400;
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
