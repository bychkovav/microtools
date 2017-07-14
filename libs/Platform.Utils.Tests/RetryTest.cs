namespace Platform.Utils.Tests
{
    using System;
    using Ioc;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using ServiceBus.CassandraRetry;
    using ServiceBus.Core;
    using ServiceBus.Objects;

    [TestFixture]
    public class RetryTest : TestBase
    {
        private IPlatformBus platformBus;
        private AttemptDataProvider dataProvider;

        public override void SetUp()
        {
            base.SetUp();
            this.platformBus = IocContainerProvider.CurrentContainer.GetInstance<IPlatformBus>();
            this.dataProvider = IocContainerProvider.CurrentContainer.GetInstance<AttemptDataProvider>();
        }

        [Test]
        public void TestOne()
        {
            //    var res = this.dataProvider.Session.Execute("SELECT * FROM notsentattempt");
            //    Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));

            //    res = this.dataProvider.Session.Execute("SELECT * FROM sendattempt");
            //    Console.WriteLine(JsonConvert.SerializeObject(res, Formatting.Indented));

            //this.platformBus.SendAndCreate("EventLocalSend", "4EF69428-FD92-4E0B-B28D-5102C80AD90D",
            //   "3E6D82D2-3C22-4041-96F3-169CCD5823A2", new ExtendSessionMessage());

            //var global = this.dataProvider.Mapper.Fetch<SendAttemptEntity>();

            //Console.WriteLine(JsonConvert.SerializeObject(global, Formatting.Indented));

            PlatformBusFactory.DynamicAddInfrustructure(this.platformBus as PlatformBus, new[]
            {
                new ExchangeItem()
                {
                    Name = "3E6D82D2-3C22-4041-96F3-169CCD5823A2",
                    Type = "topic",
                    In = new[]
                    {
                        new InMessage()
                        {
                            Key ="4EF69428-FD92-4E0B-B28D-5102C80AD90D",
                            Type = "Platform.Utils.Communication.Contracts.Auth.ExtendSessionMessage, Platform.Utils.Communication.Contracts",
                            Name = "EventLocalSendConsumer",
                            React = "Platform.Utils.Tests.TestConsumer, Platform.Utils.Tests"
                        }

                    },
                    Out = new OutMessage[0],
                    PrefetchCount = 1
                }
            });

            Console.Read();
        }
    }
}
