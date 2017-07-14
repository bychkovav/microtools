namespace Platform.Utils.ServiceBus.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPlatformBus
    {
        Task Send<T>(string name, T obj, IDictionary<string, object> headers = null, string exchangeName = null)
            where T : class;

        Task<TResponse> RequestAsync<TResponse, TRequest>(string name, TRequest obj, IDictionary<string, object> headers = null, string exchangeName = null)
            where TResponse : class
            where TRequest : class;

        Task SendSpecific<T>(string name, string specificRoute, T obj, IDictionary<string, object> headers = null,
            string exchangeName = null) where T : class;

        Task SendAndCreate<T>(string name, string key, string exchangeName, T obj,
            IDictionary<string, object> headers = null) where T : class;

        void Dispose();
    }
}
