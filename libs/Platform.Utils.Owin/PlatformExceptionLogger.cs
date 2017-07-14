namespace Platform.Utils.Owin
{
    using System.Web.Http.ExceptionHandling;
    using global::NLog;

    public class PlatformExceptionLogger : ExceptionLogger
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public override void Log(ExceptionLoggerContext context)
        {
            //NOTE: Simple for now. But could be enhanced.
            this.logger.Error(context.ExceptionContext.Exception.ToString());
        }

        public override bool ShouldLog(ExceptionLoggerContext context)
        {
            return true;
        }
    }
}
