namespace Platform.Utils.Owin.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Events.Domain.Enums;
    using Events.Domain.Objects;
    using Events.Manager.Interfaces;
    using Events.QueryGenerator;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Platform.Utils.Domain.Objects;
    using Platform.Utils.Owin.Models;

    public abstract class ModelApiControllerBase : ControllerBase
    {
        private readonly IEventSender eventSender;
        private readonly QueryGenerator queryGenerator;
        private readonly string modelClassNameInUpperCamelCaseExcludeDots;

        protected ModelApiControllerBase(IEventSender eventSender, QueryGenerator queryGenerator, string modelClassNameInUpperCamelCaseExcludeDots)
        {
            this.eventSender = eventSender;
            this.queryGenerator = queryGenerator;
            this.modelClassNameInUpperCamelCaseExcludeDots = modelClassNameInUpperCamelCaseExcludeDots;
        }

        private IList<ModelElementObjectBase> allClassDescriptions;
        protected IList<ModelElementObjectBase> AllClassDescriptions
        {
            get
            {
                if (this.allClassDescriptions == null)
                {
                    var assembly = GetType().Assembly;
                    var resourceName = $"{assembly.GetName().Name}.{this.modelClassNameInUpperCamelCaseExcludeDots}ClassDescriptions.txt";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Debug.Assert(stream != null, "stream != null");

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            this.allClassDescriptions = JsonConvert.DeserializeObject<IList<ModelElementObjectBase>>(reader.ReadToEnd());
                        }
                    }
                }

                return this.allClassDescriptions;
            }
        }

        private async Task<ExecutionResult<dynamic>> SendEvent(JObject edmObject, string path, string eventCode)
        {
            TaskCompletionSource<ExecutionResult<dynamic>> resultTsk = new TaskCompletionSource<ExecutionResult<dynamic>>();
          //  var exc = GetExecutionContextContainer();

            /*   var ctx = new TransactionalMsgContext()
               {
                   Event = new Envelope()
                   {
                       Content = edmObject.ToString(),
                       EventPath = path,
                       EventType = MsgType.Command,
                       //EventPhase = "Invoked",  UNCOMMENT IT WHen generation will be fixed
                       EventAction = eventCode
                   },
                   ExecutionContext = exc,
                   ResultTsk = resultTsk,
                   Data = edmObject,
               };

               this.eventSender.SendEvent(ctx);

               var res = await resultTsk.Task;
               return res;*/
            return new ExecutionResult<dynamic>();
        }

        public async Task<ServiceResponseModel<IList<JObject>>> GetManyInternal(JObject edmObject, string path, string eventCode = "Query")
        {
            return await GetManyOrListInternal(edmObject, path, eventCode);
        }

        public async Task<ServiceResponseModel<IList<JObject>>> GetListInternal(JObject edmObject, string path, string eventCode = "Get")
        {
            return await GetManyOrListInternal(edmObject, path, eventCode);
        }

        private async Task<ServiceResponseModel<IList<JObject>>> GetManyOrListInternal(JObject edmObject, string path, string eventCode)
        {
            var res = await SendEvent(edmObject, path, eventCode);

            var resTyped = (res.Value as IEnumerable<JToken>)?
                .Select(x => x as JObject)
                .ToList();

            return CreateResponse<IList<JObject>>(res, serviceResponse => { serviceResponse.Model = resTyped; });
        }

        public async Task<ServiceResponseModel<object>> GetSingleInternal(JObject edmObject, string path, string eventCode = "Get")
        {
            var res = await SendEvent(edmObject, path, eventCode);

            var resTyped = (res.Value as IEnumerable<JToken>)?.FirstOrDefault();

            return CreateResponse<object>(res, serviceResponse =>
            {
                serviceResponse.Model = resTyped ?? res.Value;
            });
        }

        public async Task<ServiceResponseModel> DeleteInternal(JObject edmObject, string path, string eventCode = "Delete")
        {
            var res = await SendEvent(edmObject, path, eventCode);

            return CreateResponse(res);
        }

        public async Task<ServiceResponseModel<object>> PutInternal(JObject edmObject, string path, string eventCode = "Add")
        {
            var res = await SendEvent(edmObject, path, eventCode);

            return CreateResponse<object>(res, serviceResponse =>
            {
                serviceResponse.Model = res.Value;
            });
        }

        public async Task<ServiceResponseModel> PatchInternal(JObject edmObject, string path, string eventCode = "Set")
        {
            var res = await SendEvent(edmObject, path, eventCode);

            return CreateResponse(res);
        }

        public async Task<ServiceResponseModel<object>> PostInternal(JObject edmObject, string path, string eventCode = "Initiate")
        {
            var res = await SendEvent(edmObject, path, eventCode);

            return CreateResponse<object>(res, serviceResponse =>
            {
                serviceResponse.Model = res.Value;
            });
        }

        public async Task<ServiceResponseModel<object>> Custom(JObject edmObject, string path, string customAction)
        {
            var res = await SendEvent(edmObject, path, customAction);

            return CreateResponse<object>(res, serviceResponse =>
            {
                serviceResponse.Model = res.Value;
            });
        }

        protected bool DisableDots()
        {
            return this.Request.Headers.Contains("X-Platform-Dots") &&
                   this.Request.Headers.GetValues("X-Platform-Dots").FirstOrDefault() == "false";
        }
    }
}