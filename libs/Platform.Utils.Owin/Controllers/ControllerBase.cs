namespace Platform.Utils.Owin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Web.Http;
    using Domain.Objects;
    using Events.Domain.Objects.ExecutionContext;
    using Helpers;
    using Models;

    public class ControllerBase : ApiController
    {
        protected virtual void ModelStateToResponse(ServiceResponseModel response)
        {
            foreach (var modelKey in ModelState.Keys)
            {
                var modelErrorCollection = ModelState[modelKey].Errors;
                foreach (var error in modelErrorCollection)
                {
                    response.Errors.Add(new ErrorInfo(modelKey, error.ErrorMessage));
                }
            }
        }

        protected virtual ServiceResponseModel CreateResponse()
        {
            return CreateResponse(null, null);
        }

        protected virtual ServiceResponseModel CreateResponse(ExecutionResult result)
        {
            return CreateResponse(result, null);
        }

        protected virtual ServiceResponseModel CreateResponse(Action<ServiceResponseModel> builder)
        {
            return CreateResponse(null, builder);
        }

        protected virtual ServiceResponseModel CreateResponse(ExecutionResult result, Action<ServiceResponseModel> builder)
        {
            var serviceResponseModel = ApiResultHelper.CreateResponse(result);
            ModelStateToResponse(serviceResponseModel);
            builder?.Invoke(serviceResponseModel);
            return serviceResponseModel;
        }

        protected virtual ServiceResponseModel<TModel> CreateResponse<TModel>()
        {
            return CreateResponse<TModel>(null, null);
        }

        protected virtual ServiceResponseModel<TModel> CreateResponse<TModel>(Action<ServiceResponseModel<TModel>> builder)
        {
            return CreateResponse<TModel>(null, builder);
        }

        protected virtual ServiceResponseModel<TModel> CreateResponse<TModel>(ExecutionResult result)
        {
            return CreateResponse<TModel>(result, null);
        }

        protected virtual ServiceResponseModel<TModel> CreateResponse<TModel>(ExecutionResult result, Action<ServiceResponseModel<TModel>> builder)
        {
            var serviceResponseModel = ApiResultHelper.CreateResponse<TModel>(result);
            ModelStateToResponse(serviceResponseModel);
            if (builder != null)
            {
                builder(serviceResponseModel);
            }
            return serviceResponseModel;
        }

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }

        /*    protected ExecutionContextContainer GetExecutionContextContainer()
            {
                ExecutionContextContainer exc = new ExecutionContextContainer()
                {
                    ExecutionContexts = new List<ExecutionContext>(),
                    Request = Request
                };

                var identity = User?.Identity as ClaimsIdentity;
                if (identity?.Claims != null)
                {
                    var localMasterId = identity.Claims.FirstOrDefault(x => x.Type == "localMasterId");
                    if (localMasterId != null)
                    {
                        var it = new ExecutionContext()
                        {
                            LocalMasterId = Guid.Parse(localMasterId.Value),
                            CoreIdentities = new CoreIdentities()
                        };

                        exc.ExecutionContexts.Add(it);
                    }
                }

                return exc;
            }*/
    }
}