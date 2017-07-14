namespace Platform.Utils.TransactionCache.Mappings
{
    using Entities;
    using global::Cassandra.Mapping;

    public class MasterMappings : Mappings
    {
        public MasterMappings()
        {
            For<MasterEntity>()
                .TableName("masterdata")
                .PartitionKey(u => u.MasterId)
                .ClusteringKey(u => u.Version, SortOrder.Descending)
                .Column(u => u.Data, cm => cm.WithName("data"));
        }
    }
}
