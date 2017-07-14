using Platform.Utils.Events.Domain.Enums;

namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using Services;
    using Utils.Events.Domain.Objects;
    using Utils.Events.ScriptEngine;
    using Utils.Events.Dispatcher;

    public class EventDispatcherDelegate : IAppProxyExtension
    {
        private readonly EventDispatcher eventDispatcher;

        private readonly TransactionalService transactionalService;

        public EventDispatcherDelegate(EventDispatcher eventDispatcher, TransactionalService transactionalService)
        {
            this.eventDispatcher = eventDispatcher;
            this.transactionalService = transactionalService;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.ProcessIncomingMessage = new Action(() =>
            {
                TransactionalEventContext context = this.transactionalService.GetTransactionCtx(container);
                var eventMessage = context.Event;
                this.eventDispatcher.ProcessIncomingMessage(eventMessage, StaticCorrelationTypes.Api);
            });
        }
    }
}
