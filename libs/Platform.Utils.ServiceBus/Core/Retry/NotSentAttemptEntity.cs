namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System;

    /// <summary>
    /// Represents a not-succeeded publish attempt that might be republished.
    /// </summary>
    public class NotSentAttemptEntity
    {
        public Guid Id { get; set; }

        public Guid RetryMessageId { get; set; }

        public string Name { get; set; }

        public string SpecificRoute { get; set; }

        public string ExchangeName { get; set; }

        public string MessageType { get; set; }

        public string MessageJson { get; set; }

        public string MessageHeadersJson { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
