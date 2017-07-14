namespace Platform.Utils.Events.Domain
{
    using System;
    using Utils.Domain;

    public class ScriptEntity : EntityBase
    {
        public virtual string EventCode { get; set; }

        public virtual string ScriptType { get; set; }

        public virtual string ScriptBody { get; set; }

        public virtual string Description { get; set; }

        public virtual string ScriptName { get; set; }

        public virtual bool IsDefault { get; set; }

        public virtual Guid OwnerContextIdentity { get; set; }
    }
}