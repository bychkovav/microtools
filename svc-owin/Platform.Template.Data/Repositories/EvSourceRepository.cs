namespace Platform.Template.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cassandra.Mapping;
    using Domain.EvSourceEntities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Utils.Cassandra;
    using Utils.Diagnostics;

    public class EvSourceRepository
    {
        private readonly CassandraDataProviderPool dataProviderPool;

        public EvSourceRepository(CassandraDataProviderPool dataProviderPool)
        {
            this.dataProviderPool = dataProviderPool;
        }

        public IList<TransactionEventEntity> GetTransactionEvents(string dataProviderKey, Guid transactionId)
        {
            var transactionalCasDataProvider = this.dataProviderPool.GetProvider(dataProviderKey);

            return transactionalCasDataProvider.Mapper.Fetch<TransactionEventEntity>(
                    "SELECT * FROM transactionEvent WHERE id = ?", transactionId).ToList();
        }

        public TransactionEventEntity GetLastEvent(string dataProviderKey, Guid transactionId)
        {
            var transactionalCasDataProvider = this.dataProviderPool.GetProvider(dataProviderKey);

            return transactionalCasDataProvider.Mapper.FirstOrDefault<TransactionEventEntity>(
                "SELECT * FROM transactionEvent WHERE id = ?", transactionId);
        }

        public TransactionStateEntity GetState(string dataProviderKey, Guid transactionId)
        {
            var transactionalCasDataProvider = this.dataProviderPool.GetProvider(dataProviderKey);

            return transactionalCasDataProvider.Mapper.FirstOrDefault<TransactionStateEntity>(
               "SELECT * FROM transactionState WHERE id = ?", transactionId);
        }

        public void AddEvent(string dataProviderKey, string deltaJson, Guid transactionId)
        {
            TransactionEventEntity n = new TransactionEventEntity() { Delta = deltaJson, DeltaTimeStamp = DateTime.UtcNow, Id = transactionId };
            var transactionalCasDataProvider = this.dataProviderPool.GetProvider(dataProviderKey);
            using (new StopwatchLog(Metrics.CassGet))
            {
                transactionalCasDataProvider.Mapper.InsertAsync(n, CqlQueryOptions.New());
            }
        }

        public void AddState(string dataProviderKey, JToken data, Guid transactionId)
        {
            var dataStr = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            TransactionStateEntity n = new TransactionStateEntity() { Data = dataStr, Id = transactionId };
            var transactionalCasDataProvider = this.dataProviderPool.GetProvider(dataProviderKey);
            using (new StopwatchLog(Metrics.CassAdd))
            {
                transactionalCasDataProvider.Mapper.InsertAsync(n, CqlQueryOptions.New());
            }
        }
    }
}
