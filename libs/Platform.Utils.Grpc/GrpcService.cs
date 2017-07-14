namespace Platform.Utils.Grpc
{
    using System.Configuration;
    using Configuration;
    using global::Grpc.Core;
    using NLog;
    using Utils.Rpc;

    public interface IGrpcServiceDefinition
    {
        ServerServiceDefinition GetServerServiceDefinition();
    }

    public class GrpcService : IRemoteService
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        //private static readonly string StringType =
        //    $"{typeof(string).FullName},{typeof(string).Assembly.GetName().Name}";

        private readonly object lockRoot = new object();

        private Server server;

        private readonly IGrpcServiceDefinition grpcServiceDefinition;

        public GrpcService(IGrpcServiceDefinition grpcServiceDefinition)
        {
            this.grpcServiceDefinition = grpcServiceDefinition;
        }

        //public override async Task<RpcResponse> RemoteCall(RpcRequest request, ServerCallContext context)
        //{
        //    TaskCompletionSource<ExecutionResult<object>> resultTsk =
        //        new TaskCompletionSource<ExecutionResult<object>>();

        //    this.logger.Info($"Remote Call: {request.Code}:{request.Body}");

        //    try
        //    {
        //        var eventMessage = new EventMessage()
        //        {
        //            EventCode = request.Code,
        //            Content = request.Body,
        //            ContentType = StringType
        //        };

        //        this.notifier.PushEvent(eventMessage, new ExecutionContextContainer()
        //        {
        //            ExecutionContexts = new List<ExecutionContext>(),
        //        }, resultTsk);

        //        var executionResult = await resultTsk.Task;
        //        var response = GetResponseMessage(request, executionResult.Value);
        //        foreach (var errorInfo in executionResult.Errors)
        //        {
        //            response.Errors.Add(new ErrorInfo()
        //            {
        //                Key = errorInfo.Key,
        //                ErrorMessage = errorInfo.ErrorMessage
        //            });
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        this.logger.Error(ex);
        //        var response = GetResponseMessage(request, null);
        //        response.Errors.Add(new ErrorInfo() { Key = ex.Message, ErrorMessage = ex.ToString() });
        //        return response;
        //    }
        //}


        public void Start(params object[] startParams)
        {
            if (this.server == null)
            {
                lock (this.lockRoot)
                {
                    if (this.server == null)
                    {
                        var host = ConfigurationManager.AppSettings["grpcHost"] ?? ServiceSection.Current?.ServiceHost?.Host;
                        if (string.IsNullOrEmpty(host))
                        {
                            throw new ConfigurationErrorsException("GRPC Host is not provided in config section or appsetting");
                        }

                        var appSettingPort = ConfigurationManager.AppSettings["grpcPort"];
                        int appSettingPortInt = 0;
                        if (!string.IsNullOrEmpty(appSettingPort))
                        {
                            int.TryParse(appSettingPort, out appSettingPortInt);
                        }
                        var port = appSettingPortInt > 0 ? appSettingPortInt : ServiceSection.Current?.ServiceHost?.Port;
                        if (!port.HasValue || port.Value <= 0)
                        {
                            throw new ConfigurationErrorsException("GRPC Port is not provided in config section or appsetting");
                        }
                        this.logger.Info($"Starting GRPC Service {host}:{port}");

                        GrpcEnvironment.SetLogger(new PlatformGrpcLogger(GetType()));
                        this.server = new Server
                        {

                            Services = { this.grpcServiceDefinition.GetServerServiceDefinition() },
                            Ports =
                            {
                                new ServerPort(host,
                                    port.Value, ServerCredentials.Insecure)
                            }
                        };
                        this.server.Start();

                        this.logger.Info($"GRPC Service {host}:{port} started");
                    }
                }
            }
        }

        public void Stop()
        {
            lock (this.lockRoot)
            {
                this.logger.Info($"Stopping GRPC Service {ServiceSection.Current.ServiceHost.Host}:{ServiceSection.Current.ServiceHost.Port}");
                this.server.ShutdownAsync().GetAwaiter().GetResult();
                this.logger.Info($"GRPC Service {ServiceSection.Current.ServiceHost.Host}:{ServiceSection.Current.ServiceHost.Port} stopped");
            }
        }
    }
}
