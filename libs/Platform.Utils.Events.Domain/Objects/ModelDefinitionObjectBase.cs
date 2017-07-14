using System;

namespace Platform.Utils.Events.Domain.Objects
{
    public class ModelDefinitionObjectBase
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string NamePlural { get; set; }

        public string JsonStructure { get; set; }
    }
}
