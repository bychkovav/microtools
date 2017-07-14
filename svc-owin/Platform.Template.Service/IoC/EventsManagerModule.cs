namespace Platform.Template.Service.IoC
{
    using System;
    using System.Configuration;
    using System.Linq;
    using Platform.Utils.Ioc;
    using Platform.Utils.Redis;
    using SimpleInjector;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Interfaces;
    using Utils.Events.QueryParser;
    using Utils.Events.ScriptEngine;
    using Platform.Utils.Events.Dispatcher;
    using Platform.Utils.Events.Dispatcher.Domain;
    using Platform.Utils.Events.Domain.Interfaces;
    using Utils.Owin.Configuration;

    public class EventsManagerModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IEventReciever, EventReciever>();
            container.RegisterSingleton<IEventSender, EventSender>();

            var eventDispatcherRegistration = Lifestyle.Singleton.CreateRegistration(() =>
            {
                var rdb = new RedisDatabase(
                    new RedisConnection(ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString),
                    int.Parse(ConfigurationManager.AppSettings["redisApiResultStorageDb"]));

                var redisStorage = new RedisStorage<AsyncResultContainer<dynamic>>(rdb, null, "result:{0:D}");
                return new EventDispatcher(
                    container.GetInstance<IEventSender>(),
                    redisStorage);
            }, container);
            container.AddRegistration(typeof(EventDispatcher), eventDispatcherRegistration);

            var storageProviderRegistration = Lifestyle.Singleton.CreateRegistration(() => new StorageProvider(new RedisDatabase(
                new RedisConnection(ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString),
                int.Parse(ConfigurationManager.AppSettings["redisEventStorageDb"]))), container);

            container.AddRegistration(typeof(StorageProvider), storageProviderRegistration);
            container.AddRegistration(typeof(IModelElementStorage), storageProviderRegistration);

            container.RegisterSingleton<ManagerDependencyResolver>(new ManagerDependencyResolver(x => ((IServiceProvider)container).GetService(x)));
            container.RegisterSingleton<ScriptEngine>();
            container.RegisterSingleton<Engine>();
            container.RegisterSingleton<ApplicationProxyBase, EventApplicationProxy>();
        }
    }
}
