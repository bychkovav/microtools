namespace Platform.Utils.Owin.Filters
{
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Web.Http.Filters;
    using Domain.Objects;
    using Domain.Objects.Exceptions;
    using global::NLog;
    using Models;

    public class ExceptionFilterAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            this.logger.Error(context.Exception);
            var entityNotFoundException = context.Exception as EntityNotFoundException;
            if (entityNotFoundException != null)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.NotFound,
                    new ServiceResponseModel
                    {
                        Status = false,
                        Errors = new ErrorInfo[] { new ErrorInfo(entityNotFoundException.ToString()), }
                    });
                return;
            }

            var notAuth = context.Exception as NotAuthorizedException;
            if (notAuth != null)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.Unauthorized,
                   new ServiceResponseModel
                   {
                       Status = false,
                       Errors = new ErrorInfo[] { new ErrorInfo(notAuth.ToString()), }
                   });
                return;
            }

            var security = context.Exception as SecurityException;
            if (security != null)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.Forbidden,
                   new ServiceResponseModel
                   {
                       Status = false,
                       Errors = new ErrorInfo[] { new ErrorInfo(security.ToString()), }
                   });
                return;
            }

            if (context.Exception != null)
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new ServiceResponseModel
                    {
                        Status = false,
                        Errors = new ErrorInfo[] { new ErrorInfo(context.Exception.ToString()), }
                    });
            }
        }
    }
}
