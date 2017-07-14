using System;
using System.IO;
using System.Reflection;

namespace Platform.Utils.Owin
{
    using System.Configuration;
    using Events.Manager;
    //using Events.Manager;
    using global::NLog;
    using Ioc;
    using Microsoft.Owin.Hosting;
    using ServiceBus.Core;

    public class OwinService
    {
        private IDisposable owinWeb;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string DynamicLibPath = "extensions";

        public virtual void Start()
        {
            if (Directory.Exists(DynamicLibPath))
            {
                foreach (var dll in Directory.GetFiles(DynamicLibPath, "*.dll"))
                {
                    var ass = Assembly.LoadFrom(dll);
                    this.logger.Info($"LOADING EXTENSION {ass}");
                    if (ass.Location != null)
                    {
                        AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(ass.Location));
                    }
                    else
                    {
                        this.logger.Error($"ERROR LOADING EXTENSION {ass}: Assembly.Location property is null");
                    }
                }
            }
            else
            {
                this.logger.Warn("NO EXTENSIONS FOUND");
            }

            this.logger.Info("IoC initializing....");
            IocContainerProvider.InitIoc();
            IocContainerProvider.CurrentContainer.Verify();
            this.logger.Info("IoC initialized.");

            IocContainerProvider.CurrentContainer.GetInstance<ManagerInitializer>().StartManager();
            PlatformBusFactory.RedButton.Invoke();
            this.logger.Info("Service started");

            var serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];
            this.owinWeb = WebApp.Start(new StartOptions()
            {
                AppStartup = typeof(OwinStartup).AssemblyQualifiedName,
                Urls = { serviceUrl }
            });
        }

        public virtual void Shutdown()
        {
            this.logger.Info("Service shutdown");
        }

        public virtual void Stop()
        {
            this.owinWeb.Dispose();

            var bus = IocContainerProvider.CurrentContainer.GetInstance<IPlatformBus>();
            bus?.Dispose();

            this.logger.Info("Service stopped");
        }
    }
}
