namespace Platform.Utils.Tests.IoC
{
    using System.Configuration;
    using Cassandra;
    using global::Cassandra.Mapping;
    using SimpleInjector;
    using Utils.Ioc;

    public class DataModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
          
        }
    }
}
