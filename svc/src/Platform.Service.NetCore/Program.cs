using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;
using Platform.Utils;
using Platform.Utils.Agents.Interfaces;
using Platform.Utils.Common;
using Platform.Utils.Ioc;

namespace Platform.Service.NetCore
{
    public class Program
    {
        private const string DynamicLibPath = "extensions";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var cfg = builder.Build();

            LoadExtensions();

            Logger.Info("IoC initializing....");
            try
            {
                IocContainerProvider.InitIoc(cfg);
                IocContainerProvider.CurrentContainer.Verify();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderException in ex.LoaderExceptions)
                    {
                        Logger.Error(loaderException);
                    }
                }
                throw;
            }
            Logger.Info("IoC initialized.");

            InitExtensions(cfg);

            var agentManager = IocContainerProvider.CurrentContainer.GetInstance<IAgentManager>();
            agentManager.StartManager();

            var wait = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) => wait.Set();

            Logger.Info("The service is now running, press Control+C to exit.");
            wait.WaitOne();
        }

        public static IList<Assembly> LoadExtensions()
        {
            IList<Assembly> result = new List<Assembly>();
            if (Directory.Exists(DynamicLibPath))
            {
                foreach (var dll in Directory.GetFiles(DynamicLibPath, "*.dll"))
                {
                    var loaded = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                    Logger.Info($"LOADING EXTENSION {loaded.GetName().Name}");
                }
            }
            else
            {
                Logger.Warn("NO EXTENSIONS FOUND");
            }

            return result;
        }

        public static void InitExtensions(IConfigurationRoot cfg)
        {
            Logger.Info("Initializing extensions...");
            var list = AssemblyHelper.GetAssemblies().SelectMany(x => x.GetTypes());

            foreach (var conf in list.Where(p => typeof(IPlatformExtension).IsAssignableFrom(p) && !p.GetTypeInfo().IsInterface && !p.GetTypeInfo().IsAbstract))
            {
                Logger.Info("Initializing {0}", conf.AssemblyQualifiedName);

                var appConfig = Expression.MemberInit(Expression.New(conf));
                var callingMethod = Expression.Call(appConfig, conf.GetMethod("Load"), Expression.Constant(cfg, typeof(IConfigurationRoot)));
                Expression.Lambda<Action>(callingMethod).Compile().Invoke();
            }
        }
    }
}
