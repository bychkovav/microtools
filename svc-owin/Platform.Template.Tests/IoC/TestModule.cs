namespace Platform.Template.Tests.IoC
{
    using Core.ProxyExtensions;
    using Core.Services;
    using SimpleInjector;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Consumers;
    using Utils.Events.ScriptEngine;
    using Utils.Ioc;
    using Utils.ServiceBus;
    using Utils.TransactionCache;

    public class TestModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
//            container.RegisterSingleton<OperationsExtension>();
            container.RegisterSingleton<DataSourceExtension>();
            container.RegisterSingleton<TransactonCacheService>();
            container.RegisterSingleton<TransactionCacheCore>();

            container.RegisterSingleton<ProxyMock>();
            container.RegisterSingleton<EvSourceService>();

            //container.RegisterSingleton<CryptoProvider>(new CryptoProvider(string.Empty, string.Empty));


            container.RegisterSingleton<ApplicationProxyBase, EventApplicationProxy>();

            //var builder = PlatformBusFactory.Create(container.GetInstance);
            //container.RegisterSingleton(typeof(IPlatformBus), () => builder.Bus);
            container.RegisterSingleton<IServiceBus, RabbitMqBus>();
            container.RegisterSingleton<EventMessageConsumer>();
        }
    }
}
