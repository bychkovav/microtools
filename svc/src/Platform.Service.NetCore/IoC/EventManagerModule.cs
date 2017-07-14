using System;
using Microsoft.Extensions.Configuration;
using Platform.Utils.Cassandra;
using Platform.Utils.Events.Manager;
using Platform.Utils.Events.Manager.Interfaces;
using Platform.Utils.Ioc;
using Platform.Utils.Redis;
using SimpleInjector;

namespace Platform.Service.NetCore.IoC
{
    using Utils.Events.Manager.EventDispatcher;

    public class EventsManagerModule : IPlatformIocModule
    {
        public void Register(Container container, IConfigurationRoot config)
        {
            container.RegisterSingleton<IEventSender, EventSender>();

            var eventDispatcherRegistration = Lifestyle.Singleton.CreateRegistration(() =>
            {
                var rdb = new RedisDatabase(
                    new RedisConnection(config.GetSection("ConnectionStrings")["RedisServer"]),
                    int.Parse(config.GetSection("AppSettings")["redisApiResultStorageDb"]));

                var redisStorage = new RedisStorage<AsyncResultContainer<dynamic>>(rdb, null, "result:{0:D}");
                return new EventDispatcher(
                    container.GetInstance<IEventSender>(),
                    redisStorage,
                    container.GetInstance<RedisPubSub>()
                    );
            }, container);

            container.AddRegistration(typeof(EventDispatcher), eventDispatcherRegistration);
            container.RegisterSingleton<CassandraDataProviderPool>();
        }
    }
}
