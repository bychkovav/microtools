namespace Platform.Utils.Events.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.Objects;
    using Helpers;
    using Interfaces;
    using Newtonsoft.Json;
    using NLog;
    using ServiceBus.Core;
    using Utils.Domain.Objects;

    public class EventSender : IEventSender
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region [Fields]

        private readonly IPlatformBus bus;

        private readonly StorageProvider storageProvider;

        private readonly EventNotifier eventNotifier;

        private readonly string serviceIdentity;

        #endregion

        public EventSender(IPlatformBus bus, StorageProvider storageProvider, EventNotifier eventNotifier)
        {
            this.bus = bus;
            this.storageProvider = storageProvider;
            this.eventNotifier = eventNotifier;
            this.serviceIdentity = ConfigurationManager.AppSettings["identity"];
        }

        public void SendEvent(Envelope env)
        {
            /*         if (string.IsNullOrEmpty(ectx.Msg.EventPath) || string.IsNullOrEmpty(ectx.Event.EventAction))
                     {
                         throw new Exception("Event path and event action should be specified");
                     }

                     this.Logger.Trace("Sending event {0}", ectx.Event.EventCode);

                     var listeners = this.storageProvider.GetEventListeners(ectx.Event.EventCode);

                     this.Logger.Trace("Listeners: {0}", string.Join(",", listeners.Select(x => x.ServiceName)));

                     var self =
                         listeners.FirstOrDefault(
                             x =>
                                 string.Equals(x.ServiceIdentity, this.serviceIdentity, StringComparison.CurrentCultureIgnoreCase));

                     if (self != null)
                     {
                         listeners.Remove(self);

                         this.Logger.Trace("Self sending...");
                         this.eventNotifier.PushEvent(ectx);
                     }

                     foreach (var listener in listeners)
                     {
                         this.bus.SendAndCreate(ManagerInitializer.EventRouteName, ManagerInitializer.EventRouteKey, listener.ServiceIdentity, ectx.Event);
                     }*/
        }

        // public void SendEvent(Envelope env)
        //{
        /*    if (string.IsNullOrEmpty(obj.EventPath) || string.IsNullOrEmpty(obj.EventAction))
            {
                throw new Exception("Event path and event action should be specified");
            }

            if (obj.Content == null)
            {
                throw new Exception("Content is null");
            }

            if (string.IsNullOrEmpty(obj.EventCode))
            {
                throw new ArgumentException("eventCode is empty");
            }

            var contentType = obj.Content.GetType();

            Envelope message = new Envelope()
            {
                EventUniqueId = Guid.NewGuid(),
                Content = JsonConvert.SerializeObject(obj.Content),
                ContentType = TypeHelper.GetTypeFullName(contentType),
                EventAction = obj.EventAction,
                EventPath = obj.EventPath,
                EventPhase = obj.EventPhase,
                EventType = obj.EventType,
                DebugId = obj.DebugId
            };

            this.SendEvent(new MsgContext()
            {
                Event = message,
                ExecutionContext = contextContainer,
                ResultTsk = resultTsk
            });*/
        // }

        protected virtual void PreProccess(Envelope message)
        {
            return;
        }
    }
}
