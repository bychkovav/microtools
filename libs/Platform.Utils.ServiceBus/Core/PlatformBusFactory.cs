namespace Platform.Utils.ServiceBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Configuration;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using EasyNetQ.Topology;
    using Helpers;
    using NLog;
    using Objects;
    using Receiving;
    using Retry.Impl;
    using Sending;

    public static class PlatformBusFactory
    {
        private static readonly object lockObj = new object();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static bool started = false;

        internal static readonly ConcurrentDictionary<Type, Func<byte[], MessageProperties, MessageReceivedInfo, Task>> ConsumerActions =
            new ConcurrentDictionary<Type, Func<byte[], MessageProperties, MessageReceivedInfo, Task>>();

        public static string CreatedBindedQueue(PlatformBus bus, string queueKey, string exchangeName, string exchType)
        {
            var rabbitExchange = bus.CoreBus.ExchangeDeclare(exchangeName, exchType);
            var qName = string.Format("{0}.{1}", exchangeName, queueKey);
            var queue = bus.CoreBus.QueueDeclare(qName);
            bus.CoreBus.Bind(rabbitExchange, queue, queueKey);
            return qName;
        }

        public static void DynamicAddInfrustructure(PlatformBus bus, IList<ExchangeItem> exchnages)
        {
            foreach (var exchangeItem in exchnages)
            {
                var existExchange = bus.Exchanges.FirstOrDefault(x => x.Name == exchangeItem.Name);
                if (existExchange == null)
                {
                    var rabbitExchange = bus.CoreBus.ExchangeDeclare(exchangeItem.Name, exchangeItem.Type);
                    exchangeItem.RabbitExchange = rabbitExchange;
                    bus.Exchanges.Add(exchangeItem);
                }
                else
                {
                    exchangeItem.RabbitExchange = existExchange.RabbitExchange;
                }

                CreateConsumers(bus.Resolver, bus.CoreBus, exchangeItem);
                foreach (var outMes in exchangeItem.Out)
                {
                    DynamicAddSender(bus, outMes, exchangeItem.Name, exchangeItem.Type);
                }
            }
        }

        public static void DynamicAddSender(PlatformBus advancedBus, OutMessage outMes, string exchName, string exchType = null)
        {
            if (advancedBus.Senders.Any(x => x.ExchangeName == exchName && x.RouteKey == outMes.Key))
            {
                return;
            }

            if (string.IsNullOrEmpty(exchName) || string.IsNullOrEmpty(outMes.Name))
            {
                throw new ArgumentNullException("exchName or name");
            }
            if (string.IsNullOrEmpty(exchType))
            {
                exchType = "topic";
            }

            var exchange = advancedBus.CoreBus.ExchangeDeclare(exchName, exchType);
            if (!outMes.Timeout.HasValue)
            {
                outMes.Timeout = new TimeSpan(0, 0, 10);
            }

            advancedBus.Senders.Add(
                new BusSender(EndpointInfoFactory.Create(advancedBus.CoreBus, exchange, outMes.Name, outMes.Key,
                    outMes.Persist, outMes.Timeout))
                { ExchangeType = exchType });
        }

        public static PlatformBusBuilder Create(DependencyResolverFunc func)
        {
            return Create(new LambdaDependencyResolver(func));
        }

        public static PlatformBusBuilder Create(IDependencyResolver resolver)
        {
            ServiceBusSection section = ServiceBusSection.Current;
            var bus = RabbitHutch.CreateBus(section.ConnectionString,
                serviceRegister =>
                {
                    serviceRegister.Register<ITypeNameSerializer>(
                        serviceProvider => new PlatformTypeNameSerializer());
                    serviceRegister.Register<IEasyNetQLogger>(serviceProvider => new PlatformLogger());
                    serviceRegister.Register<IConventions>(
                        serviceProvider => new PlatformConventions(serviceProvider.Resolve<ITypeNameSerializer>()));
                    serviceRegister.Register<IConsumerErrorStrategy>(
                        serviceProvider =>
                            new ConsumerErrorStrategy(serviceProvider.Resolve<IConnectionFactory>(),
                                serviceProvider.Resolve<ISerializer>(), serviceProvider.Resolve<IEasyNetQLogger>(),
                                serviceProvider.Resolve<IConventions>(), serviceProvider.Resolve<ITypeNameSerializer>()));
                });
            var advancedBus = bus.Advanced;

            advancedBus.MessageReturned += (bytes, properties, arg3) =>
              Logger.Error("Message was returned to sender with reason :[{0}].Message : [{1}]", arg3, Encoding.UTF8.GetString(bytes));

            var set = new HashSet<IBusSender>();

            var platformBus = new PlatformBus(set, advancedBus, resolver, section.SendRetryEnabled);

            IList<Action> consumeActions = new List<Action>();

            //TODO: NOTE: Or get it from DB! 
            foreach (var exchangeItem in section.Exchanges.Cast<ExchangeElement>())
            {
                var rabbitExchange = advancedBus.ExchangeDeclare(exchangeItem.Name, exchangeItem.Type);
                var platformExchange = GetPlatformExchangeFromConfig(exchangeItem);
                platformExchange.RabbitExchange = rabbitExchange;
                platformBus.Exchanges.Add(platformExchange);

                consumeActions.Add(() => CreateConsumers(resolver, advancedBus, platformExchange));
                AddSenders(advancedBus, rabbitExchange, exchangeItem, set, section);
            }

            lock (lockObj)
            {
                RedButton = () =>
                {
                    if (!started)
                    {
                        lock (lockObj)
                        {
                            if (!started)
                            {
                                foreach (var consumeAction in consumeActions)
                                {
                                    consumeAction.Invoke();
                                }

                                started = true;
                            }
                        }
                    }
                };
            }

            return new PlatformBusBuilder(platformBus);
        }

        /// <summary>
        /// Gets the red button.
        /// </summary>
        public static Action RedButton { get; private set; }

        private static void CreateConsumers(IDependencyResolver resolver, IAdvancedBus advancedBus, ExchangeItem exchangeItem)
        {
            if (exchangeItem.RabbitExchange == null)
            {
                throw new ArgumentException("rabbit exchange is null`");
            }

            foreach (var income in exchangeItem.In)
            {
                var queue = advancedBus.QueueDeclare(string.Format("{0}.{1}", exchangeItem.Name, income.Key));
                advancedBus.Bind(exchangeItem.RabbitExchange, queue, income.Key);

                var messageType = ReflectionHelper.ResolveType(income.Type);

                if (messageType == null)
                {
                    throw new Exception("no such message type");
                }

                Type consType = ReflectionHelper.ResolveType(income.React);
                var consumer = resolver.Resolve(consType);
                var endpointInfo = EndpointInfoFactory.Create(advancedBus, exchangeItem.RabbitExchange, income.Name, income.Key);

                var handlerReg = GetHandlerRegistration(endpointInfo, consumer, messageType);


                IList<object> consumeInvokeParams = new List<object>();
                consumeInvokeParams.Add(queue);

                var action = ConsumeWrapperProvider.GetConsumeActionWrapper(handlerReg, resolver);
                consumeInvokeParams.Add(action);

                if (!ConsumerActions.TryAdd(consumer.GetType(), action))
                {
                    Logger.Warn("can't add consumer handler for {0}", consumer.GetType().ToString());
                }

                Action<IConsumerConfiguration> consConf = configuration => configuration.WithPrefetchCount(exchangeItem.PrefetchCount);
                consumeInvokeParams.Add(consConf);

                //NOTE: Such ugly shit to get generic overloaded method
                var sbConsume = typeof(IAdvancedBus).GetMethods().Where(x => x.Name == "Consume").Select(m => new
                {
                    Method = m,
                    Params = m.GetParameters(),
                    Args = m.GetGenericArguments()
                }).Where(x => x.Params.Length == 3 && x.Args.Length == 0
                                    && x.Params[1].ParameterType.Name == action.GetType().Name)
                       .Select(x => x.Method)
                       .First();
                sbConsume.Invoke(advancedBus, consumeInvokeParams.ToArray());
            }
        }

        private static HandlerRegistration GetHandlerRegistration(IEndpointInfo endpointInfo, object consumer, Type messageType)
        {
            var ctxConstr = typeof(ConsumerContext<>).MakeGenericType(messageType).GetConstructor(new[] { typeof(IEndpointInfo), typeof(IMessage<>).MakeGenericType(messageType) });

            if (ctxConstr == null)
            {
                throw new Exception("no constructor");
            }

            return new HandlerRegistration()
            {
                Consumer = consumer,
                EndpointInfo = endpointInfo,
                GetConsumerContextFunc = (mes) => ctxConstr.Invoke(new[] { endpointInfo, mes }),
                MessageType = messageType
            };
        }

        private static void AddSenders(IAdvancedBus advancedBus, IExchange exchange,
                                                      ExchangeElement exchangeItem, HashSet<IBusSender> set, ServiceBusSection configSection)
        {
            foreach (var outcome in exchangeItem.Out.Cast<OutgoingElement>())
            {
                set.Add(
                    new BusSender(EndpointInfoFactory.Create(advancedBus, exchange, outcome.Name, outcome.Key,
                        outcome.Persist, outcome.Timeout))
                    { ExchangeType = exchangeItem.Type });
            }
        }

        private static ExchangeItem GetPlatformExchangeFromConfig(ExchangeElement configElement)
        {
            var item = new ExchangeItem() { Name = configElement.Name, PrefetchCount = configElement.PrefetchCount, Type = configElement.Type, Out = new List<OutMessage>(), In = new List<InMessage>() };
            foreach (var outEl in configElement.Out.Cast<OutgoingElement>())
            {
                var outItem = new OutMessage() { Key = outEl.Key, Name = outEl.Name, Persist = outEl.Persist, Timeout = outEl.Timeout };
                item.Out.Add(outItem);
            }
            foreach (var inElement in configElement.In.Cast<IncomingElement>())
            {
                var inItem = new InMessage() { Key = inElement.Key, Name = inElement.Name, Type = inElement.Type, React = inElement.React };
                item.In.Add(inItem);
            }

            return item;
        }

    }
}
