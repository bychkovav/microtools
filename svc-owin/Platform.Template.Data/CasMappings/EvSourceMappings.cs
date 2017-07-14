namespace Platform.Template.Data.CasMappings
{
    using Cassandra.Mapping;
    using Domain.EvSourceEntities;

    public class EvSourceMappings : Mappings
    {
        public EvSourceMappings()
        {
            this.For<TransactionEventEntity>()
                .TableName("transactionEvent")
                .PartitionKey(u => u.Id)
                .ClusteringKey(u => u.DeltaTimeStamp, SortOrder.Descending)
                .Column(u => u.Delta, cm => cm.WithName("delta"));

            this.For<TransactionStateEntity>()
                .TableName("transactionState")
                .PartitionKey(u => u.Id)
                .Column(u => u.Data, cm => cm.WithName("data"));
        }
    }
}
