namespace Platform.Utils.ServiceBus.CassandraRetry
{
    using System;
    using System.Collections.Generic;
    using Core.Retry;

    public class CassandraAttemptRepository : ISendAttemptRepository
    {
        private readonly AttemptDataProvider dataProvider;

        public CassandraAttemptRepository(AttemptDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public Guid SaveAttempt(SendAttemptEntity entity)
        {
            this.dataProvider.Mapper.Update(entity);
            return entity.Id;
        }

        public Guid SaveNotSentAttempt(NotSentAttemptEntity entity)
        {
            this.dataProvider.Mapper.Update(entity);
            return entity.Id;
        }

        public IEnumerable<SendAttemptEntity> GetAttemptsByRetryId(Guid retryId)
        {
            return this.dataProvider.Mapper.Fetch<SendAttemptEntity>("WHERE retrymessageid = ?", retryId);
        }

        public NotSentAttemptEntity GetNextNotSent()
        {
            return this.dataProvider.Mapper.FirstOrDefault<NotSentAttemptEntity>("LIMIT 1"); ;
        }

        public void DeleteNotSentAttemptByRetryId(Guid retryId)
        {
            this.dataProvider.Mapper.Delete<NotSentAttemptEntity>("WHERE retrymessageid = ?", retryId);
        }
    }
}
