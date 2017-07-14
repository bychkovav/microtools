namespace Platform.Utils.ServiceBus.Receiving
{
    using System;

    public interface IConsumerInterceptor
    {
        void Intercept<TMessage>(IConsumerContext<TMessage> ctx, Action<IConsumerContext<TMessage>> invocation) where TMessage : class;

        bool IsGlobal { get; }
    }
}
