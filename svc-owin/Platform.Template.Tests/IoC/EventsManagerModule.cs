namespace Platform.Template.Tests.IoC
{
    using System;
    using System.Configuration;
    using SimpleInjector;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Interfaces;
    using Utils.Events.QueryParser;
    using Utils.Events.ScriptEngine;
    using Utils.Ioc;
    using Utils.Redis;

    public class EventsManagerModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IEventReciever, EventReciever>();
            container.RegisterSingleton<IEventSender, EventSender>();
            container.RegisterSingleton<StorageProvider>(new StorageProvider(new RedisDatabase(
                       new RedisConnection(ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString),
                       int.Parse(ConfigurationManager.AppSettings["redisEventStorageDb"]))));
            container.RegisterSingleton<ManagerDependencyResolver>(new ManagerDependencyResolver(x => ((IServiceProvider)container).GetService(x)));
            container.RegisterSingleton<ScriptEngine>();
            container.RegisterSingleton<Engine>();
            //container.RegisterSingleton<ManagerInitializer>();
            container.RegisterSingleton<ApplicationProxyBase, EventApplicationProxy>();
        }
    }
}
