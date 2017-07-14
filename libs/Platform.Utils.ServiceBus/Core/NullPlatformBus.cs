namespace Platform.Utils.ServiceBus.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Null-object pattern implementation for IPlatformBus
    /// </summary>
    public class NullPlatformBus : IPlatformBus
    {
        public Task Send<T>(string name, T obj, IDictionary<string, object> headers = null, string exchangeName = null) where T : class
        {
            return Task.FromResult<object>(null);
        }

        public Task<TResponse> RequestAsync<TResponse, TRequest>(string name, TRequest obj, IDictionary<string, object> headers = null, string exchangeName = null)
            where TResponse : class
            where TRequest : class
        {
            return Task.FromResult<TResponse>(null);
        }

        public Task SendSpecific<T>(string specificRoute, string name, T obj, IDictionary<string, object> headers = null, string exchangeName = null) where T : class
        {
            return Task.FromResult<object>(null);
        }

        public Task SendAndCreate<T>(string name, string key, string exchangeName, T obj, IDictionary<string, object> headers = null) where T : class
        {
            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
        }
    }
}
