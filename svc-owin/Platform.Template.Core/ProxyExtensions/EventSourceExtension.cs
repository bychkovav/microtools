using Platform.Utils.Diagnostics;

namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Data.Repositories;
    using Newtonsoft.Json.Linq;
    using Services;
    using Utils.Events.Domain.Objects;
    using Utils.Events.QueryParser.Builders.JsonLinq;
    using Utils.Events.QueryParser.Domain.Objects;
    using Utils.Events.ScriptEngine;
    using Utils.Json;

    public class EventSourceExtension : IAppProxyExtension
    {
        #region [Fields]

        private readonly TransactionalService transactionalService;

        private readonly TransactionSlaveRepository transactionSlaveRepository;

        private readonly EvSourceService evSourceService;

        private readonly InfraService infraService;

        #endregion

        public EventSourceExtension(TransactionalService transactionalService, EvSourceService evSourceService, TransactionSlaveRepository transactionSlaveRepository, InfraService infraService)
        {
            this.transactionalService = transactionalService;
            this.evSourceService = evSourceService;
            this.transactionSlaveRepository = transactionSlaveRepository;
            this.infraService = infraService;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.Query = new Func<SingleQuery, IList<JObject>>((singleQuery) =>
            {
                using (new StopwatchLog(Metrics.Query))
                {

                    var transactions = this.transactionSlaveRepository.GetTransactions((string)container.Context.AgentIdentity, singleQuery);
                    return transactions;
                }
            });

            container.Get = new Func<SingleQuery, JToken>((filterQuery) =>
            {
                using (new StopwatchLog(Metrics.Get))
                {
                    var ctx = this.transactionalService.GetTransactionCtx(container);
                    // Get transaction from event source
                    JToken transaction = this.transactionalService.GetSingle(container, (string)container.Context.AgentIdentity, ctx.TransactionId);

                    IList<JToken> result = JsonLinqExecutor.GetExecutor.Run(new[] { filterQuery }, transaction, (ExpandoObject)container);

                    return result.Any() ? result.First() : null;
                }
            });

            container.DeltaUpdate = new Func<JToken, JToken>(edm =>
            {
                TransactionalEventContext ctx = this.transactionalService.GetTransactionCtx(container);
                using (new StopwatchLog(Metrics.DeltaUpdate))
                {
                    //NOTE:TODO: Generate ids? 
                    var operation = edm.FindOperations().First();
                    this.infraService.AddModifyByObj(operation, ctx);

                    this.evSourceService.AddEvent((string)container.Context.AgentIdentity, edm, ctx.TransactionId);
                    return edm;
                }
            });
        }
    }
}