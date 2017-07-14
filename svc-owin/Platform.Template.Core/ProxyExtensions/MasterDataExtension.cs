namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using Data.Repositories;
    using Newtonsoft.Json.Linq;
    using Utils.Domain.Objects;
    using Utils.Events.QueryParser.Domain.Objects;
    using Utils.Events.ScriptEngine;

    public class MasterDataExtension : IAppProxyExtension
    {
        private readonly TransactionSlaveRepository masterDataRepository;

        public MasterDataExtension(TransactionSlaveRepository masterDataRepository)
        {
            this.masterDataRepository = masterDataRepository;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.QueryMasterData = new Func<SingleQuery, JToken, ExecutionResult<dynamic>>((singleQuery, obj) =>
            {
                var transactions = this.masterDataRepository.GetTransactions("masterdata", singleQuery);

                return new ExecutionResult<dynamic>(transactions);
            });
        }
    }
}
