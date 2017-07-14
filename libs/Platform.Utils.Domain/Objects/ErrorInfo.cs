using System;

namespace Platform.Utils.Domain.Objects
{
    /// <summary>
    /// Information about error
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Default constructor of class
        /// </summary>
        public ErrorInfo()
            : this(String.Empty, String.Empty)
        {
        }

        public ErrorInfo(string errorMessage)
            : this(Guid.NewGuid().ToString(), errorMessage)
        {
        }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="key">Key of error (for example, property name)</param>
        /// <param name="errorMessage">Error message</param>

        public ErrorInfo(string key, string errorMessage)
        {
            Key = key;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="key">Key of error (for example, property name)</param>
        /// <param name="message">Error message</param>
        /// <param name="messageParams">Error message params</param>
        public ErrorInfo(string key, string message, params object[] messageParams)
            : this(key ?? Guid.NewGuid().ToString(), message)
        {
            Params = messageParams;
        }

        /// <summary>
        /// Key of error
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Error message params
        /// </summary>
        public object[] Params { get; set; }

        public override string ToString()
        {
            return String.Format("{0}. Key: '{1}', ErrorMessage: '{2}'", base.ToString(), Key, ErrorMessage);
        }
    }
}
