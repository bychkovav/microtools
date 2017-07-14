namespace Platform.Extensions.Transactional.Tests.IoC
{
	using ServiceBus.Receiving;
    using SimpleInjector;
    using Utils.Ioc;
    using Utils.Communication.Contracts.Configurator;
    using Utils.ServiceConfiguration;
    using Utils.ServiceConfiguration.Data;

    public class ServiceConfiguratorModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<ExchangeRepository>();

            container.RegisterSingleton<ICurrentConfigurationManager, CurrentConfigurationManager>();

            container.RegisterSingleton<IConsumer<ConfigWrapperDto>, ConfConsumer>();
        }
    }
}
