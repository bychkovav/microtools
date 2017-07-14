namespace Platform.Template.Service.IoC
{
    using System.Configuration;
    using Core.Interceptors;
    using Core.ProxyExtensions;
    using Core.Services;
    using Data;
    using Data.CasMappings;
    using Data.Repositories;
    using SimpleInjector;
    using Utils.Cassandra;
    using Utils.Events.Manager.Helpers;
    using Utils.Ioc;
    using Utils.MongoDb;
    using Utils.TransactionCache;
    using Utils.TransactionCache.Mappings;

    public class TransactionalModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<TransactionalInterceptor>();

            container.RegisterSingleton<CreateExtension>();
            container.RegisterSingleton<QueryLangOperationsExtension>();
            container.RegisterSingleton<DataSourceExtension>();

            container.RegisterSingleton<EvSourceService>();

            container.RegisterSingleton<TransactionSlaveRepository>();

            container.RegisterSingleton<TransactionCacheCore>();
            container.RegisterSingleton<ModelsHelper>();

            container.RegisterSingleton<TransactonCacheService>();
            container.RegisterSingleton<TransactionalService>();
            container.RegisterSingleton<InfraService>();

            container.RegisterSingleton<TransactionalDelegate>();

            container.RegisterSingleton<EvSourceRepository>();

            container.RegisterSingleton<MongoDataProviderPool>();
            container.RegisterSingleton<CassandraDataProviderPool>();

            
        }
    }
}
