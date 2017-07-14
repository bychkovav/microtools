namespace Platform.Template.Domain.EvSourceEntities
{
    using System;

    public class TransactionEventEntity
    {
        public DateTime DeltaTimeStamp { get; set; }

        public string Delta { get; set; }

        public Guid Id { get; set; }
    }
}
