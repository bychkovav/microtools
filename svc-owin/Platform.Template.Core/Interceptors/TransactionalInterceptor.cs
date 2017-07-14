using System;

namespace Platform.Template.Core.Interceptors
{
    using Newtonsoft.Json.Linq;
    using Utils.Domain.Objects;
    using Utils.Events.Domain.Objects;
    using Utils.Events.Manager.Interfaces;
    using Utils.Json;

    public class TransactionalInterceptor : IEventInterceptor
    {
        public ExecutionResult PreProcess(EventContext ectx)
        {
            var trContext = ectx as TransactionalEventContext;
            if (trContext == null)
            {
                throw new Exception("Context should be type of TransactionalEventContext");
            }

            if (ectx.Data.Type == JTokenType.Object)
            {
                trContext.TransactionId = ObjectHelper.GetId(ectx.Data);
            }
            return new ExecutionResult();
        }

        public bool SupportEvent(string eventKey)
        {
            return true;
        }

        public int Order => int.MinValue;
    }
}
