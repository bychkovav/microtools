﻿namespace Platform.Utils.Events.Domain.Items
{
    using System;

    public class ScriptListItem
    {
        public Guid Id { get; set; }

        public DateTime CreateDate { get; set; }

        public string EventCode { get; set; }

        public string Description { get; set; }

        public string ScriptName { get; set; }

        public string ScriptBody { get; set; }

        public Guid OwnerContextIdentity { get; set; }

        public string ScriptType { get; set; }

        public bool IsDefault { get; set; }
    }
}