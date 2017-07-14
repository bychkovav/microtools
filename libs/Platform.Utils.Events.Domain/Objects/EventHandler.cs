namespace Platform.Utils.Events.Domain.Objects
{
    using System.Collections.Generic;
    using Enums;

    public class EventHandler
    {
        public string Name { get; set; }

        public string HandlerClassName { get; set; }

        public string Script { get; set; }

        public CodeBase CodeBase { get; set; }

        public HandlerStackPlace HandlerStackPlace { get; set; }

        public EventData In { get; set; }

        public IList<EventData> Out { get; set; }
    }
}
