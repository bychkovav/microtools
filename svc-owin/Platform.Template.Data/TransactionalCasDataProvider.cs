namespace Platform.Template.Data
{
    using System.Collections.Generic;
    using Cassandra.Data.Linq;
    using Domain.EvSourceEntities;
    using Utils.Cassandra;

    public class TransactionalCasDataProvider : CassandraDataProvider
    {
        public TransactionalCasDataProvider(string connectionString, string keyspace,
            IList<Cassandra.Mapping.Mappings> mappings) : base(connectionString, keyspace, mappings)
        {
            var eventTable = new Table<TransactionEventEntity>(this.Session);
            eventTable.CreateIfNotExists();

            var stateTable = new Table<TransactionStateEntity>(this.Session);
            stateTable.CreateIfNotExists();
        }
    }
}
