namespace Platform.Utils.ServiceBus.Core.Retry.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using Newtonsoft.Json;
    using NLog;

    public class BackgroundSendRetryStrategy : ISendRetryStrategy
    {
        private readonly ISendAttemptRepository attemptRepository;
        private readonly ISendRetryScheduler sendRetryScheduler;
        private readonly IPlatformBus platformBus;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public long NextSleepInterval { get; }

        public BackgroundSendRetryStrategy(ISendAttemptRepository attemptRepository, ISendRetryScheduler sendRetryScheduler, IPlatformBus platformBus)
        {
            this.attemptRepository = attemptRepository;
            this.sendRetryScheduler = sendRetryScheduler;
            this.platformBus = platformBus;
            NextSleepInterval = 15000;
        }

        public void RetryMessagePublish()
        {
            this.logger.Info("BackgroundSendRetryStrategy.RetryConsumerMessage started");
            try
            {
                var nextFailed = this.attemptRepository.GetNextNotSent();
                if (nextFailed != null)
                {
                    var msgType = Type.GetType(nextFailed.MessageType) ?? typeof(ExpandoObject);
                    var msg = JsonConvert.DeserializeObject(nextFailed.MessageJson, msgType);
                    var headers =
                        JsonConvert.DeserializeObject<IDictionary<string, object>>(nextFailed.MessageHeadersJson);
                    this.platformBus.SendSpecific(nextFailed.Name, nextFailed.SpecificRoute, msg, headers,
                        nextFailed.ExchangeName);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
            finally
            {
                this.sendRetryScheduler.ScheduleSendRetry(TimeSpan.FromMilliseconds(NextSleepInterval));
            }
        }
    }
}
