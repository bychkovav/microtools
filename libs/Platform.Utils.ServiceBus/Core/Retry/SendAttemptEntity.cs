namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System;

    /// <summary>
    /// Represents an event that describes an attempt to publish message. Both failed and succeeded.
    /// </summary>
    public class SendAttemptEntity : ICloneable
    {
        public Guid Id { get; set; }

        public Guid RetryMessageId { get; set; }

        public string Name { get; set; }

        public string SpecificRoute { get; set; }

        public string ExchangeName { get; set; }

        public string MessageJson { get; set; }

        public string MessageHeadersJson { get; set; }

        public DateTime CreateDate { get; set; }

        public AttemptStatus Status { get; set; }

        public enum AttemptStatus
        {
            Sent = 0,
            NotSent = 1
        }

        public object Clone()
        {
            return new SendAttemptEntity()
            {
                RetryMessageId = this.RetryMessageId,
                Name = this.Name,
                SpecificRoute = this.SpecificRoute,
                ExchangeName = this.ExchangeName,
                MessageJson = this.MessageJson,
                MessageHeadersJson = this.MessageHeadersJson,
                CreateDate = this.CreateDate,
                Status = this.Status,
            };
        }
    }
}
