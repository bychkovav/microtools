namespace Platform.Utils.Owin.Helpers
{
    using System;
    using Domain.Objects;
    using Models;

    internal static class ApiResultHelper
    {
        #region Private Methods

        public static string ParseException(Exception ex)
        {
            return
                $"{ex.Message} \n {ex.InnerException?.Message ?? string.Empty} \n {ex.Source}";
        }

        public static ServiceResponseModel CreateResponse()
        {
            return CreateResponse(new ExecutionResult());
        }

        public static ServiceResponseModel CreateResponse(ExecutionResult result)
        {
            var response = new ServiceResponseModel();
            ResultToReponse(response, result);
            return response;
        }

        public static ServiceResponseModel<TModel> CreateResponse<TModel>(ExecutionResult result)
        {
            var response = new ServiceResponseModel<TModel>();
            ResultToReponse(response, result);
            return response;
        }

        private static void ResultToReponse(ServiceResponseModel response, ExecutionResult result)
        {
            if (result != null)
            {
                response.Errors = result.Errors;
                response.Status = result.Success;
            }
        }

        #endregion
    }
}