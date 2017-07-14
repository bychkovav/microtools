namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System;
    using System.Collections.Generic;

    public interface ISendAttemptRepository
    {
        Guid SaveAttempt(SendAttemptEntity entity);
        Guid SaveNotSentAttempt(NotSentAttemptEntity entity);

        IEnumerable<SendAttemptEntity> GetAttemptsByRetryId(Guid retryId);
        NotSentAttemptEntity GetNextNotSent();

        void DeleteNotSentAttemptByRetryId(Guid retryId);
    }
}

