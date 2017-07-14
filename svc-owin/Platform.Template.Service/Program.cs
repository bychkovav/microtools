namespace Platform.Template.Service
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NLog;
    using Utils.Owin;

    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            logger.Debug("Started");
#if DEBUG

            var loaded = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedPaths = loaded.Select(a => a.Location).ToArray();

            var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            var toLoad =
                referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase) &&
                                           (r.Contains("Library") || r.Contains("Extension"))).ToList();
            if (toLoad.Any())
            {
                toLoad.ForEach(path =>
                {
                    var assemblyName = AssemblyName.GetAssemblyName(path);
                    logger.Warn($"LOADING EXTENSION IN DEBUG:{assemblyName} ");
                    loaded.Add(AppDomain.CurrentDomain.Load(assemblyName));
                });
            }
            else
            {
                logger.Warn($"NO EXTENSIONS WERE FOUND IN DEBUG");
            }


#endif
            ServiceContainer.Run(() => new OwinService());
        }
    }
}
