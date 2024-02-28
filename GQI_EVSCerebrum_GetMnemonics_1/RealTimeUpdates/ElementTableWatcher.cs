namespace GQI_EVSCerebrum_GetMnemonics_1.RealTimeUpdates
{
    using System;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.SubscriptionFilters;

    public sealed class ElementTableWatcher : IDisposable
    {
        private readonly Connection _connection;
        private readonly SubscriptionFilter _subscriptionFilter;

        public ElementTableWatcher(Connection connection, int dataminerId, int elementId, int tableId)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.OnNewMessage += Connection_OnNewMessage;

            _subscriptionFilter = new SubscriptionFilterParameter(typeof(ParameterChangeEventMessage).Name, new[] { "forceFullTable=true" }, dataminerId, elementId, tableId, index: null);
            _connection.AddSubscription("1", _subscriptionFilter);

            _connection.Subscribe();
        }

        public event EventHandler<ParameterChangeEventMessage> Changed;

        public void Dispose()
        {
            try
            {
                _connection.RemoveSubscription("1", _subscriptionFilter);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Connection_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            if (e.Message is ParameterChangeEventMessage tableChange)
            {
                Changed?.Invoke(this, tableChange);
            }
        }
    }
}
