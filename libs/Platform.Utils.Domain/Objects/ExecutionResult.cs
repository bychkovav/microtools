using System;
using System.Collections.Generic;

namespace Platform.Utils.Domain.Objects
{
    /// <summary>
    /// Represents result of an action.
    /// </summary>
    [Serializable]
    public class ExecutionResult
    {
        private bool? success;

        public ExecutionResult()
            : this((ExecutionResult)null)
        {
        }

        public ExecutionResult(ErrorInfo error)
            : this(new[] { error })
        {
        }

        public ExecutionResult(IEnumerable<ErrorInfo> errors)
            : this((ExecutionResult)null)
        {
            foreach (var errorInfo in errors)
            {
                Errors.Add(errorInfo);
            }
        }

        public ExecutionResult(ExecutionResult result)
        {
            if (result != null)
            {
                Success = result.Success;
                Errors = new List<ErrorInfo>(result.Errors);
            }
            else
            {
                Errors = new List<ErrorInfo>();
            }
        }

        /// <summary>
        ///     Indicates if result is successful.
        /// </summary>
        public bool Success
        {
            get { return this.success ?? Errors.Count == 0; }
            set { this.success = value; }
        }

        /// <summary>
        /// 	Gets a list of errors.
        /// </summary>
        public IList<ErrorInfo> Errors { get; private set; }

        public static ExecutionResult SuccessResult()
        {
            return new ExecutionResult();
        }

        public static ExecutionResult<T> SuccessResult<T>(T result)
        {
            return new ExecutionResult<T>(result);
        }

        public static ExecutionResult ErrorResult(string message)
        {
            return new ExecutionResult(new ErrorInfo(message));
        }

        public static ExecutionResult<T> ErrorResultFor<T>(T result, string message)
        {
            return new ExecutionResult<T>(new ErrorInfo(message));
        }

        public static ExecutionResult<T> ErrorResultFor<T>(T result, string key, string message)
        {
            return new ExecutionResult<T>(new ErrorInfo(key, message));
        }
    }

    /// <summary>
    /// Represents result of an action that returns any value
    /// </summary>
    /// <typeparam name="T">Type of value to be returned with action</typeparam>
    [Serializable]
    public class ExecutionResult<T> : ExecutionResult
    {
        public ExecutionResult()
            : this((ExecutionResult)null)
        {
        }

        public ExecutionResult(T result)
            : this((ExecutionResult)null)
        {
            Value = result;
        }

        public ExecutionResult(ExecutionResult result)
            : base(result)
        {
            var r = result as ExecutionResult<T>;
            if (r != null)
            {
                Value = r.Value;
            }
        }

        public ExecutionResult(ErrorInfo error)
            : this(new[] { error })
        {
        }

        public ExecutionResult(IEnumerable<ErrorInfo> errors)
            : this((ExecutionResult)null)
        {
            foreach (var errorInfo in errors)
            {
                Errors.Add(errorInfo);
            }
        }

        public T Value { get; set; }

        
    }
}
