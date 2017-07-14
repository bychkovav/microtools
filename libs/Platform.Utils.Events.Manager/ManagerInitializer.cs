namespace Platform.Utils.Events.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Domain.Objects;
    using Helpers;
    using Interfaces;
    using NLog;
    using ScriptEngine;
    using ServiceBus.Core;
    using ServiceBus.Objects;

    public class ManagerInitializer
    {
        #region [Constants]

        public const string EventRouteName = "EventLocalSend";

        public const string EventRouteKey = "4EF69428-FD92-4E0B-B28D-5102C80AD90D";

        public const string CloudRouterInside = "CloudRouterInside";

        #endregion

        #region [Fields]

        private readonly StorageProvider storageProvider;

        private readonly string serviceIdentity;

        private readonly IPlatformBus bus;

        protected readonly ManagerDependencyResolver Resolver;

        #endregion

        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ManagerInitializer(StorageProvider storageProvider, IPlatformBus bus, ManagerDependencyResolver resolver)
        {
            this.storageProvider = storageProvider;
            this.bus = bus;
            this.serviceIdentity = ConfigurationManager.AppSettings["identity"];
            this.Resolver = resolver;
        }

        public void StartManager()
        {
            var obsTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IEventObserver).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
            IList<IEventObserver> observers = obsTypes.Select(obsType => (IEventObserver)this.Resolver.Resolve(obsType)).ToList();

            EventNotifier.Init(observers);

          //  AddEventQueue();
        }

        private void AddEventQueue()
        {
            var platformBus = this.bus as PlatformBus;
            if (platformBus == null)
            {
                return;
            }

            //TODO:NOTE: Remove Rabbit at all.
            /*   PlatformBusFactory.DynamicAddInfrustructure(platformBus, new List<ExchangeItem>() {new ExchangeItem()
               {
                   In = new List<InMessage>() {new InMessage()
                   {
                       Name = EventRouteName,
                       Key = EventRouteKey,
                       React = TypeHelper.GetTypeFullName(typeof(EventConsumer)),
                       Type = TypeHelper.GetTypeFullName(typeof(Envelope))
                   } }, Out = new List<OutMessage>(),Name = this.serviceIdentity, Type = "topic"
               } });*/
        }


    }
}
