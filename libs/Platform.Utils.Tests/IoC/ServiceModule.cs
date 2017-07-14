namespace Platform.Utils.Tests.IoC
{
    using System.Configuration;
    using global::Cassandra.Mapping;
    using Ioc;
    using ServiceBus.CassandraRetry;
    using ServiceBus.Core;
    using ServiceBus.Core.Retry;
    using ServiceBus.Core.Retry.Impl;
    using SimpleInjector;

    public class ServiceModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton(typeof(AttemptDataProvider),
             new AttemptDataProvider(ConfigurationManager.ConnectionStrings["Cassandra"].ConnectionString,
                 "publishattempts", new Mappings[] { new SendAttemptsMappings() }));

            container.RegisterSingleton<BeforeSendInterceptor>();
            //container.RegisterSingleton<TestConsumer>();

            container.RegisterSingleton<ISendResultProcessor, DefaultSendResultProcessor>();
            container.RegisterSingleton<ISendAttemptRepository, CassandraAttemptRepository>();

            var builder = PlatformBusFactory.Create(container.GetInstance);
            builder.AddBeforeSendAction<BeforeSendInterceptor>();
            container.RegisterSingleton(typeof(IPlatformBus), () => builder.Bus);

            container.RegisterSingleton<ISendRetryStrategy, BackgroundSendRetryStrategy>();
            container.RegisterSingleton<IConsumeRetryStrategy, BackgroundConsumeRetryStrategy>();
            

            var registration = Lifestyle.Singleton.CreateRegistration<HangfireSendRetryScheduler>(container);
            container.AddRegistration(typeof(IConsumeRetryScheduler),registration);
            container.AddRegistration(typeof(ISendRetryScheduler),registration);
        }
    }
}
