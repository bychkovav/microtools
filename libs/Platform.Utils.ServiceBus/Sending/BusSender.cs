namespace Platform.Utils.ServiceBus.Sending
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Newtonsoft.Json;
    using NLog;

    public class BusSender : IBusSender
    {
        private readonly IEndpointInfo conf;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string Name
        {
            get { return this.conf.Name; }
        }

        public string ExchangeName
        {
            get
            {
                return this.conf.Exchange != null ? this.conf.Exchange.Name : string.Empty;
            }
        }

        public string RouteKey
        {
            get
            {
                return this.conf.RoutingKey;
            }
        }

        public string ExchangeType { get; set; }

        public BusSender(IEndpointInfo conf)
        {
            this.conf = conf;
        }

        public Task Send<T>(T obj, string specificRoute, IDictionary<string, object> headers = null) where T : class
        {
            var message = new Message<T>(obj);
            message.SetProperties(this.conf.Properties);
            if (headers != null)
            {
                message.Properties.Headers = new Dictionary<string, object>(headers);
            }

            try
            {
                var jsonContent = JsonConvert.SerializeObject(obj);
                Log.Debug("Send message {0} to {1}", obj?.GetType() ?? typeof(T), jsonContent);
                return this.conf.Bus.PublishAsync(this.conf.Exchange,
                    string.IsNullOrEmpty(specificRoute) ? this.conf.RoutingKey : specificRoute, true, false, message);
            }
            catch (Exception ex)
            {
                Log.Error("error while sending :[{0}]", ex.Message);
                throw;
            }
        }

        public TResponse Request<TResponse, TRequest>(TRequest obj, IDictionary<string, object> headers = null)
            where TResponse : class
            where TRequest : class
        {
            var c = RequestInternal<TResponse, TRequest>(obj, headers).Result;
            return c;
        }

        private async Task<TResponse> RequestInternal<TResponse, TRequest>(TRequest obj, IDictionary<string, object> headers = null)
            where TResponse : class
            where TRequest : class
        {
            return await RequestAsync<TResponse, TRequest>(obj, headers);
        }

        public Task<TResponse> RequestAsync<TResponse, TRequest>(TRequest obj, IDictionary<string, object> headers = null)
            where TResponse : class
            where TRequest : class
        {
            var jsonContent = JsonConvert.SerializeObject(obj);

            var message = new Message<TRequest>(obj);
            var correlationId = Guid.NewGuid().ToString();

            message.Properties.ContentType = "application/json";
            message.Properties.DeliveryMode = 0x02;
            message.Properties.CorrelationId = correlationId;
            message.Properties.ReplyTo = correlationId;
            if (headers != null)
            {
                message.Properties.Headers = new Dictionary<string, object>(headers);
            }

            if (this.conf.Expires == 0)
            {
                throw new ArgumentException("You should set expires param for request.");
            }
            //TODO: Think here about recreate a queue every time.
            var callbackQueue = this.conf.Bus.QueueDeclare(Guid.NewGuid().ToString(), false, true, true, true, this.conf.Expires, this.conf.Expires);
            this.conf.Bus.Bind(this.conf.Exchange, callbackQueue, message.Properties.ReplyTo);

            var tcs = new TaskCompletionSource<TResponse>();

            var ct = new CancellationTokenSource(this.conf.Expires);
            ct.Token.Register(() => tcs.TrySetException(new TimeoutException("timeout")));

            Action<IMessage<TResponse>, TaskCompletionSource<TResponse>, MessageReceivedInfo> action =
                (mes, source,
                 info) =>
                {
                    if (correlationId == mes.Properties.CorrelationId)
                    {
                        source.TrySetResult(mes.Body);
                    }
                };
            var cons = this.conf.Bus.Consume<TResponse>(callbackQueue, (mess, info) => action.Invoke(mess, tcs, info), (conf) => this.conf.SetQoS(conf));

            tcs.Task.ContinueWith((t) => ((ConsumerCancellation)cons).Dispose(), ct.Token);

            tcs.Task.ContinueWith(
                t =>
                {
                    if (t.Exception != null)
                    {
                        Log.Error(string.Format("Error occured while waiting fo response {0} for {1}", t.Exception is AggregateException ? t.Exception.InnerException : t.Exception, jsonContent));
                    }
                    else
                    {
                        Log.Warn("Smth strange, task is not completed");
                    }
                },
                TaskContinuationOptions.OnlyOnFaulted);

            this.conf.Bus.PublishAsync(this.conf.Exchange, this.conf.RoutingKey, true, false, message);
            return tcs.Task;
        }
    }
}