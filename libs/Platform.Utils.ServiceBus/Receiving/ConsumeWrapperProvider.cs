namespace Platform.Utils.ServiceBus.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Core;
    using EasyNetQ;
    using Newtonsoft.Json;
    using NLog;

    internal class ConsumeWrapperProvider
    {
        private static readonly object lockObj = new object();

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<Type, IList<Type>> InterceptorsMap = new Dictionary<Type, IList<Type>>();
        private static readonly Dictionary<Type, IConsumerInterceptor> InterceptorsInstances = new Dictionary<Type, IConsumerInterceptor>();
        private static readonly Dictionary<Type, Action<dynamic>> InterceptCache = new Dictionary<Type, Action<dynamic>>();

        public static void AddConsumerDecorator<TDecorator>(IList<Type> typesToDecorate) where TDecorator : IConsumerInterceptor
        {
            InterceptorsMap.Add(typeof(TDecorator), typesToDecorate);
        }

        public static Func<byte[], MessageProperties, MessageReceivedInfo, Task> GetConsumeActionWrapper(HandlerRegistration handlerRegistration, IDependencyResolver dependencyResolver)
        {
            Func<byte[], MessageProperties, MessageReceivedInfo, Task> consumeAction = (body, messageProps, info) =>
                {
                    var bodyString = Encoding.UTF8.GetString(body);
                    var messageConent = typeof(string) == handlerRegistration.MessageType
                        ? bodyString
                        : JsonConvert.DeserializeObject(bodyString, handlerRegistration.MessageType, SerializerSettings);

                    Logger.Debug("Message {0} recieved with content {1}", handlerRegistration.MessageType, bodyString);

                    var message = Message.CreateInstance(handlerRegistration.MessageType, messageConent);
                    message.SetProperties(messageProps);
                    return
                        TaskHelpers.ExecuteSynchronously(
                            () =>
                                GetHandler(message, handlerRegistration.GetConsumerContextFunc,
                                    handlerRegistration.Consumer, dependencyResolver));
                };

            return consumeAction;
        }

        private static dynamic GetHandler(dynamic message, Func<dynamic, dynamic> getConsumerContextFunc, object consumer, IDependencyResolver dependencyResolver)
        {
            var ctx = getConsumerContextFunc(message);
            var consumerType = consumer.GetType();

            ctx.ConsumerType = consumerType;

            if (!InterceptCache.ContainsKey(consumerType))
            {
                var interceptorsToApply = new List<IConsumerInterceptor>();
                foreach (var decoratorMapItem in InterceptorsMap)
                {
                    lock (lockObj)
                    {
                        if (!InterceptorsInstances.ContainsKey(decoratorMapItem.Key))
                        {
                            lock (lockObj)
                            {
                                if (!InterceptorsInstances.ContainsKey(decoratorMapItem.Key))
                                {
                                    InterceptorsInstances.Add(decoratorMapItem.Key,
                                        (IConsumerInterceptor)
                                            dependencyResolver.Resolve(decoratorMapItem.Key));
                                }
                            }
                        }
                    }

                    var decorator = InterceptorsInstances[decoratorMapItem.Key];
                    if (decorator.IsGlobal ||
                        (decoratorMapItem.Value != null && decoratorMapItem.Value.Any(x => x == consumerType)))
                    {
                        interceptorsToApply.Add(decorator);
                    }
                }
                MethodInfo handleMethod = consumer.GetType().GetMethod("Handle");
                Action<dynamic> target = (context) => handleMethod.Invoke(consumer, new[] { context });
                target = interceptorsToApply.Aggregate(target, (current, interceptor) => NestCall(interceptor, current));
                InterceptCache.Add(consumerType, target);
            }
            var callChain = InterceptCache[consumerType];
            callChain(ctx);
            return null;
        }

        private static Action<dynamic> NestCall(IConsumerInterceptor interceptor, Action<dynamic> inner)
        {
            return (ctx) => interceptor.Intercept(ctx, inner);
        }

    }
}
