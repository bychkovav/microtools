namespace Platform.Utils.Events.QueryGenerator.Domain
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Platform.Utils.Events.Domain.Enums;

    public class ApiActionInfo
    {
        public IList<string> PathItems { get; set; } = new List<string>();

        public JObject EdmObject { get; set; }

        public JObject PlaceForBody { get; set; }

        public EventAction EventAction { get; set; }

        public string Path => string.Join(".", PathItems);
    }
}