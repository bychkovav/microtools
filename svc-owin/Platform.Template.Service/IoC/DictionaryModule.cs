namespace Platform.Template.Service.IoC
{
    using System.Configuration;
    using Cassandra.Mapping;
    using SimpleInjector;
    using Utils.Dictionaries.Core;
    using Utils.Dictionaries.Data;
    using Utils.Dictionaries.Data.CasMappings;
    using Utils.Dictionaries.Data.Repositories;
    using Utils.Ioc;

    public class DictionaryModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IDictionaryService, DictionaryService>();
            container.RegisterSingleton<DictionaryRepository>();
        }
    }
}