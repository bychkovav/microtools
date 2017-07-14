namespace Platform.Utils.ServiceBus.Core.Retry.Impl
{
    using System;
    using EasyNetQ;
    using EasyNetQ.SystemMessages;
    using EasyNetQ.Topology;
    using Newtonsoft.Json;
    using NLog;

    public class BackgroundConsumeRetryStrategy : IConsumeRetryStrategy
    {
        private static readonly object LockObj = new object();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IConsumeRetryScheduler retryScheduler;

        private readonly PlatformBus platformBus;
        private readonly ITypeNameSerializer typeNameSerializer;

        private bool isInitialized = false;

        private IQueue errorQueue;

        /// <summary>
        /// Next pause interval in milliseconds
        /// </summary>
        public long NextSleepInterval { get; }

        public BackgroundConsumeRetryStrategy(IConsumeRetryScheduler retryScheduler, IPlatformBus platformBus)
        {
            this.retryScheduler = retryScheduler;
            this.platformBus = platformBus as PlatformBus;
            if (this.platformBus != null)
            {
                this.typeNameSerializer = this.platformBus.CoreBus.Container.Resolve<ITypeNameSerializer>();
            }
            NextSleepInterval = 15000;
        }

        void Init()
        {
            if (!this.isInitialized)
            {
                lock (LockObj)
                {
                    this.errorQueue =
                         this.platformBus.CoreBus.QueueDeclare(
                             this.platformBus.CoreBus.Container.Resolve<IConventions>().ErrorQueueNamingConvention());

                    if (!this.isInitialized)
                    {
                        this.isInitialized = true;
                    }
                }
            }
        }

        public void RetryConsumerMessage()
        {
            if (this.platformBus == null)
            {
                return;
            }

            try
            {
                Init();

                var nextFailed = this.platformBus.CoreBus.Get<Error>(this.errorQueue);
                if (nextFailed?.MessageAvailable == true)
                {
                    var msgType = this.typeNameSerializer.DeSerialize(nextFailed.Message.Body.BasicProperties.Type);
                    var msg = JsonConvert.DeserializeObject(nextFailed.Message.Body.Message, msgType);

                    this.platformBus.SendSpecific($":{nextFailed.Message.Body.RoutingKey}",
                        nextFailed.Message.Body.RoutingKey, msg, nextFailed.Message.Body.BasicProperties.Headers,
                        nextFailed.Message.Body.Exchange);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
            finally
            {
                this.retryScheduler.ScheduleConsumeRetry(TimeSpan.FromMilliseconds(NextSleepInterval));
            }
        }
    }
}
