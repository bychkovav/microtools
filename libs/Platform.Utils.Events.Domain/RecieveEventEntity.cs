namespace Platform.Utils.Events.Domain
{
    using System;

    public class RecieveEventEntity : EventEntityBase
    {
        public virtual string SenderServiceIdentity { get; set; }

        public virtual Guid SenderContextIdentity { get; set; }

        public virtual bool Completed { get; set; } 
    }
}
