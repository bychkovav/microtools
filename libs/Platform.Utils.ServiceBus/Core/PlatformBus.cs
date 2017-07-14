namespace Platform.Utils.ServiceBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EasyNetQ;
    using Objects;
    using Retry;
    using Sending;

    /// <summary>
    /// Istance implemented IPlatfomBus.
    /// </summary>
    public class PlatformBus : IPlatformBus
    {
        private readonly object lockObj = new object();

        private readonly HashSet<IBusSender> senders;

        private readonly IAdvancedBus coreBus;

        private readonly IDependencyResolver resolver;

        private readonly IList<ExchangeItem> exchanges;

        internal IList<ExchangeItem> Exchanges
        {
            get
            {
                return this.exchanges;
            }
        }

        internal HashSet<IBusSender> Senders
        {
            get
            {
                return this.senders;
            }
        }

        internal IDependencyResolver Resolver
        {
            get
            {
                return this.resolver;
            }
        }

        internal IAdvancedBus CoreBus
        {
            get
            {
                return this.coreBus;
            }
        }

        private Dictionary<Type, IBeforeSend> BeforeSendActions { get; set; }

        private IList<Type> BeforeSendActionsKeys { get; set; }

        private ISendResultProcessor sendResultProcessor;

        private ISendResultProcessor SendResultProcessor
            =>
                this.sendResultProcessor ??
                (this.sendResultProcessor = (ISendResultProcessor)this.resolver.Resolve(typeof(ISendResultProcessor)))
            ;

        public bool IsRetryEnabled { get; }

        public PlatformBus(HashSet<IBusSender> senders, IAdvancedBus coreBus, IDependencyResolver resolver, bool isRetryEnabled)
        {
            this.senders = senders;
            this.coreBus = coreBus;
            this.resolver = resolver;
            IsRetryEnabled = isRetryEnabled;
            BeforeSendActions = new Dictionary<Type, IBeforeSend>();
            BeforeSendActionsKeys = new List<Type>();
            this.exchanges = new List<ExchangeItem>();
        }

        internal void AddBeforeSendAction<TAction>() where TAction : IBeforeSend
        {
            var type = typeof(TAction);
            BeforeSendActions.Add(type, null);
            BeforeSendActionsKeys.Add(type);
        }

        private IBusSender GetSenderFor(string name, string exchangeName)
        {
            IList<IBusSender> possibleSenders = name.StartsWith(":") ? this.senders.Where(x => x.RouteKey == name.TrimStart(':')).ToList()
                : this.senders.Where(x => x.Name == name).ToList();

            if (possibleSenders == null || possibleSenders.Count == 0)
            {
                throw new KeyNotFoundException(string.Format("There is no such sender with the name :{0}", name));
            }

            if (string.IsNullOrEmpty(exchangeName))
            {
                return possibleSenders.First();
            }

            var sender = possibleSenders.FirstOrDefault(x => x.ExchangeName == exchangeName);

            if (sender == null)
            {
                throw new KeyNotFoundException(string.Format("No senders for exchange name :{0}", exchangeName));
            }

            return sender;
        }

        public Task SendSpecific<T>(string name, string specificRoute, T obj, IDictionary<string, object> headers = null,
            string exchangeName = null) where T : class
        {
            var sender = this.GetSenderFor(name, exchangeName);
            if (headers == null)
            {
                headers = new Dictionary<string, object>();
            }

            OnBeforeSend<T>(name, specificRoute, obj, headers, exchangeName, sender);

            try
            {
                return sender.Send(obj, specificRoute, headers).ContinueWith(x =>
                {
                    if (IsRetryEnabled)
                    {
                        if (x.IsCompleted && !x.IsFaulted)
                        {
                            SendResultProcessor.ProcessSentMessage(name, specificRoute, obj, headers, exchangeName);
                        }
                        else
                        {
                            SendResultProcessor.ProcessFailedMessage(name, specificRoute, obj, headers, exchangeName);
                        }
                    }
                });
            }
            catch
            {
                if (IsRetryEnabled)
                {
                    SendResultProcessor.ProcessFailedMessage(name, specificRoute, obj, headers, exchangeName);
                }
                throw;
            }
        }

        public Task Send<T>(string name, T obj, IDictionary<string, object> headers = null, string exchangeName = null) where T : class
        {
            return SendSpecific(name, null, obj, headers, exchangeName);
        }

        public Task SendAndCreate<T>(string name, string key, string exchangeName, T obj, IDictionary<string, object> headers = null) where T : class
        {
            if (string.IsNullOrEmpty(exchangeName) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("You should specify name, key and exchange");
            }

            if (!this.senders.Any(x => (x.Name == name || x.RouteKey == key) && x.ExchangeName == exchangeName))
            {
                PlatformBusFactory.DynamicAddSender(this, new OutMessage() { Key = key, Name = name }, exchangeName);
            }

            return SendSpecific(name, null, obj, headers, exchangeName);
        }

        public Task<TResponse> RequestAsync<TResponse, TRequest>(string name, TRequest obj, IDictionary<string, object> headers = null, string exchangeName = null)
            where TResponse : class
            where TRequest : class
        {
            var sender = this.GetSenderFor(name, exchangeName);
            if (headers == null)
            {
                headers = new Dictionary<string, object>();
            }

            OnBeforeSend<TRequest>(name, null, obj, headers, exchangeName, sender);

            return sender.RequestAsync<TResponse, TRequest>(obj, headers);
        }



        private void OnBeforeSend<TRequest>(string name, string specificRoute, TRequest obj, IDictionary<string, object> headers, string exchangeName, IBusSender sender) where TRequest : class
        {
            foreach (var beforeSend in BeforeSendActionsKeys)
            {
                if (BeforeSendActions[beforeSend] == null)
                {
                    lock (this.lockObj)
                    {
                        if (BeforeSendActions[beforeSend] == null)
                        {
                            BeforeSendActions[beforeSend] =
                                (IBeforeSend)this.resolver.Resolve(beforeSend);
                        }
                    }
                }
                BeforeSendActions[beforeSend].BeforeSend(name, specificRoute, obj, headers, exchangeName, sender);
            }
        }

        public void Dispose()
        {
            this.coreBus.Dispose();
        }
    }
}
