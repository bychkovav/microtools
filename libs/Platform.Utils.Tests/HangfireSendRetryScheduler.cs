using System;

namespace Platform.Utils.Tests
{
    using global::Hangfire;
    using Hangfire;
    using Ioc;
    using ServiceBus.Core.Retry;

    class HangfireSendRetryScheduler : ISendRetryScheduler, IConsumeRetryScheduler
    {
        public void ScheduleSendRetry(TimeSpan delay)
        {
            if (HangfireBootstrapper.Instance.Started)
            {
                BackgroundJob.Schedule(
                    () => IocContainerProvider.CurrentContainer.GetInstance<ISendRetryStrategy>().RetryMessagePublish(),
                    delay);
            }
        }

        public void ScheduleConsumeRetry(TimeSpan delay)
        {
            if (HangfireBootstrapper.Instance.Started)
            {
                BackgroundJob.Schedule(
                    () =>
                        IocContainerProvider.CurrentContainer.GetInstance<IConsumeRetryStrategy>()
                            .RetryConsumerMessage(), delay);
            }
        }
    }
}
