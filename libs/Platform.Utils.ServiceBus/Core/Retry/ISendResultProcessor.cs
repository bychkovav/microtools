namespace Platform.Utils.ServiceBus.Core.Retry
{
    using System.Collections.Generic;

    public interface ISendResultProcessor
    {
        void ProcessNotSentMessage(string name, string specificRoute, object obj, IDictionary<string, object> headers,
                   string exchangeName);

        void ProcessFailedMessage(string name, string specificRoute, object obj, IDictionary<string, object> headers,
                   string exchangeName);

        void ProcessSentMessage(string name, string specificRoute, object obj,
            IDictionary<string, object> headers, string exchangeName);
    }
}
