namespace Platform.Template.Service.IoC
{
    using Utils.Ioc;
    using SimpleInjector;
    using Utils.Owin.Authorization;

    public class CoreModule : IPlatformIocModule
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<PasswordHasher>();
        }
    }
}
