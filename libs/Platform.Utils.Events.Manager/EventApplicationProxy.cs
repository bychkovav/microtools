namespace Platform.Utils.Events.Manager
{
    using System;
    using System.Dynamic;
    using Domain.Enums;
    using Domain.Objects;
    using Helpers;
    using Newtonsoft.Json;
    using NLog;
    using ScriptEngine;
    using Utils.Domain.Objects;

    public class EventApplicationProxy : ApplicationProxyBase
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public EventApplicationProxy(ManagerDependencyResolver resolver) : base(resolver)
        {
        }

        public override ExpandoObject GetServicesContainer()
        {
            dynamic sc = base.GetServicesContainer();


            sc.FireEvent = new Action<string, string, string, object>((elem, action, phase, o) => PublishEvent(elem, action, phase, o, MsgType.Event, sc.Context));

            sc.InvokeCommand = new Action<string, string, string, object>((elem, action, phase, o) => PublishEvent(elem, action, phase, o, MsgType.Command, sc.Context));

            //   sc.PostEvent = new Action<string, object>((e, o) => PublishEvent(e, o, EventType.EventPost, sc.Context));

            return sc;
        }

        private void PublishEvent(string elem, string action, string phase, object objToSend, MsgType msgType, MsgContext ctx)
        {
            if (objToSend == null)
            {
                throw new Exception("Content is null");
            }

            if (string.IsNullOrEmpty(elem) || string.IsNullOrEmpty(action))
            {
                throw new ArgumentException("eventCode is empty");
            }

            var s = (EventSender)Resolver.Resolve(typeof(EventSender));
            if (s == null)
            {
                throw new Exception("No sender initialized");
            }

            var ctxCopy = (MsgContext)ctx.Clone();

            /* ctxCopy.Event.EventUniqueId = Guid.NewGuid();
             ctxCopy.Event.EventAction = action;
             ctxCopy.Event.EventPhase = phase;
             ctxCopy.Event.EventPath = elem;

             ctxCopy.Event.Content = JsonConvert.SerializeObject(objToSend);
             ctxCopy.Event.ContentType = TypeHelper.GetTypeFullName(objToSend.GetType());

             ctxCopy.Event.EventType = msgType;

             s.SendEvent(ctxCopy);*/
        }
    }
}
