namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System;

    public interface ISendRetryScheduler
    {
        void ScheduleSendRetry(TimeSpan delay);
    }
}
