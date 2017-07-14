using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Platform.Utils.MongoDb
{
    public class MongoDataProvider
    {
        private readonly IMongoDatabase database;
        private readonly MongoClient client;

        public MongoDataProvider(string connectionString)
        {
            client = new MongoClient(connectionString);
            var databaseName = new Uri(connectionString).AbsolutePath.Trim('/');

            database = client.GetDatabase(databaseName);
        }

        public async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            //filter by collection name
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            //check for existence
            return (await collections.ToListAsync()).Any();
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }

    }
}
