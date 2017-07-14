namespace Platform.Utils.Hangfire
{
    using System.Configuration;
    using global::Hangfire;
    using global::Hangfire.Redis;
    using global::Hangfire.SimpleInjector;
    using Ioc;

    public class HangfireBootstrapper
    {
        private readonly object lockObject = new object();

        private BackgroundJobServer backgroundJobServer;

        public bool Started { get; private set; }

        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private HangfireBootstrapper()
        {
        }

        public void Start()
        {
            lock (this.lockObject)
            {
                if (this.Started)
                {
                    return;
                }

                var redisConnectionStr = ConfigurationManager.ConnectionStrings["RedisServer"];
                if (redisConnectionStr == null)
                {
                    throw new ConfigurationErrorsException("No Hangfire Redis connection string provided with the name \"RedisServer\". Please provide it as follows: <add name=\"RedisServer\"...  ");
                }

                var redisDb = ConfigurationManager.AppSettings["redisHangfireDb"];
                if (string.IsNullOrEmpty(redisDb))
                {
                    throw new ConfigurationErrorsException("No Hangfire Redis db provided in app settings. Please provide it as follows: <add key=\"redisHangfireDb\" value=\"No of db\" />");
                }

                GlobalConfiguration.Configuration.UseActivator(new SimpleInjectorJobActivator(IocContainerProvider.CurrentContainer));
                JobStorage.Current = new RedisStorage(redisConnectionStr.ConnectionString, new RedisStorageOptions()
                {
                    Db = int.Parse(redisDb),
                    DeletedListSize = 20,
                    SucceededListSize = 20,
                    Prefix = "_hangFire:"
                });

                this.backgroundJobServer = new BackgroundJobServer();

                this.Started = true;
            }
        }

        public void Stop()
        {
            lock (this.lockObject)
            {
                this.backgroundJobServer?.Dispose();
                this.Started = false;
            }
        }
    }
}