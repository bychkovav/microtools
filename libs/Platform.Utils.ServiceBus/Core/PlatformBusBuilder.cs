namespace Platform.Utils.ServiceBus.Core
{
    using System;
    using System.Collections.Generic;
    using Receiving;
    using Sending;

    public class PlatformBusBuilder
    {
        public PlatformBus Bus { get; private set; }

        public PlatformBusBuilder(PlatformBus bus)
        {
            Bus = bus;
        }

        public PlatformBusBuilder AddConsumerInterceptor<TInterceptor>(IList<Type> typesToDecorate)
            where TInterceptor : IConsumerInterceptor
        {
            ConsumeWrapperProvider.AddConsumerDecorator<TInterceptor>(typesToDecorate);
            return this;
        }

        public PlatformBusBuilder AddConsumerInterceptor<TInterceptor>()
            where TInterceptor : IConsumerInterceptor
        {
            this.AddConsumerInterceptor<TInterceptor>(null);
            return this;
        }

        public PlatformBusBuilder AddBeforeSendAction<TInterceptor>()
            where TInterceptor : IBeforeSend
        {
            this.Bus.AddBeforeSendAction<TInterceptor>();
            return this;
        }

    }
}
