namespace Platform.Utils.Events.Manager.Interfaces
{
    using Domain.Objects;

    public interface IEventObserver
    {
        void EventPushed(Envelope env);
    }
}
