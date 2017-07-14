using Platform.Utils.Diagnostics;
using Platform.Utils.Events.QueryParser.Domain.Enums;
using Platform.Utils.Events.QueryParser.Helpers;

namespace Platform.Template.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using NHibernate.Util;
    using NLog;
    using Utils.Domain.Objects;
    using Utils.Events.QueryParser.Builders.JsonLinq.Extensions;
    using Utils.Json;
    using Utils.TransactionCache;

    public class TransactonCacheService
    {
        private const string CapacityPropertyName = "capacity";
        private readonly TransactionCacheCore transactionCacheCore;
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public TransactonCacheService(TransactionCacheCore transactionCacheCore)
        {
            this.transactionCacheCore = transactionCacheCore;
        }

        public ExecutionResult AddTransaction(JObject transaction)
        {
            using (new StopwatchLog(Metrics.TransactonCacheServiceAddTransaction))
            {
                this.transactionCacheCore.AddMaster(transaction);
                return new ExecutionResult();
            }
        }

        public ExecutionResult<JObject> GetTransaction(Guid id)
        {
            return new ExecutionResult<JObject>(this.transactionCacheCore.GetLast(id));
        }

        public ExecutionResult<JObject> GetMasterData(Guid id, string capacity)
        {
            var jObject = this.transactionCacheCore.GetLast(id);
            if (jObject == null)
            {
                this.logger.Error($"No master data from cache: {id}, {capacity}");
                return new ExecutionResult<JObject>();
            }
            return new ExecutionResult<JObject>((JObject)ObjectHelper.GetInnerObject(jObject).ToMd(capacity));
        }

        public ExecutionResult InjectMasterData(JObject transaction)
        {
            transaction.Traverse(ProcessJObject);

            return new ExecutionResult();
        }

        public ExecutionResult InjectMasterData(IList<JObject> transactions)
        {
            foreach (var transaction in transactions)
            {
                InjectMasterData(transaction);
            }
            return new ExecutionResult();
        }

        private void ProcessJObject(JObject jObject)
        {
            var array = jObject.Parent as JArray;
            var wrapper = array?.Parent as JProperty;
            if (wrapper != null && ParserHelper.GetPivotData(wrapper.Name).PivotDefinition.Type == PivotType.MasterData)
            {
                Guid masterId = ObjectHelper.GetMasterId(jObject);
                if (masterId == Guid.Empty)
                {
                    this.logger.Error($"Masterdata hotswap error. masterId was not found in: {jObject}");
                    return;
                }
                var capacity = jObject[CapacityPropertyName];
                if (capacity.Type != JTokenType.String)
                {
                    this.logger.Error($"Masterdata hotswap error. Capacity is not a string in: {jObject}");
                    return;
                }
                var capacityVal = capacity.ToString();
                if (string.IsNullOrEmpty(capacityVal))
                {
                    this.logger.Error($"Masterdata hotswap error. Capacity is empty in: {jObject}");
                    return;
                }
                var actualData = GetMasterData(masterId, capacityVal);
                if (actualData?.Value == null)
                {
                    this.logger.Error(
                        $"Masterdata hotswap error. Masterdata in cache not found: localMasterId:{masterId}");
                    return;
                }

                jObject.Children().ToList().ForEach(x => x.Remove());

                actualData.Value.Add("Marker", "Marker");

                actualData.Value.Children().ForEach(jObject.Add);
            }
        }
    }
}
