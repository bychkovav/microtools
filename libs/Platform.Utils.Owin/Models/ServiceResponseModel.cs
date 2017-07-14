namespace Platform.Utils.Owin.Models
{
    using System.Collections.Generic;
    using Domain.Objects;

    public class ServiceResponseModel
    {
        private bool? status;

        internal ServiceResponseModel()
        {
            Errors = new List<ErrorInfo>();
        }

        public bool Status
        {
            get { return this.status ?? (Errors == null || Errors.Count == 0); }
            set { this.status = value; }
        }

        public string Version { get; set; }

        public IList<ErrorInfo> Errors { get; set; }
    }

    public class ServiceResponseModel<T> : ServiceResponseModel
    {
        public T Model { get; set; }

        internal ServiceResponseModel()
        {

        }
    }
}