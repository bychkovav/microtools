namespace Platform.Template.Core.Services
{
    using System;
    using System.Collections.Generic;
    using Data.Repositories;
    using Domain.EvSourceEntities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Utils.Json;

    public class EvSourceService
    {
        private readonly EvSourceRepository evSourceRepository;

        public EvSourceService(EvSourceRepository evSourceRepository)
        {
            this.evSourceRepository = evSourceRepository;
        }

        public IList<TransactionEventEntity> GetTransactionEvents(string dataProviderKey, Guid transactionId)
        {
            return this.evSourceRepository.GetTransactionEvents(dataProviderKey, transactionId);
        }

        public TransactionEventEntity GetLastEvent(string dataProviderKey, Guid transactionId)
        {
            return this.evSourceRepository.GetLastEvent(dataProviderKey, transactionId);
        }

        public TransactionStateEntity GetState(string dataProviderKey, Guid transactionId)
        {
            return this.evSourceRepository.GetState(dataProviderKey, transactionId);
        }

        public void AddEvent(string dataProviderKey, JToken edm, Guid transactionId)
        {
            var edmStr = JsonConvert.SerializeObject(edm, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            this.evSourceRepository.AddEvent(dataProviderKey, edmStr, transactionId);
        }

        public JToken AddState(string dataProviderKey, JToken rObj)
        {
            if (rObj == null)
            {
                throw new ArgumentNullException(nameof(rObj));
            }

            var transactionId = ObjectHelper.GetId(rObj);
            this.evSourceRepository.AddState(dataProviderKey, rObj, transactionId);
            return rObj;
        }
    }
}
