namespace Platform.Utils.Events.Manager
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Helpers;
    using Interfaces;
    using Newtonsoft.Json.Linq;
    using NLog;
    using ScriptEngine;
    using Utils.Domain.Objects;
    using Utils.Domain.Objects.Exceptions;

    public class EventReciever : IEventReciever
    {
        #region [Fields]

        protected readonly StorageProvider StorageProvider;

        protected readonly string ServiceIdentity;

        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly ScriptEngine ScriptEngine;

        protected readonly ConcurrentQueue<MsgContext> EventsQueue;

        protected readonly ManagerDependencyResolver Resolver;

        protected readonly IList<IEventInterceptor> EventInterceptors;

        #endregion

        #region [Ctr]

        public EventReciever(StorageProvider storageProvider, ScriptEngine scriptEngine, ManagerDependencyResolver resolver)
        {
            this.StorageProvider = storageProvider;
            this.ScriptEngine = scriptEngine;
            this.ServiceIdentity = ConfigurationManager.AppSettings["identity"];
            this.EventsQueue = new ConcurrentQueue<MsgContext>();
            this.Resolver = resolver;

            var inerceptorTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IEventInterceptor).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
            this.EventInterceptors = inerceptorTypes.Select(obsType => (IEventInterceptor)this.Resolver.Resolve(obsType)).Where(y => y != null).ToList();

            this.Logger.Info("interceptors found: {0}", string.Join(";", this.EventInterceptors.Select(y => y.GetType().FullName)));
        }

        #endregion

        protected virtual void ProcessEvent(Envelope envelope)
        {

            this.Logger.Trace("Recieving event...");

            /* if (ectx != null)
             {
                 var mes = ectx.Event;
                 DebugEventMessage debugEv = new DebugEventMessage() { DebugId = mes.DebugId, ServiceIdentity = this.ServiceIdentity, Success = true };

                 this.Logger.Trace("Event come {0}", mes.EventCode);

                 var res = new ExecutionResult();
                 try
                 {
                     ectx.Data = JToken.Parse(mes.Content);

                     var interceptorExist = false;
                     foreach (var interceptor in this.EventInterceptors.Where(x => x.SupportEvent(ectx.Event.EventCode)).OrderBy(x => x.Order))
                     {
                         interceptorExist = true;
                         res = interceptor.PreProcess(ectx);
                         if (!res.Success)
                         {
                             this.Logger.Error(string.Join(";", res.Errors));
                             break;
                         }
                     }

                     if (!res.Success)
                     {
                         return;
                     }

                     var serviceHandlers = this.StorageProvider.GetServiceHandlers(this.ServiceIdentity);

                     var currentEventHandlers = serviceHandlers.Where(x => x.In.EventCode == mes.EventCode).ToList();
                     if (!currentEventHandlers.Any() && !interceptorExist)
                     {
                         throw new NotSupportedException($"Such event is not supported by this service {mes.EventCode}");
                     }

                     ApplicationProxyBase proxy =
                         this.Resolver.Resolve(typeof(ApplicationProxyBase)) as ApplicationProxyBase;

                     foreach (var handler in currentEventHandlers.OrderByDescending(x => (int)x.HandlerStackPlace))
                     {
                         if (handler.CodeBase == CodeBase.Script)
                         {
                             if (string.IsNullOrEmpty(handler.Script))
                             {
                                 throw new Exception("Script handler doesn't have script");
                             }

                             var result =
                                 this.ScriptEngine.Execute(new ScriptDefinition()
                                 {
                                     InputData = ectx,
                                     Script = handler.Script,
                                     Proxy = proxy,
                                     ScriptUniqueId = handler.Name
                                 });
                             if (!result.Success)
                             {
                                 var err = $"Script error : {string.Join(";", result.Errors)}";
                                 this.Logger.Error(err);

                                 debugEv.Success = false;
                                 debugEv.ErrorDetails = err;
                                 debugEv.FailedHandler = handler.Name;

                                 if (ectx.ResultTsk != null &&
                                     !(ectx.ResultTsk.Task.IsCompleted || ectx.ResultTsk.Task.IsCanceled))
                                 {
                                     ectx.ResultTsk.SetResult(new ExecutionResult<dynamic>()
                                     {
                                         Success = false,
                                         Errors = { new ErrorInfo(err) }
                                     });
                                 }

                                 break;
                             }
                         }
                         else
                         {
                             if (string.IsNullOrEmpty(handler.HandlerClassName))
                             {
                                 throw new Exception("Code handler doesn't have class name value");
                             }

                             var t = Type.GetType(handler.HandlerClassName);

                             if (t == null)
                             {
                                 this.Logger.Trace("type is null in current context :{0}", handler.HandlerClassName);
                                 var deep = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                                     .FirstOrDefault(x => TypeHelper.GetTypeFullName(x) == handler.HandlerClassName);
                                 if (deep != null)
                                 {
                                     t = deep;
                                 }
                                 else
                                 {
                                     throw new Exception(
                                         $"There is no such type in app domain: {handler.HandlerClassName}");
                                 }

                             }

                             IEventHandler codeHandler =
                                 (IEventHandler)this.Resolver.Resolve(t);
                             if (codeHandler == null)
                             {
                                 throw new EntityNotFoundException($"There is no such handler class :{handler.HandlerClassName}");
                             }

                             try
                             {
                                 codeHandler.Handle(ectx);
                             }
                             catch (Exception ex)
                             {
                                 debugEv.Success = false;
                                 debugEv.ErrorDetails = ex.ToString();
                                 debugEv.FailedHandler = handler.Name;


                                 this.Logger.Error(ex);
                                 break;
                             }

                         }
                     }
                 }
                 catch (Exception ex)
                 {
                     debugEv.Success = false;
                     debugEv.ErrorDetails = ex.ToString();
                     throw;
                 }
                 finally
                 {
                     if (!string.IsNullOrEmpty(debugEv.DebugId))
                     {
                         // Send information to debug service;
                     }
                 }
             }*/
        }

        public void EventPushed(Envelope env)
        {
            //  this.EventsQueue.Enqueue(ectx);
            //if (this.EventsQueue.Count == 1)
            // {
            ProcessEvent(env);
            //}
        }

    }
}
