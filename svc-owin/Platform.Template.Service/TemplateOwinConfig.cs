using Owin;

namespace Platform.Template.Service
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Http;
    using Data;
    using Data.CasMappings;
    using Hangfire;
    using Newtonsoft.Json.Linq;
    using NLog;
    using Platform.Utils.Owin;
    using Utils.Cassandra;
    using Utils.Dictionaries.Data.CasMappings;
    using Utils.Events.Manager;
    using Utils.Events.Manager.Consumers;
    using Utils.Events.Manager.Interfaces;
    using Utils.Events.QueryParser.Extensions;
    using Utils.Hangfire;
    using Utils.Ioc;
    using Utils.MongoDb;
    using Utils.Owin.Configuration;
    using Utils.ServiceBus;
    using Utils.TransactionCache.Mappings;
    using ILogger = NLog.ILogger;

    public class TemplateOwinConfig : IOwinConfig
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private const string NugetPath = "C:\\tools\\nuget.exe";
        private const string NugetSources =
            @"C:\Users\Administrator2\.nuget\packages\;C:\Users\Administrator\AppData\Local\NuGet\Cache;C:\Users\Administrator\AppData\Local\NuGet\v3-cache;C:\Windows\system32\config\systemprofile\AppData\Local\NuGet\Cache\;C:\Windows\system32\config\systemprofile\.nuget\packages\;https://api.nuget.org/v3/index.json;http://54.187.86.186:10008/nuget/";

        //private readonly PackageManager PlatformPackageManager = new PackageManager(
        //    new AggregateRepository(new[]
        //    {
        //        PackageRepositoryFactory.Default.CreateRepository("http://54.187.86.186:10008/nuget/"),
        //        PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2")
        //    }),
        //    Environment.CurrentDirectory + "\\libTmp");


        public void Register(IAppBuilder app, HttpConfiguration config)
        {
            //HangfireBootstrapper.Instance.Start();
            //RecurringJob.AddOrUpdate(() => Console.WriteLine("Background job works!"),
            //    Cron.Minutely());

            var epaListUrl = ConfigurationManager.AppSettings["epaListUrl"];
            if (string.IsNullOrEmpty(epaListUrl))
            {
                throw new ConfigurationErrorsException("epaListUrl application setting should be specified as a valid url to EPA list.");
            }
            var epaConfigUrl = ConfigurationManager.AppSettings["epaConfigUrl"];
            if (string.IsNullOrEmpty(epaConfigUrl))
            {
                throw new ConfigurationErrorsException("epaConfigUrl application setting should be specified as a valid url to EPA config.");
            }
            var physicalSvcConfigUrl = ConfigurationManager.AppSettings["physicalSvcConfigUrl"];
            if (string.IsNullOrEmpty(physicalSvcConfigUrl))
            {
                throw new ConfigurationErrorsException("physicalSvcConfigUrl application setting should be specified as a valid url to Physical Svc config.");
            }

            var libsUrl = ConfigurationManager.AppSettings["epaLibsUrl"];
            if (string.IsNullOrEmpty(libsUrl))
            {
                throw new ConfigurationErrorsException("epaLibsUrl application setting should be specified as a valid url to Svc Libs List.");
            }

            using (WebClient wc = new WebClient())
            {
                var physicalSvcIdentity = ConfigurationManager.AppSettings["identity"];

                var json = wc.DownloadString(string.Format(physicalSvcConfigUrl, physicalSvcIdentity));

                var physicalCfgArray = JArray.Parse(json);
                var physicalCfg = physicalCfgArray.ToDictionary(x => (string)x["Key"], x => (string)x["Value"]);

                //INIT MD MONGO 
                var mongoPool =
                    IocContainerProvider.CurrentContainer.GetInstance<MongoDataProviderPool>();
                var mongoCs = $"{physicalCfg["mongocs"]}masterdata";
                mongoPool.AddProvider("masterdata", new MongoDataProvider(mongoCs));

                //INIT dictionaries CASSANDRA
                var casPool =
                    IocContainerProvider.CurrentContainer
                        .GetInstance<CassandraDataProviderPool>();
                casPool.AddProvider("dictionaries",
                    new CassandraDataProvider(physicalCfg["DictionaryCassandraCs"], "dictionaries",
                        new Cassandra.Mapping.Mappings[] { new DictionaryMappings() }));

                //INIT Trans Cache CASSANDRA
                casPool.AddProvider("transactionCache",
                    new CassandraDataProvider(physicalCfg["cassandraCs"], "TransactionCache",
                        new Cassandra.Mapping.Mappings[] { new MasterMappings() }));

                json = wc.DownloadString(string.Format(epaListUrl, physicalSvcIdentity));
                var epas = JArray.Parse(json);

                var epasList = epas.Select(x => (string)x["ServiceIdentity"]).ToList();
                this.logger.Debug($"Starting identities: {string.Join(", ", epasList)}");

                foreach (var epaIdentity in epasList)
                {
                    this.logger.Debug($"Starting EPA: {epaIdentity}");

                    var epaJson = wc.DownloadString(string.Format(epaConfigUrl, epaIdentity));
                    var epaConfig = JArray.Parse(epaJson);

                    ConfigureEpa(epaConfig.ToDictionary(x => (string)x["Key"], x => (string)x["Value"]));

                    json = wc.DownloadString(string.Format(libsUrl, epaIdentity));
                    var libsList = JArray.Parse(json).ToDictionary(x => (string)x["LibraryName"], x => (string)x["Version"]);

#if !DEBUG
                    this.logger.Debug("Requesting libs");
                    var nugetRestoreOutput = CommandLineHelper.RunCommandLine("powershell",
                        $".\\request_libs.ps1  -Identity \"{epaIdentity}\" -ServiceRootPath {Environment.CurrentDirectory}");
                    this.logger.Debug(nugetRestoreOutput);
#else
                    this.logger.Debug("Skip requesting libs");
#endif

                    var loadedAsses = OwinService.LoadExtensions();
                    OwinStartup.CallIOwinConfig(app, config, loadedAsses);

                    this.logger.Debug($"EPA {epaIdentity} started");
                }
            }
        }

        public static class CommandLineHelper
        {
            private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

            public static string RunCommandLine(string path, string arguments)
            {
                Logger.Debug($"Execution command: {path} {arguments}");

                var p = new Process
                {
                    StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = path,
                    Arguments = arguments
                }
                };
                // Redirect the output stream of the child process.
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                Logger.Debug($"Output: {output}");
                return output;
            }
        }

        public static void ConfigureEpa(IDictionary<string, string> epaConfig)
        {
            var identity = epaConfig["ServiceIdentity"];

            IocContainerProvider.CurrentContainer.GetInstance<IServiceBus>()
                .Subscribe(identity, IocContainerProvider.CurrentContainer.GetInstance<EventMessageConsumer>());

            //INIT MONGO
            var mongoPool =
                IocContainerProvider.CurrentContainer.GetInstance<MongoDataProviderPool>();

            var mongoCs = $"{epaConfig["mongocs"]}masterdb_{identity}";
            mongoPool.AddProvider(identity, new MongoDataProvider(mongoCs));

            //INIT TRANSACT CASSANDRA
            var casPool =
                IocContainerProvider.CurrentContainer
                    .GetInstance<CassandraDataProviderPool>();
            casPool.AddProvider(identity,
                new TransactionalCasDataProvider(epaConfig["cassandraCs"], $"masterdb_{identity.Replace("-", "_")}",
                    new Cassandra.Mapping.Mappings[] { new EvSourceMappings() }));

            IocContainerProvider.CurrentContainer.GetInstance<IEventSender>().AddIdentity(identity);
            IocContainerProvider.CurrentContainer.GetInstance<IEventReciever>().AddIdentity(identity);
        }
    }
}
