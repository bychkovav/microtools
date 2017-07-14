using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using NLog;

namespace Platform.Utils.Owin
{
    public class RequestTracingMiddleware : OwinMiddleware
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RequestTracingMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            Logger.Debug($"{request.Method} {request.Uri} {request.ContentType}");

            await this.Next.Invoke(context);
        }
    }
}