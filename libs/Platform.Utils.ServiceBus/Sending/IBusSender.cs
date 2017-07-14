namespace Platform.Utils.ServiceBus.Sending
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// The BusSender interface.
    /// </summary>
    public interface IBusSender
    {
        string RouteKey { get; }

        string Name { get; }

        string ExchangeName { get; }

        string ExchangeType { get; set; }

        Task Send<T>(T obj, string specificRoute, IDictionary<string, object> headers = null) where T : class;

        Task<TResponse> RequestAsync<TResponse, TRequest>(TRequest obj, IDictionary<string, object> headers = null)
            where TResponse : class
            where TRequest : class;
    }
}