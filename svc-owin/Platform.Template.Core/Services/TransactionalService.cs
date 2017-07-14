using Platform.Utils.Diagnostics;

namespace Platform.Template.Core.Services
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Utils.Domain.Objects.Exceptions;
    using Utils.Events.Domain.Objects;
    using Utils.Events.QueryParser.Extensions;
    using Utils.Events.QueryParser.Helpers;
    using Utils.Json;

    public class TransactionalService
    {
        private readonly EvSourceService evSourceService;

        private readonly TransactonCacheService transactonCacheService;

        public TransactionalService(EvSourceService evSourceService, TransactonCacheService transactonCacheService)
        {
            this.evSourceService = evSourceService;
            this.transactonCacheService = transactonCacheService;
        }

        public TransactionalEventContext GetTransactionCtx(dynamic container)
        {
            TransactionalEventContext ctx = container.Context as TransactionalEventContext;
            if (ctx == null)
            {
                throw new Exception("Context should be Transactional");
            }

            return ctx;
        }

        public JToken GetSingle(dynamic container, string dataProviderKey, Guid transactionId)
        {
            using (new StopwatchLog(Metrics.TransactionalServiceGetSingle))
            {
                var state = this.evSourceService.GetState(dataProviderKey, transactionId);
                if (string.IsNullOrEmpty(state?.Data))
                {
                    throw new EntityNotFoundException($"there is no transaction state for id:{transactionId}");
                }

                JObject stateObj = JsonConvert.DeserializeObject<JObject>(state.Data, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                var evs = this.evSourceService.GetTransactionEvents(dataProviderKey, transactionId).OrderBy(x => x.DeltaTimeStamp);

                var edms = evs.Select(x => JsonConvert.DeserializeObject<JObject>(x.Delta, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })).ToList();

                foreach (var edm in edms)
                {
                    stateObj.ApplyPatch(edm);
                }

                this.RemoveDeleted(ObjectHelper.GetInnerObject(stateObj), false);
                this.transactonCacheService.InjectMasterData(stateObj);
                return stateObj;
            }
        }


        private void RemoveDeleted(JToken containerToken, bool nested)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                var deleted = containerToken.SelectToken(ObjectHelper.DeletePropName);
                if (deleted != null)
                {

                    if (deleted.Value<bool>())
                    {
                        if (!nested)
                        {
                            var pivotData = ParserHelper.GetPivotData(containerToken.GetParentProperty().Name);
                            var objCode = pivotData.MainValue;

                            var deletedObj = new JObject
                            {
                                [ObjectHelper.DeletePropName] = true,
                                [ObjectHelper.IdPropName] = ObjectHelper.GetId(containerToken.Root),
                                [ObjectHelper.MasterIdPropName] = ObjectHelper.GetMasterId(containerToken.Root),
                                [pivotData.PivotDefinition.MainProperty] = objCode

                            };
                            containerToken.Root[objCode] = deletedObj;
                        }
                        else
                        {
                            containerToken.Remove();
                        }

                    }
                    return;
                }

                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    RemoveDeleted(child.Value, true);
                }
            }
        }

        public JToken CreateTransaction(string dataProviderKey, JToken obj)
        {
            var result = this.evSourceService.AddState(dataProviderKey, obj);
            return result;
        }
    }
}
