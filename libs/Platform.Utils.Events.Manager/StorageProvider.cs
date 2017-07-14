namespace Platform.Utils.Events.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Objects;
    using Newtonsoft.Json;
    using Platform.Utils.Events.Manager.Interfaces;
    using Platform.Utils.Events.QueryGenerator.Interfaces;
    using Platform.Utils.Redis;
    using StackExchange.Redis;
    using Utils.Domain.Objects.Exceptions;
    using EventHandler = Domain.Objects.EventHandler;

    public class StorageProvider : IModelElementStorage
    {
        #region [Fields]

        private readonly RedisDatabase redis;

        #endregion

        public StorageProvider(RedisDatabase redis)
        {
            this.redis = redis;
        }

        #region [Get methods]

        public IList<EventHandler> GetServiceHandlers(string serviceIdentity)
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetHandlersKey(serviceIdentity));
            if (!string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<IList<EventHandler>>(result);
            }

            return new List<EventHandler>();
        }

        public IList<ServiceInfo> GetEventSenders(string eventCode)
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetEventSendersKey(eventCode));
            if (!string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<IList<ServiceInfo>>(result);
            }

            return new List<ServiceInfo>();
        }

        public EventDetails GetEventDetails(string eventCode)
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetEventDetailsKey(eventCode));
            if (!string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<EventDetails>(result);
            }

            return new EventDetails();
        }

        public IList<ServiceInfo> GetEventListeners(string eventCode)
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetEventListenersKey(eventCode));
            if (!string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<IList<ServiceInfo>>(result);
            }

            return new List<ServiceInfo>();
        }

        public IList<EventData> GetAllEvents()
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetEventsKey());
            var list = string.IsNullOrEmpty(result)
               ? new List<EventData>()
               : JsonConvert.DeserializeObject<List<EventData>>(result);

            return list;
        }

        public IList<ServiceInfo> GetAllServices()
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetServicesKey());
            var list = string.IsNullOrEmpty(result)
               ? new List<ServiceInfo>()
               : JsonConvert.DeserializeObject<List<ServiceInfo>>(result);

            return list;
        }

        public ModelDefinitionObjectBase GetRootModel(string modelName)
        {
            IDatabase db = this.redis.Get();
            var result = db.StringGet(GetRootModelKey(modelName));
            if (!string.IsNullOrEmpty(result))
            {
                return JsonConvert.DeserializeObject<ModelDefinitionObjectBase>(result);
            }
            else
            {
                result = db.StringGet(GetRootModelPluralKey(modelName));
                if (!string.IsNullOrEmpty(result))
                {
                    return JsonConvert.DeserializeObject<ModelDefinitionObjectBase>(result);
                }
            }

            return null;
        }

        private IList<ModelElementObjectBase> GetModelElements(Guid modelDefinitionId)
        {
            IDatabase db = this.redis.Get();
            var modelElementKey = GetModelElementKey(modelDefinitionId.ToString());
            var result = db.ListRange(modelElementKey);
            return
                result.Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => JsonConvert.DeserializeObject<ModelElementObjectBase>(x))
                    .ToList();
        }

        #region IModelElementStorage Members

        public ModelElementObjectBase GetModelElementTree(string modelName)
        {
            var rootModel = GetRootModel(modelName);

            return GetModelElementTree(rootModel.Id);
        }

        public ModelElementObjectBase GetModelElementTree(Guid modelDefinitionId)
        {
            var list = GetModelElements(modelDefinitionId);

            var dictionary = list.ToDictionary(x => x.Id, x => x);

            var children = list.Where(x => x.ParentId.HasValue)
                .GroupBy(x => x.ParentId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var element in list)
            {
                if (element.ParentId.HasValue && dictionary.ContainsKey(element.ParentId.Value))
                {
                    element.Parent = dictionary[element.ParentId.Value];
                }

                element.Children = children.ContainsKey(element.Id)
                    ? children[element.Id]
                    : new List<ModelElementObjectBase>();
            }

            return list.FirstOrDefault(x => x.ParentId.HasValue == false);
        }

        #endregion

        #endregion

        #region [Delete methods]

        public void FlushEvents()
        {
            this.redis.Flush();
        }

        public void DeleteEvent(string eventCode)
        {
            IDatabase db = this.redis.Get();
            var allEvents = GetAllEvents();
            var matchEvent = allEvents.FirstOrDefault(x => x.EventCode == eventCode);
            if (matchEvent != null)
            {
                allEvents.Remove(matchEvent);
                db.StringSet(GetEventsKey(), JsonConvert.SerializeObject(allEvents));
            }

            var listeneers = GetEventListeners(eventCode);
            var senders = GetEventSenders(eventCode);

            List<ServiceInfo> all = listeneers.ToList();
            all.AddRange(senders);

            foreach (var serviceInfo in all)
            {
                var handlers = GetServiceHandlers(serviceInfo.ServiceIdentity);
                foreach (var eventHandler in handlers)
                {
                    if (eventHandler.Out.Any(x => x.EventCode == eventCode))
                    {
                        RemoveFromHandler(eventHandler, serviceInfo.ServiceIdentity, new[] { eventCode }, true);
                    }

                    if (eventHandler.In.EventCode == eventCode)
                    {
                        RemoveFromHandler(eventHandler, serviceInfo.ServiceIdentity, new[] { eventCode }, false);
                    }
                }
            }
        }

        public void DeleteRootModel(string name)
        {
            IDatabase db = this.redis.Get();
            db.KeyDelete(GetRootModelKey(name));
        }

        public void DeleteService(string serviceIdentity)
        {
            IDatabase db = this.redis.Get();
            var allServices = GetAllServices();
            var service = allServices.FirstOrDefault(x => x.ServiceIdentity == serviceIdentity);
            if (service == null)
            {
                throw new EntityNotFoundException("No such service");
            }

            var handlers = GetServiceHandlers(serviceIdentity) ?? new List<EventHandler>();

            foreach (var eventHandler in handlers)
            {
                ClearHandler(eventHandler, serviceIdentity);
                db.KeyDelete(GetHandlersKey(serviceIdentity));
            }

            allServices.Remove(service);

            db.StringSet(GetServicesKey(), JsonConvert.SerializeObject(allServices));
        }

        public void DeleteHandler(string serviceIdentity, string handlerName)
        {
            IDatabase db = this.redis.Get();
            var service = GetAllServices().FirstOrDefault(x => x.ServiceIdentity == serviceIdentity);
            if (service == null)
            {
                throw new EntityNotFoundException("No such service");
            }

            var handlers = GetServiceHandlers(serviceIdentity) ?? new List<EventHandler>();
            var handler = handlers.FirstOrDefault(x => x.Name == handlerName);
            if (handler == null)
            {
                throw new EntityNotFoundException(string.Format("There is no such handler with name {0} for service {1}", handlerName, serviceIdentity));
            }

            ClearHandler(handler, serviceIdentity);

            handlers.Remove(handler);
            db.StringSet(GetHandlersKey(serviceIdentity), JsonConvert.SerializeObject(handlers));
        }

        public void UnmapHandlerFromEvent(string code, string serviceIdentity, string handlerName, bool isOut)
        {
            IDatabase db = this.redis.Get();
            var service = GetAllServices().FirstOrDefault(x => x.ServiceIdentity == serviceIdentity);
            if (service == null)
            {
                throw new EntityNotFoundException("No such service");
            }

            var handlers = GetServiceHandlers(serviceIdentity) ?? new List<EventHandler>();
            var handler = handlers.FirstOrDefault(x => x.Name == handlerName);
            if (handler == null)
            {
                throw new EntityNotFoundException(string.Format("There is no such handler with name {0} for service {1}", handlerName, serviceIdentity));
            }

            RemoveFromHandler(handler, serviceIdentity, new[] { code }, isOut);
            db.StringSet(GetHandlersKey(serviceIdentity), JsonConvert.SerializeObject(handlers));
        }

        private void ClearHandler(EventHandler handler, string identity)
        {
            RemoveFromHandler(handler, identity, handler.Out.Select(x => x.EventCode).ToList(), true);
            RemoveFromHandler(handler, identity, new[] { handler.In.EventCode }, false);
        }

        private void RemoveFromHandler(EventHandler handler, string identity, IEnumerable<string> codes, bool isOut)
        {
            IDatabase db = this.redis.Get();
            IList<EventData> events = isOut ? handler.Out : new[] { handler.In };
            foreach (var code in codes)
            {
                var e = events.FirstOrDefault(x => x.EventCode == code);
                if (e != null)
                {
                    events.Remove(e);
                }

                if (isOut)
                {
                    var senders = GetEventSenders(code);
                    var sender = senders.FirstOrDefault(x => x.ServiceIdentity == identity);
                    if (sender != null)
                    {
                        senders.Remove(sender);
                        db.StringSet(GetEventSendersKey(code), JsonConvert.SerializeObject(senders));
                    }
                }
                else
                {
                    var listeners = GetEventListeners(code);
                    var listener = listeners.FirstOrDefault(x => x.ServiceIdentity == identity);
                    if (listener != null)
                    {
                        listeners.Remove(listener);
                        db.StringSet(GetEventListenersKey(code), JsonConvert.SerializeObject(listeners));
                    }
                }
            }
        }

        #endregion

        #region [Add methods]

        public void SaveEvent(EventData eventData)
        {
            IDatabase db = this.redis.Get();
            var list = GetAllEvents();
            var exist = list.FirstOrDefault(x => x.EventCode == eventData.EventCode);
            if (exist == null)
            {
                list.Add(eventData);
            }
            else
            {
                exist.EventCode = eventData.EventCode;
            }

            db.StringSet(GetEventsKey(), JsonConvert.SerializeObject(list));
        }

        public void SaveRootModel(ModelDefinitionObjectBase model)
        {
            IDatabase db = this.redis.Get();
            db.StringSet(GetRootModelKey(model.Name), JsonConvert.SerializeObject(model));
            if (!string.IsNullOrEmpty(model.NamePlural))
            {
                db.StringSet(GetRootModelPluralKey(model.NamePlural), JsonConvert.SerializeObject(model));
            }
        }

        public void SaveModelElements(Guid modelDefinitionId, IList<ModelElementObjectBase> items)
        {
            IDatabase db = this.redis.Get();
            var modelElementKey = GetModelElementKey(modelDefinitionId.ToString());
            db.KeyDelete(modelElementKey);
            foreach (var modelElementObjectBase in items)
            {
                db.ListRightPush(modelElementKey, JsonConvert.SerializeObject(modelElementObjectBase));
            }
        }

        public void MapHandlerToEvent(EventData ev, string serviceIdentity, string handlerName, bool isOut)
        {
            IDatabase db = this.redis.Get();
            var service = GetAllServices().FirstOrDefault(x => x.ServiceIdentity == serviceIdentity);
            if (service == null)
            {
                throw new EntityNotFoundException("No such service");
            }


            var handlers = GetServiceHandlers(serviceIdentity) ?? new List<EventHandler>();
            var handler = handlers.FirstOrDefault(x => x.Name == handlerName);
            if (handler == null)
            {
                throw new EntityNotFoundException(string.Format("There is no such handler with name {0} for service {1}", handlerName, serviceIdentity));
            }

            var neEv = new EventData() { EventCode = ev.EventCode };
            if (isOut)
            {
                if (handler.Out == null)
                {
                    handler.Out = new List<EventData>();
                }

                if (handler.Out.All(x => x.EventCode != ev.EventCode))
                {
                    handler.Out.Add(neEv);

                }
            }
            else
            {
                handler.In = neEv;
            }

            db.StringSet(GetHandlersKey(serviceIdentity), JsonConvert.SerializeObject(handlers));

            if (isOut)
            {
                AddEventSender(ev.EventCode, service);
            }
            else
            {
                AddEventListener(ev.EventCode, service);
            }
        }

        public void SaveService(ServiceInfo serviceInfo)
        {
            IDatabase db = this.redis.Get();
            var list = GetAllServices();
            var exist = list.FirstOrDefault(x => x.ServiceIdentity == serviceInfo.ServiceIdentity);
            if (exist == null)
            {
                list.Add(serviceInfo);
            }
            else
            {
                exist.ServiceIdentity = serviceInfo.ServiceIdentity;
                exist.ServiceName = serviceInfo.ServiceName;
            }

            db.StringSet(GetServicesKey(), JsonConvert.SerializeObject(list));
        }

        public void SaveEventDetails(EventDetails det)
        {
            IDatabase db = this.redis.Get();

            var details = GetEventDetails(det.EventCode);
            details.DebugId = det.DebugId;
            db.StringSet(GetEventDetailsKey(det.EventCode), JsonConvert.SerializeObject(details));
        }

        public void SaveHandler(EventHandler handler, string serviceIdentity)
        {
            IDatabase db = this.redis.Get();
            var handlers = GetServiceHandlers(serviceIdentity) ?? new List<EventHandler>();

            var exist = handlers.FirstOrDefault(x => x.Name == handler.Name);
            if (exist == null)
            {
                handlers.Add(new EventHandler()
                {
                    Out = new List<EventData>(),
                    Name = handler.Name,
                    HandlerStackPlace = handler.HandlerStackPlace,
                    CodeBase = handler.CodeBase,
                    HandlerClassName = handler.HandlerClassName,
                    Script = handler.Script
                });
            }

            db.StringSet(GetHandlersKey(serviceIdentity), JsonConvert.SerializeObject(handlers));
        }

        private void AddEventListener(string code, ServiceInfo serviceInfo)
        {
            IDatabase db = this.redis.Get();
            var listeners = GetEventListeners(code);

            if (listeners.All(x => x.ServiceIdentity != serviceInfo.ServiceIdentity))
            {
                listeners.Add(serviceInfo);
                db.StringSet(GetEventListenersKey(code), JsonConvert.SerializeObject(listeners));
            }
        }

        private void AddEventSender(string code, ServiceInfo serviceInfo)
        {
            IDatabase db = this.redis.Get();

            var senders = GetEventSenders(code);

            if (senders.All(x => x.ServiceIdentity != serviceInfo.ServiceIdentity))
            {
                senders.Add(serviceInfo);
                db.StringSet(GetEventSendersKey(code), JsonConvert.SerializeObject(senders));
            }
        }


        #endregion

        #region [Db keys]

        private string GetEventDetailsKey(string eventCode)
        {
            return $"eventDetails_{eventCode}";
        }

        private string GetEventsKey()
        {
            return "events";
        }

        private string GetServicesKey()
        {
            return "services";
        }

        private string GetEventSendersKey(string param)
        {
            return $"eventSenders_{param}";
        }

        private string GetHandlersKey(string servideIdentity)
        {
            return $"serviceHandlers_{servideIdentity}";
        }

        private string GetEventListenersKey(string code)
        {
            return $"eventListeners_{code}";
        }

        private string GetRootModelKey(string name)
        {
            return $"models_{name}";
        }

        private string GetModelElementKey(string name)
        {
            return $"elements_{name}";
        }

        private string GetRootModelPluralKey(string name)
        {
            return $"modelsPlural_{name}";
        }

        #endregion
    }
}
