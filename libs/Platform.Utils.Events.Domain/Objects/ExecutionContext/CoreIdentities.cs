namespace Platform.Utils.Events.Domain.Objects.ExecutionContext
{
    using System;

    public class CoreIdentities
    {
        public Guid GlobalMasterId { get; set; }

        public Guid GlobalIndexId { get; set; }

        public Guid LocalMasterId { get; set; }

        public Guid LocalOwnerId { get; set; }

        public Guid ContextIdentity { get; set; }
    }
}
