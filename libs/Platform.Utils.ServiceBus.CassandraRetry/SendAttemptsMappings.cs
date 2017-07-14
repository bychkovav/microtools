namespace Platform.Utils.ServiceBus.CassandraRetry
{
    using Core.Retry;
    using global::Cassandra.Mapping;

    public class SendAttemptsMappings : Mappings
    {
        public SendAttemptsMappings()
        {
            this.For<SendAttemptEntity>()
                .TableName("send_attempt")
                .PartitionKey(u => u.RetryMessageId, u => u.CreateDate)
                .Column(u => u.RetryMessageId, cm => cm.WithName("retrymessageid"))
                .Column(u => u.Id, cm => cm.WithName("id"))
                .Column(u => u.Name, cm => cm.WithName("name"))
                .Column(u => u.SpecificRoute, cm => cm.WithName("specificroute"))
                .Column(u => u.ExchangeName, cm => cm.WithName("exchangename"))
                .Column(u => u.MessageJson, cm => cm.WithName("messagejson"))
                .Column(u => u.MessageHeadersJson, cm => cm.WithName("messageheadersjson"))
                .Column(u => u.CreateDate, cm => cm.WithName("createdate"))
                .Column(u => u.Status, cm => cm.WithName("status").WithDbType<int>());

            this.For<NotSentAttemptEntity>()
                .TableName("not_sent_attempt")
                .PartitionKey(u => u.RetryMessageId)
                .Column(u => u.RetryMessageId, cm => cm.WithName("retrymessageid"))
                .Column(u => u.Id, cm => cm.WithName("id"))
                .Column(u => u.Name, cm => cm.WithName("name"))
                .Column(u => u.SpecificRoute, cm => cm.WithName("specificroute"))
                .Column(u => u.ExchangeName, cm => cm.WithName("exchangename"))
                .Column(u => u.MessageJson, cm => cm.WithName("messagejson"))
                .Column(u => u.MessageHeadersJson, cm => cm.WithName("messageheadersjson"))
                .Column(u => u.CreateDate, cm => cm.WithName("createdate"))
                .Column(u => u.MessageType, cm => cm.WithName("messagetype"));
        }
    }
}
