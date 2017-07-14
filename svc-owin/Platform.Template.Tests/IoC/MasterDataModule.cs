namespace Platform.Template.Tests.IoC
{
    using System.Configuration;
    using Core.ProxyExtensions;
    using Data;
    using Data.Repositories;
    using SimpleInjector;
    using Utils.Ioc;

    public class MasterDataModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<MasterDataExtension>();
        }
    }
}
