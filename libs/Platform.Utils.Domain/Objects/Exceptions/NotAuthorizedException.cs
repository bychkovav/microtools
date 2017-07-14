using System;

namespace Platform.Utils.Domain.Objects.Exceptions
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() { }

        public NotAuthorizedException(string message) : base(message) { }

        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
