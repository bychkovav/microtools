namespace Platform.Utils.Tests
{
    using System;
    using System.Linq;
    using global::NLog;
    using Hangfire;
    using Ioc;
    using Kafka;
    using KafkaNet;
    using KafkaNet.Common;
    using Owin;
    using ServiceBus.CassandraRetry;
    using ServiceBus.Core;

    class TestService : OwinService
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IPlatformBus platformBus;
        private AttemptDataProvider dataProvider;

        public override void Start()
        {
            this.logger.Info("IoC initializing....");
            IocContainerProvider.InitIoc();
            IocContainerProvider.CurrentContainer.Verify();
            this.logger.Info("IoC initialized.");

            PlatformBusFactory.RedButton.Invoke();
            this.logger.Info("Service started");

            this.platformBus = IocContainerProvider.CurrentContainer.GetInstance<IPlatformBus>();
            this.dataProvider = IocContainerProvider.CurrentContainer.GetInstance<AttemptDataProvider>();

            //HangfireBootstrapper.Instance.Start();

            //IocContainerProvider.CurrentContainer.GetInstance<ISendRetryStrategy>().RetryMessagePublish();
            //IocContainerProvider.CurrentContainer.GetInstance<IConsumeRetryStrategy>().RetryConsumerMessage();

            TestOne();
        }

        public async void TestOne()
        {
            var consumer = KafkaBootstrapper.Instance.GetConsumer("test");
            foreach (var message in consumer.Consume())
            {
                Console.WriteLine(message.Value.ToUtf8String());
            }

            //    var res = this.dataProvider.Session.Execute("SELECT * FROM notsentattempt");
            //    Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));

            //    res = this.dataProvider.Session.Execute("SELECT * FROM sendattempt");
            //    Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));

            //this.platformBus.SendAndCreate("EventLocalSend", "4EF69428-FD92-4E0B-B28D-5102C80AD90D",
            //   "3E6D82D2-3C22-4041-96F3-169CCD5823A2", new ExtendSessionMessage());

            ////var global = this.dataProvider.Mapper.Fetch<SendAttemptEntity>();

            ////Console.WriteLine(JsonConvert.SerializeObject(global, Formatting.Indented));

            //PlatformBusFactory.DynamicAddInfrustructure(this.platformBus as PlatformBus, new[]
            //{
            //    new ExchangeItem()
            //    {
            //        Name = "3E6D82D2-3C22-4041-96F3-169CCD5823A2",
            //        Type = "topic",
            //        In = new[]
            //        {
            //            new InMessage()
            //            {
            //                Key ="4EF69428-FD92-4E0B-B28D-5102C80AD90D",
            //                Type = "Platform.Utils.Communication.Contracts.Auth.ExtendSessionMessage, Platform.Utils.Communication.Contracts",
            //                Name = "EventLocalSendConsumer",
            //                React = "Platform.Utils.Tests.TestConsumer, Platform.Utils.Tests"
            //            }

            //        },
            //        Out = new OutMessage[0],
            //        PrefetchCount = 1
            //    }
            //});
        }

        //NOTE: Here dispose resources
        public override void Stop()
        {
            var bus = IocContainerProvider.CurrentContainer.GetInstance<IPlatformBus>();
            bus?.Dispose();

            HangfireBootstrapper.Instance.Stop();

            this.logger.Info("Service stopped");
        }

        public override void Shutdown()
        {
            this.logger.Info("Service shutdown");
        }
    }
}
