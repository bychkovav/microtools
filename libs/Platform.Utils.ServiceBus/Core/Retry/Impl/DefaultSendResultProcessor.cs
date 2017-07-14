namespace Platform.Utils.ServiceBus.Core.Retry.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Configuration;
    using Newtonsoft.Json;
    using NLog;

    public class DefaultSendResultProcessor : ISendResultProcessor
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ISendAttemptRepository attemptRepository;

        private readonly int maxAttemptsCount;

        public DefaultSendResultProcessor(ISendAttemptRepository attemptRepository)
        {
            this.attemptRepository = attemptRepository;
            this.maxAttemptsCount = ServiceBusSection.Current.MaxSendRetries;
        }

        public void ProcessNotSentMessage(string name, string specificRoute, object obj, IDictionary<string, object> headers, string exchangeName)
        {
            if (headers == null)
            {
                this.logger.Error("Null headers");
                return;
            }

            ProcessRetryMessageId(headers);
        }

        private string GetMessageType(object obj)
        {
            var type = obj.GetType();
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        public void ProcessFailedMessage(string name, string specificRoute, object obj, IDictionary<string, object> headers, string exchangeName)
        {
            var retryMessageId = ProcessRetryMessageId(headers);

            var messageJson = JsonConvert.SerializeObject(obj);

            var sendAttempts = this.attemptRepository.GetAttemptsByRetryId(retryMessageId);

            var skipRetry = false;
            var retryCount = sendAttempts?.Count(x => x.Status == SendAttemptEntity.AttemptStatus.NotSent);
            if (retryCount > this.maxAttemptsCount)
            {
                this.logger.Error("Skip trying republish - max attempts count exceeded.");
                //LOG THE MESSAGE
                this.attemptRepository.DeleteNotSentAttemptByRetryId(retryMessageId);
                skipRetry = true;
            }

            var succeededAttempt = new SendAttemptEntity()
            {
                CreateDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                RetryMessageId = retryMessageId,
                MessageHeadersJson = JsonConvert.SerializeObject(headers),
                MessageJson = messageJson,
                Name = name,
                SpecificRoute = specificRoute,
                ExchangeName = exchangeName,
                Status = SendAttemptEntity.AttemptStatus.NotSent
            };
            this.attemptRepository.SaveAttempt(succeededAttempt);

            if (skipRetry)
            {
                return;
            }

            var notSentAttemptEntity = new NotSentAttemptEntity()
            {
                CreateDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                RetryMessageId = retryMessageId,
                MessageHeadersJson = JsonConvert.SerializeObject(headers),
                MessageJson = messageJson,
                Name = name,
                SpecificRoute = specificRoute,
                ExchangeName = exchangeName,
                MessageType = GetMessageType(obj)
            };

            this.attemptRepository.SaveNotSentAttempt(notSentAttemptEntity);
        }

        public void ProcessSentMessage(string name, string specificRoute, object obj, IDictionary<string, object> headers, string exchangeName)
        {
            if (headers == null || !headers.ContainsKey(RetryConstants.RetryMessageIdKey))
            {
                this.logger.Error("Null headers or no RetryMessageId");
                return;
            }

            var retryMessageId = ProcessRetryMessageId(headers);

            this.attemptRepository.DeleteNotSentAttemptByRetryId(retryMessageId);

            var succeededAttempt = new SendAttemptEntity()
            {
                CreateDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                RetryMessageId = retryMessageId,
                MessageHeadersJson = JsonConvert.SerializeObject(headers),
                MessageJson = JsonConvert.SerializeObject(obj),
                Name = name,
                SpecificRoute = specificRoute,
                ExchangeName = exchangeName,
                Status = SendAttemptEntity.AttemptStatus.Sent
            };
            this.attemptRepository.SaveAttempt(succeededAttempt);
        }

        private static Guid ProcessRetryMessageId(IDictionary<string, object> headers)
        {
            Guid retryMessageId = Guid.NewGuid();

            var hasRetryId = headers.ContainsKey(RetryConstants.RetryMessageIdKey);
            if (hasRetryId)
            {
                retryMessageId = Guid.Parse(Encoding.UTF8.GetString((byte[]) headers[RetryConstants.RetryMessageIdKey]));
            }
            else
            {
                headers.Add(RetryConstants.RetryMessageIdKey, Encoding.UTF8.GetBytes(retryMessageId.ToString()));
            }
            return retryMessageId;
        }
    }
}
