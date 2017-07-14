namespace Platform.Utils.Events.Domain
{
    using System;
    using Utils.Domain;

    public class EventEntityBase : EntityBase
    {
        public virtual Guid TargetContextIdentity { get; set; }

        public virtual Guid EventUniqueId { get; set; }

        public virtual Guid CorrelationId { get; set; }

        public virtual string EventCode { get; set; }

        public virtual string Content { get; set; }

        public virtual string ContentType { get; set; }
    }
}
