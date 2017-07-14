namespace Platform.Utils.Events.Domain.Objects.Scripts
{
    using System;

    public class ScriptObject
    {
        public Guid? Id { get; set; }

        public DateTime CreateDate { get; set; }

        public string EventCode { get; set; }

        public string ScriptBody { get; set; }

        public string Description { get; set; }

        public string ScriptName { get; set; }

        public string ScriptType { get; set; }

        public bool IsDefault { get; set; }

        public Guid OwnerContextIdentity { get; set; }
    }
}