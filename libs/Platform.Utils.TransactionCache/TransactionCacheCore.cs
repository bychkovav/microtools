namespace Platform.Utils.TransactionCache
{
    using System;
    using Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Utils.Cassandra;
    using Utils.Json;

    public class TransactionCacheCore
    {
        private readonly CassandraDataProvider cassandraDataProvider;

        public TransactionCacheCore(CassandraDataProvider cassandraDataProvider)
        {
            this.cassandraDataProvider = cassandraDataProvider;
        }

        public JObject GetLast(Guid masterId)
        {
            var master = this.cassandraDataProvider.Mapper.FirstOrDefault<MasterEntity>("SELECT * FROM masterdata WHERE masterid = ?", masterId);
            return JsonConvert.DeserializeObject<JObject>(master.Data);
        }

        public JObject GetVersion(Guid masterId, double version)
        {
            var master = this.cassandraDataProvider.Mapper.FirstOrDefault<MasterEntity>("SELECT * FROM masterdata WHERE masterid = ? AND version = ?", masterId, version);
            return JsonConvert.DeserializeObject<JObject>(master.Data);
        }

        public void AddMaster(JObject obj)
        {
            this.cassandraDataProvider.Mapper.Update(new MasterEntity()
            {
                Version = ObjectHelper.GetVersion(obj),
                MasterId = ObjectHelper.GetMasterId(obj),
                Data = obj.ToString(),
            });
        }
    }
}
