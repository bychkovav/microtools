namespace Platform.Utils.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Http.Dispatcher;
    using global::NLog;

    /// <summary>
    /// The platform assemblies resolver.
    /// </summary>
    public class PlatformAssembliesResolver : DefaultAssembliesResolver
    {
        private const string ControllerClassPostfix = "Controller";

        public override ICollection<Assembly> GetAssemblies()
        {
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();

            List<Assembly> internalAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetTypes().Any(x => x.Name.Contains(ControllerClassPostfix)))
                .ToList();

            var logger = LogManager.GetCurrentClassLogger();
            logger.Warn($" {internalAssemblies.Count} CONTROLLERS ASSEMBLIES FOUND:");
            foreach (var internalAssembly in internalAssemblies)
            {
                logger.Warn($"CONTROLLER ASSEMBLY FOUND: {internalAssembly}");
            }

            return baseAssemblies.Union(internalAssemblies).ToList();
        }
    }
}
