using Platform.Utils.Diagnostics;

namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using Services;
    using Utils.Events.Domain.Definitions;
    using Utils.Events.Domain.Enums;
    using Utils.Events.Domain.Objects;
    using Utils.Events.Manager;
    using Utils.Events.QueryParser.Domain.Enums;
    using Utils.Events.QueryParser.Extensions.Fluent;
    using Utils.Events.QueryParser.FilterationObjectParser;
    using Utils.Events.ScriptEngine;
    using Utils.Json;
    using Platform.Utils.Events.Manager.Extensions;
    using Platform.Utils.Events.Manager.Helpers;

    public class TransactionalDelegate : IAppProxyExtension
    {
        private readonly TransactionalService transactionalService;

        private readonly StorageProvider storageProvider;

        private readonly ModelsHelper modelHelper;

        public TransactionalDelegate(TransactionalService transactionalService, StorageProvider storageProvider, ModelsHelper modelHelper)
        {
            this.transactionalService = transactionalService;
            this.storageProvider = storageProvider;
            this.modelHelper = modelHelper;
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.ProcessEdm = new Action<JToken>(edm =>
            {
                using (new StopwatchLog(Metrics.ProcessEdm))
                {
                    TransactionalEventContext context = this.transactionalService.GetTransactionCtx(container);

                    EventAction? currentAction = null;
                    string eventActionName = string.Empty;

                    var modelName = edm.GetEdmMainPropName();
                    var modelInfo = this.storageProvider.GetModelElementTree(modelName);

                    if (EventHandlerDefinitions.StandardActionsStrings.ContainsKey(context.Event.EventAction))
                    {
                        currentAction = EventHandlerDefinitions.StandardActionsStrings[context.Event.EventAction];
                        eventActionName = EventHandlerDefinitions.StandardActionsNames[currentAction.Value][1];
                    }
                    else
                    {
                        var current = GetCurrentElementInfo(context.Event.EventPath, modelInfo);
                        var customAction = current.CustomActions.First(x => x.ActionName == context.Event.EventAction);

                        currentAction = customAction.ActionType;
                        eventActionName = customAction.ActionNamePast;
                    }

                    switch (currentAction)
                    {
                        case EventAction.Initiate:
                            var m = new JObject { [context.Event.EventPath] = GetTransactionalOperations(edm) };
                            var res = container.Create(modelInfo, m);
                            container.UpdateAdditionalSources(ObjectHelper.GetId(res));
                            container.FireEvent(context.Event.EventPath, eventActionName, string.Empty, res);
                            context.Data = res;
                            break;

                        case EventAction.Set:
                            UpdateEvSource(edm, container, context, eventActionName);
                            context.Data = edm;
                            break;

                        case EventAction.Delete:
                            UpdateEvSource(edm, container, context, eventActionName);
                            context.Data = edm;
                            break;

                        case EventAction.Add:
                            var operation = GetTransactionalOperations(edm);
                            var initItem = this.modelHelper.Init(modelInfo, context.Event.EventPath);
                            foreach (var child in initItem.Children<JProperty>())
                            {
                                operation[child.Name] = child.Value;
                            }
                            UpdateEvSource(edm, container, context, eventActionName);
                            context.Data = edm;
                            break;

                        case EventAction.Get:
                            var obj = container.Get((new EDMObjectParser().Parse(edm as JObject)).AddMethod(QueryMethodType.Get));
                            container.FireEvent(context.Event.EventPath, eventActionName, string.Empty, obj);
                            context.Data = obj;
                            break;

                        case EventAction.Query:
                            //TODO:NOTE: Here we need to get single query from EDM.
                            var objs = container.Query(new FilterParser().Parse(edm as JObject));
                            container.FireEvent(context.Event.EventPath, eventActionName, string.Empty, objs);
                            context.Data = new JArray((IList<JObject>)objs);
                            break;

                        default:
                            break;
                    } 
                }
            });
        }

        private ModelElementObjectBase GetCurrentElementInfo(string path, ModelElementObjectBase root)
        {
            var current = root.FindInTreeByPath(path, true);
            if (current == null)
            {
                throw new Exception("No such model element");
            }

            return current;
        }

        private JToken GetTransactionalOperations(JToken edm)
        {
            var operations = edm.FindOperations();

            if (operations.Count != 1)
            {
                throw new Exception("Just only 1 operation per event possible");
            }

            return operations.First();
        }

        private void UpdateEvSource(JToken edm, dynamic container, TransactionalEventContext context, string eventActionToSend)
        {
            var result = container.DeltaUpdate(edm);
            container.UpdateAdditionalSources(context.TransactionId);
            FireEvents(edm, result, eventActionToSend, context.Event.EventPath, container);
        }

        private void FireEvents(JToken edm, JToken result, string eventActionToSend, string path, dynamic conainer)
        {
            var operation = GetTransactionalOperations(edm);
            var changed = operation.GetWayFromLeaf();


            conainer.FireEvent(path, eventActionToSend, string.Empty, result);

            while (changed.Count > 0)
            {
                var currentNode = changed.Pop() as JProperty;
                if (currentNode != null && !(currentNode.Value is JArray))
                {
                    conainer.FireEvent(currentNode.Name, EventHandlerDefinitions.StandardActionsNames[EventAction.Update][1], string.Empty, result);
                }
            }
        }
    }
}
