namespace Platform.Utils.Events.Domain
{
    public class SentEventEntity : EventEntityBase
    {
        public virtual string TargetServiceIdentity { get; set; }
    }
}
