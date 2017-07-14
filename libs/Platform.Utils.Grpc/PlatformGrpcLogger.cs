namespace Platform.Utils.Grpc
{
    using System;
    using NLog;

    public class PlatformGrpcLogger : global::Grpc.Core.Logging.ILogger
    {
        private readonly Type targetType;

        private readonly ILogger logger;

        public PlatformGrpcLogger(Type targetType)
        {
            this.targetType = targetType;
            this.logger = LogManager.GetLogger(this.targetType != null ? this.targetType.FullName : string.Empty);
        }

        public global::Grpc.Core.Logging.ILogger ForType<T>()
        {
            return typeof(T) == this.targetType ? this : new PlatformGrpcLogger(typeof(T));
        }

        public void Debug(string message)
        {
            this.logger.Debug(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            this.logger.Debug(format, formatArgs);
        }

        public void Info(string message)
        {
            this.logger.Info(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            this.logger.Info(format, formatArgs);
        }

        public void Warning(string message)
        {
            this.logger.Warn(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            this.logger.Warn(format, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            this.logger.Warn(exception, message);
        }

        public void Error(string message)
        {
            this.logger.Error(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            this.logger.Error(format, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            this.logger.Error(exception, message);
        }
    }
}
