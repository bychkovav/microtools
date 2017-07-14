namespace Platform.Utils.ServiceBus.CassandraRetry
{
    using System.Collections.Generic;
    using global::Cassandra.Mapping;
    using Utils.Cassandra;

    public class AttemptDataProvider : CassandraDataProvider
    {
        public AttemptDataProvider(string connectionString, string keyspace, IList<Mappings> mappings)
            : base(connectionString, keyspace, mappings)
        {
        }
    }
}
