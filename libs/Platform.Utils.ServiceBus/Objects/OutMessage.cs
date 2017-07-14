namespace Platform.Utils.ServiceBus.Objects
{
    using System;

    public class OutMessage
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public bool? Persist { get; set; }

        public TimeSpan? Timeout { get; set; }
    }
}
