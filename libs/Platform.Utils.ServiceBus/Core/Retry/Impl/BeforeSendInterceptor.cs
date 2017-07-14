namespace Platform.Utils.ServiceBus.Core.Retry.Impl
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using NLog;
    using Sending;

    public class BeforeSendInterceptor : IBeforeSend
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly ISendResultProcessor sendResultProcessor;

        private readonly bool isEnabled;

        public BeforeSendInterceptor(ISendResultProcessor sendResultProcessor)
        {
            this.sendResultProcessor = sendResultProcessor;
            this.isEnabled = ServiceBusSection.Current.SendRetryEnabled;
        }

        public void BeforeSend<T>(string name, string specificRoute, T obj, IDictionary<string, object> headers, string exchangeName, IBusSender sender) where T : class
        {
            if (this.isEnabled)
            {
                try
                {
                    this.sendResultProcessor.ProcessNotSentMessage(name, specificRoute, obj, headers, exchangeName);
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Unable to save message before send.");
                }
            }
        }
    }
}
