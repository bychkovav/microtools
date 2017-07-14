namespace Platform.Utils.ServiceBus.Core.Retry.Impl
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using Configuration;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using EasyNetQ.SystemMessages;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Exceptions;
    using IConnectionFactory = EasyNetQ.IConnectionFactory;

    public class ConsumerErrorStrategy : DefaultConsumerErrorStrategy
    {
        private readonly object syncLock = new object();

        private readonly IEasyNetQLogger logger;

        private readonly IConnectionFactory connectionFactory;

        private readonly ITypeNameSerializer typeNameSerializer;

        private readonly ISerializer serializer;

        private readonly IConventions conventions;

        private readonly ConcurrentDictionary<string, string> errorExchanges = new ConcurrentDictionary<string, string>();

        private bool errorQueueDeclared;

        private IConnection connection;

        private readonly int maxAttemptsCount;


        public ConsumerErrorStrategy(IConnectionFactory connectionFactory, ISerializer serializer,
            IEasyNetQLogger logger, IConventions conventions, ITypeNameSerializer typeNameSerializer)
            : base(connectionFactory, serializer, logger, conventions, typeNameSerializer)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.typeNameSerializer = typeNameSerializer;
            this.serializer = serializer;
            this.conventions = conventions;
            this.maxAttemptsCount = ServiceBusSection.Current.MaxConsumeRetry;
        }

        private void Connect()
        {
            if (this.connection != null && this.connection.IsOpen)
                return;
            lock (this.syncLock)
            {
                if (this.connection != null && this.connection.IsOpen)
                    return;
                this.connection?.Dispose();
                this.connection = this.connectionFactory.CreateConnection();
            }
        }

        public override AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            try
            {
                if (context.Properties.Headers.ContainsKey(RetryConstants.RetryCountKey))
                {
                    var retryCount = Convert.ToInt32((long)context.Properties.Headers[RetryConstants.RetryCountKey]);
                    if (retryCount > this.maxAttemptsCount)
                    {
                        this.logger.ErrorWrite(
                            "EasyNetQ Consumer Error Handler: Skip republish to dead-letter queue - max attempts count exceeded.");
                        //LOG MESSAGE HERE
                        return AckStrategies.Ack;
                    }
                }

                this.Connect();
                using (var model = this.connection.CreateModel())
                {
                    var exchange = this.DeclareErrorExchangeQueueStructure(model, context);
                    var errorMessage = this.CreateErrorMessage(context, exception);
                    var basicProperties = model.CreateBasicProperties();
                    basicProperties.SetPersistent(true);
                    basicProperties.Type = this.typeNameSerializer.Serialize(typeof(Error));
                    model.BasicPublish(exchange, context.Info.RoutingKey, basicProperties, errorMessage);
                }
            }
            catch (BrokerUnreachableException ex)
            {
                this.logger.ErrorWrite("EasyNetQ Consumer Error Handler cannot connect to Broker\n" + this.CreateConnectionCheckMessage(), new object[0]);
            }
            catch (OperationInterruptedException ex)
            {
                this.logger.ErrorWrite("EasyNetQ Consumer Error Handler: Broker connection was closed while attempting to publish Error message.\n" +
                                       $"Message was: '{(object)ex.Message}'\n" + this.CreateConnectionCheckMessage(), new object[0]);
            }
            catch (Exception ex)
            {
                this.logger.ErrorWrite("EasyNetQ Consumer Error Handler: Failed to publish error message\nException is:\n" + (object)ex, new object[0]);
            }
            return AckStrategies.Ack;
        }


        private void DeclareDefaultErrorQueue(IModel model)
        {
            if (this.errorQueueDeclared)
                return;
            model.QueueDeclare(this.conventions.ErrorQueueNamingConvention(), true, false, false, (IDictionary<string, object>)null);
            this.errorQueueDeclared = true;
        }

        private string DeclareErrorExchangeAndBindToDefaultErrorQueue(IModel model, ConsumerExecutionContext context)
        {
            var originalRoutingKey = context.Info.RoutingKey;
            return this.errorExchanges.GetOrAdd(originalRoutingKey, (Func<string, string>)(_ =>
            {
                var exchange = this.conventions.ErrorExchangeNamingConvention(context.Info);
                model.ExchangeDeclare(exchange, "direct", true);
                model.QueueBind(this.conventions.ErrorQueueNamingConvention(), exchange, originalRoutingKey);
                return exchange;
            }));
        }

        private string DeclareErrorExchangeQueueStructure(IModel model, ConsumerExecutionContext context)
        {
            this.DeclareDefaultErrorQueue(model);
            return this.DeclareErrorExchangeAndBindToDefaultErrorQueue(model, context);
        }

        private byte[] CreateErrorMessage(ConsumerExecutionContext context, Exception exception)
        {
            var @string = Encoding.UTF8.GetString(context.Body);
            if (context.Properties?.Headers != null)
            {
                if (context.Properties.Headers.ContainsKey(RetryConstants.RetryCountKey))
                {
                    var retryCount = Convert.ToInt32((long)context.Properties.Headers[RetryConstants.RetryCountKey]);
                    context.Properties.Headers[RetryConstants.RetryCountKey] = ++retryCount;
                }
                else
                {
                    context.Properties.Headers.Add(RetryConstants.RetryCountKey, 0);
                }
            }
            var aggregateException = exception as AggregateException;
            var resultException = aggregateException != null
                ? aggregateException.InnerException
                : exception;
            try
            {
                var message = new Error()
                {
                    RoutingKey = context.Info.RoutingKey,
                    Exchange = context.Info.Exchange,
                    Exception = resultException.ToString(),
                    Message = @string,
                    DateTime = DateTime.Now,
                    BasicProperties = context.Properties
                };
                return this.serializer.MessageToBytes(message);
            }
            catch (JsonSerializationException ex)
            {
                this.logger.ErrorWrite(ex);
                throw;
            }
            catch (Exception ex)
            {
                this.logger.ErrorWrite(ex);
                throw;
            }
        }

        private string CreateConnectionCheckMessage()
        {
            return "Please check EasyNetQ connection information and that the RabbitMQ Service is running at the specified endpoint.\n" +
                   $"\tHostname: '{(object)this.connectionFactory.CurrentHost.Host}'\n" +
                   $"\tVirtualHost: '{(object)this.connectionFactory.Configuration.VirtualHost}'\n" +
                   $"\tUserName: '{(object)this.connectionFactory.Configuration.UserName}'\n" + "Failed to write error message to error queue";
        }
    }
}
