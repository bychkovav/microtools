using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.MongoDb
{
    public class MongoDataProviderPool
    {
        private readonly Dictionary<string, MongoDataProvider> pool = new Dictionary<string, MongoDataProvider>();

        public MongoDataProviderPool()
        {
        }

        public MongoDataProviderPool(Dictionary<string, MongoDataProvider> pool)
        {
            this.pool = pool;
        }

        public void AddProvider(string name, MongoDataProvider dataProvider)
        {
            if (pool.ContainsKey(name))
                throw new DuplicateNameException($"MongoDataProvider named '{name}' has been added already.");

            pool.Add(name, dataProvider);
        }

        public MongoDataProvider GetProvider(string name)
        {
            if (!pool.ContainsKey(name))
                throw new Exception($"MongoDataProvider named '{name}' not found.");

            return pool[name];
        }
    }
}
