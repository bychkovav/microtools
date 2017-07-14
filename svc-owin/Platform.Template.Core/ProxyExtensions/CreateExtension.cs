using Platform.Utils.Diagnostics;

namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using Services;
    using Utils.Events.Domain.Objects;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Helpers;
    using Utils.Events.QueryParser.Domain.Enums;
    using Utils.Events.QueryParser.Domain.Objects;
    using Utils.Events.ScriptEngine;
    using Utils.Json;

    public class CreateExtension : IAppProxyExtension
    {
        #region [Fields]

        private readonly TransactionalService transactionalService;

        private readonly ModelsHelper modelsHelper;

        private readonly StorageProvider storageProvider;

        private readonly InfraService infraService;

        #endregion

        public CreateExtension(TransactionalService transactionalService, ModelsHelper modelsHelper, InfraService infraService, StorageProvider storageProvider)
        {
            this.transactionalService = transactionalService;
            this.modelsHelper = modelsHelper;
            this.infraService = infraService;
            this.storageProvider = storageProvider;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.Init = new Func<string, PivotData, JToken>((rootCode, initData) =>
            {
                var modelInfo = this.storageProvider.GetModelElementTree(rootCode);
                var model = this.modelsHelper.Init(modelInfo, modelInfo.Code);
                if (initData.PivotDefinition.Type == PivotType.Transaction)
                {
                    return new JObject { [modelInfo.Code] = model };
                }

                return model;
            });

            container.Create = new Func<ModelElementObjectBase, JObject, JToken>((modelInfo, obj) =>
            {
                JObject modelVal = null;
                var context = (EventContext)container.Context;
                using (new StopwatchLog(Metrics.Init))
                {
                    modelVal = this.modelsHelper.Init(modelInfo, modelInfo.Code);
                }

                using (new StopwatchLog(Metrics.Create))
                {
                    var model = new JObject { [modelInfo.Code] = modelVal };

                    this.infraService.AddModifyByObj(obj[modelInfo.Code], context);
                    model.ApplyPatch(obj);

                    var tr = this.transactionalService.CreateTransaction(context.AgentIdentity, model);

                   
                    context.Data = tr;

                    return tr;
                }
            });
        }


    }
}
