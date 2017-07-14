namespace Platform.Template.Tests.IoC
{
    using System.Configuration;
    using System.Linq;
    using Cassandra.Mapping;
    using Data;
    using Data.CasMappings;
    using Data.Repositories;
    using SimpleInjector;
    using Utils.Cassandra;
    using Utils.Ioc;
    using Utils.MongoDb;
    using Utils.Owin.Configuration;
    using Utils.TransactionCache.Mappings;

    public class DataModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton(typeof(MongoDataProvider),
               new MongoDataProvider(ConfigurationManager.ConnectionStrings["MongoDb"].ConnectionString));

            container.RegisterSingleton<TransactionSlaveRepository>();

            container.RegisterSingleton(typeof(TransactionalCasDataProvider),
                new TransactionalCasDataProvider(ConfigurationManager.ConnectionStrings["CassandraCs"].ConnectionString,
                    "VersionStorage", new Mappings[] {new EvSourceMappings()}));
            
            container.RegisterSingleton(typeof(CassandraDataProvider),
               new CassandraDataProvider(ConfigurationManager.ConnectionStrings["TransactionCache"].ConnectionString,
                   "TransactionCache", new Mappings[] { new MasterMappings() }));
        }
    }
}
