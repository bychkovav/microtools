namespace Platform.Utils.Nhibernate
{
    using System;
    using System.Threading;

    using NHibernate;
    using NHibernate.Context;
    using NHibernate.Engine;

    using NLog;

    /// <summary>
    /// The ms session context.
    /// </summary>
    public class PlatformNhSessionContext : CurrentSessionContext
    {
        #region [Constants]

        /// <summary>
        /// The http session key.
        /// </summary>
        private const string HttpSessionKey = "36E3DAF7-8BD4-4BC7-8EC1-A93902E95AAD";

        #endregion

        #region [Fields]

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The thread session.
        /// </summary>
        [ThreadStatic]
        private static ISession threadSession;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformNhSessionContext"/> class.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        public PlatformNhSessionContext(ISessionFactoryImplementor factory)
        {
        }

        /// <summary>
        /// Gets or sets the session.
        /// </summary>
        protected override ISession Session
        {
            get
            {
                var currentContext = ReflectiveHttpContext.HttpContextCurrentGetter();
                if (currentContext != null)
                {
                    var items = ReflectiveHttpContext.HttpContextItemsGetter(currentContext);
                    return items[HttpSessionKey] as ISession;
                }

                return threadSession;
            }

            set
            {
                var currentContext = ReflectiveHttpContext.HttpContextCurrentGetter();
                if (currentContext != null)
                {
                    var items = ReflectiveHttpContext.HttpContextItemsGetter(currentContext);
                    items[HttpSessionKey] = value;
                    return;
                }

                threadSession = value;
            }
        }
    }
}
