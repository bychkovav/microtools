namespace Platform.Utils.Events.Domain.Filters
{
    using System;
    using Utils.Domain;

    public class ScriptFilter : FilterBase
    {
        public string ScriptName { get; set; }

        public string EventCode { get; set; }

        public Guid? OwnerContextIdentity { get; set; }

        public string ScriptType { get; set; }
    }
}