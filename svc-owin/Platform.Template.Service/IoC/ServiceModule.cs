namespace Platform.Template.Service.IoC
{
    using Core.ProxyExtensions;
    using SimpleInjector;
    using Utils.Ioc;
    using Utils.Owin.Authorization;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Consumers;
    using Utils.Events.ScriptEngine;
    using Utils.ServiceBus;

    public class ServiceModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<ApplicationProxyBase, EventApplicationProxy>();

            container.RegisterSingleton<IServiceBus, RabbitMqBus>();
            container.RegisterSingleton<EventMessageConsumer>();

            container.RegisterSingleton<MasterDataExtension>();
        }
    }
}