namespace Platform.Utils.Events.Manager.Interfaces
{
    using Domain.Objects;

    public interface IEventHandler
    {
        void Handle(MsgContext ectx);
    }
}
