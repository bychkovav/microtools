namespace Platform.Utils.ServiceBus.Receiving
{
    using System;

    public class HandlerRegistration
    {
        public Type MessageType { get; set; }

        public object Consumer { get; set; }

        public Func<dynamic, dynamic> GetConsumerContextFunc { get; set; }

        public IEndpointInfo EndpointInfo { get; set; }
    }
}
