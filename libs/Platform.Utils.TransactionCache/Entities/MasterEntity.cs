namespace Platform.Utils.TransactionCache.Entities
{
    using System;

    public class MasterEntity
    {
        public double Version { get; set; }

        public Guid MasterId { get; set; }

        public string Data { get; set; }
    }
}
