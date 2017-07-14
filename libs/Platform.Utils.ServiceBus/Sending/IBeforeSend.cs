namespace Platform.Utils.ServiceBus.Sending
{
    using System.Collections.Generic;

    public interface IBeforeSend
    {
        void BeforeSend<T>(string name, string specificRoute, T obj, IDictionary<string, object> headers, string exchangeName, IBusSender sender) where T : class;
    }
}
