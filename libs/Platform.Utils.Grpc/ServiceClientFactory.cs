namespace Platform.Utils.Grpc
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Configuration;
    using global::Grpc.Core;
    using Utils.Rpc;

    /// <summary>
    /// Provides basic functionality for managing GRPC clients.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class ServiceClientFactory<TClient> : IRemoteServiceClientFactory where TClient : ClientBase<TClient>
    {
        private readonly IDictionary<string, TClient> clients =
            new Dictionary<string, TClient>();

        private readonly object lockRoot = new object();

        /// <summary>
        /// Returns an exsiting GPRC client by it's name from config file.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetClient(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name is null or empty");
            }
            lock (this.lockRoot)
            {
                if (this.clients.ContainsKey(name))
                {
                    return this.clients[name];
                }
                if (ServiceSection.Current.Clients == null)
                {
                    throw new ConfigurationErrorsException("No clients where configured in config section. Please, use <grpcSection><clients><add>.");
                }
                var clientElement =
                    ServiceSection.Current.Clients.Cast<ServiceClientElement>().FirstOrDefault(x => x.Name == name);
                if (clientElement == null)
                {
                    throw new ConfigurationErrorsException($"No client with name {name} was configured in config section. Please, use <grpcSection><clients><add>.");
                }

                var serviceClient = CreateNewInstance(clientElement.Host, clientElement.Port);
                this.clients.Add(name, serviceClient);
                return serviceClient;
            }
        }

        /// <summary>
        /// Provides a constructor functionality for newly created client instaces.
        /// </summary>
        /// <param name="host">Remote host to work with</param>
        /// <param name="port">Remote port to work with</param>
        /// <returns></returns>
        protected abstract TClient CreateNewInstance(string host, int port);
    }
}
