namespace Platform.Utils.Events.Contracts
{
    using System;
    using Enums;

    public class EventMessage
    {
        public EventMessage() { }

        public EventMessage(EventMessage mes)
        {
            EventUniqueId = mes.EventUniqueId;
            EventCode = mes.EventCode;
            Content = mes.Content;
            ContentType = mes.ContentType;
            DebugId = mes.DebugId;
            EventType = mes.EventType;
        }

        public EventType EventType { get; set; }

        public Guid EventUniqueId { get; set; }

        public string EventCode { get; set; }

        public string Content { get; set; }

        public string ContentType { get; set; }

        public string DebugId { get; set; }
    }
}
