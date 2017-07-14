namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using Data.Repositories;
    using Newtonsoft.Json.Linq;
    using Services;
    using Utils.Events.ScriptEngine;

    public class DataSourceExtension : IAppProxyExtension
    {
        private readonly TransactonCacheService transactonCacheService;

        private readonly TransactionSlaveRepository transactionSlaveRepository;

        private readonly TransactionalService transactionalService;

        public DataSourceExtension(TransactonCacheService transactonCacheService, TransactionSlaveRepository transactionSlaveRepository, TransactionalService transactionalService)
        {
            this.transactonCacheService = transactonCacheService;
            this.transactionSlaveRepository = transactionSlaveRepository;
            this.transactionalService = transactionalService;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.GetMd = new Func<string, string, JToken>((id, capacity) => this.transactonCacheService.GetMasterData(new Guid(id), capacity).Value);

            container.GetCachedObject = new Func<Guid, JToken>(guid => this.transactonCacheService.GetTransaction(guid).Value);

            container.CacheObject = new Action<JObject>(obj => { this.transactonCacheService.AddTransaction(obj); });

            container.UpdateAdditionalSources = new Action<Guid>((id) =>
            {

                var tr = this.transactionalService.GetSingle(container, (string)container.Context.AgentIdentity, id);
                this.transactonCacheService.AddTransaction(tr);
                this.transactionSlaveRepository.Replace((string)container.Context.AgentIdentity, tr);
            });
        }
    }
}
