namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System;

    public interface IConsumeRetryScheduler
    {
        void ScheduleConsumeRetry(TimeSpan delay);
    }
}
