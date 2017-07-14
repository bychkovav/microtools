using Platform.Utils.Diagnostics;

namespace Platform.Template.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Newtonsoft.Json.Linq;
    using Utils.Events.QueryParser;
    using Utils.Events.QueryParser.Builders.MongoDb;
    using Utils.Events.QueryParser.Domain.Enums;
    using Utils.Events.QueryParser.Domain.Objects;
    using Utils.Events.QueryParser.Extensions.Fluent;
    using Utils.Json;
    using Utils.MongoDb;
    using Utils.MongoDb.Conversion;

    public class TransactionSlaveRepository
    {
        private readonly MongoDataProviderPool dataProviderPool;

        protected virtual string CollectionName => "transactions";

        private const string IdPropertyMongoName = "_id";

        public TransactionSlaveRepository(MongoDataProviderPool dataProviderPool)
        {
            this.dataProviderPool = dataProviderPool;
        }

        public async void Insert(string dataProviderKey, JObject obj)
        {
            var bsonDocument = obj.SerializeToBsonDocument();

            var transactionsCollection =
                this.dataProviderPool.GetProvider(dataProviderKey).GetCollection<BsonDocument>(this.CollectionName);

            await transactionsCollection.InsertOneAsync(bsonDocument);
        }

        public List<JObject> GetTransactions(string dataProviderKey, SingleQuery singleQuery)
        {
            var builder = new MongoFilterBuilder<BsonDocument>();

            // Add "Deleted" filter
            singleQuery.NodesList
                .First
                .AddCriteria(CriteriaAppendType.And, ObjectHelper.DeletePropName, CriteriaComparator.NotEq, true);

            var transactionsCollection =
             this.dataProviderPool.GetProvider(dataProviderKey).GetCollection<BsonDocument>(this.CollectionName);

            var documentsFinder = builder.RenderQuery(singleQuery, transactionsCollection);

            var documents = documentsFinder.ToList();

            var jobjects = documents.DeserializeToJObject();

            return jobjects;
        }

        public List<JObject> GetTransactions(string dataProviderKey, string filterQuery)
        {
            var engine = new Engine();
            var singleQuery = engine.Parse(filterQuery);

            return GetTransactions(dataProviderKey, singleQuery);
        }

        public async void Replace(string dataProviderKey, JObject obj)
        {
            using (new StopwatchLog(Metrics.TransactionSlaveRepositoryReplace))
            {
                var masterId = ObjectHelper.GetMasterId(obj);

                var filterBuilder = new FilterDefinitionBuilder<BsonDocument>();

                var bson = obj.SerializeToBsonDocument();
                var filter = filterBuilder.Eq(x => x[IdPropertyMongoName], masterId);

                var transactionsCollection =
                    this.dataProviderPool.GetProvider(dataProviderKey).GetCollection<BsonDocument>(this.CollectionName);

                await transactionsCollection.ReplaceOneAsync(filter, bson, new UpdateOptions { IsUpsert = true }); 
            }
        }

        public async void Delete(string dataProviderKey, Guid id)
        {
            var filterBuilder = new FilterDefinitionBuilder<BsonDocument>();

            var filter = filterBuilder.Eq(x => x[IdPropertyMongoName], id);

            var transactionsCollection =
             this.dataProviderPool.GetProvider(dataProviderKey).GetCollection<BsonDocument>(this.CollectionName);

            await transactionsCollection.DeleteOneAsync(filter);
        }
    }
}
