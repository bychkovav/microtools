namespace Platform.Utils.Events.Manager
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain.Objects;
    using Domain.Objects.ExecutionContext;
    using Interfaces;
    using Utils.Domain.Objects;

    public class EventNotifier
    {
        #region [Fields]

        private static IList<IEventObserver> observers = new List<IEventObserver>();

        private readonly StorageProvider storageProvider;

        public EventNotifier(StorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        #endregion

        internal static void Init(IList<IEventObserver> obs)
        {
            observers = obs;
        }

        public void PushEvent(Envelope env)
        {
            foreach (var eventObserver in observers)
            {
                eventObserver.EventPushed(env);
            }
        }
    }
}
