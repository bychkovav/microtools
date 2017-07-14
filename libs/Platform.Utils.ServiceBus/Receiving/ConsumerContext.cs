namespace Platform.Utils.ServiceBus.Receiving
{
    using System;
    using System.Threading.Tasks;
    using EasyNetQ;
    using Newtonsoft.Json;

    public class ConsumerContext<T> : IConsumerContext<T> where T : class
    {
        private readonly IEndpointInfo endpointInfo;
        private readonly IMessage<T> message;

        public IEndpointInfo EndpointInfo
        {
            get { return this.endpointInfo; }
        }

        public Type ConsumerType { get; set; }

        public IMessage<T> Message
        {
            get { return this.message; }
        }

        public ConsumerContext(IEndpointInfo endpointInfo, IMessage<T> message)
        {
            this.endpointInfo = endpointInfo;
            this.message = message;
        }

        public Task Reply<TResponse>(TResponse result)
            where TResponse : class
        {
            if (!this.message.Properties.CorrelationIdPresent)
            {
                throw new ArgumentException("correlationId");
            }
            if (!this.message.Properties.ReplyToPresent)
            {
                throw new ArgumentException("reply to");
            }
            IMessage<TResponse> response = result != null ? new Message<TResponse>(result) : new Message<TResponse>(JsonConvert.DeserializeObject<TResponse>("{}"));
            response.Properties.CorrelationId = this.message.Properties.CorrelationId;
            //NOTE: false for immidiate because of EasyNetQ bug
            return EndpointInfo.Bus.PublishAsync(EndpointInfo.Exchange, this.message.Properties.ReplyTo,
                                                          true, false, response);
        }
    }
}
