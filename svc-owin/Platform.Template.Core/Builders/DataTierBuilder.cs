using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Template.Data;
using Platform.Template.Data.CasMappings;
using Platform.Utils.MongoDb;
using SimpleInjector;

namespace Platform.Template.Core.Builders
{
    public class DataTierBuilder
    {
        public void BuildDataObjects(Container container, string identity)
        {
            container.RegisterSingleton(typeof(MongoDataProvider),
              new MongoDataProvider(System.Configuration.ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString));


            container.RegisterSingleton(typeof(TransactionalCasDataProvider),
            new TransactionalCasDataProvider(ConfigurationManager.ConnectionStrings["CassandraCs"].ConnectionString,
                 ConfigurationManager.AppSettings["cassandraKeyspace"], new Cassandra.Mapping.Mappings[] { new EvSourceMappings() }));
        }
    }
}
