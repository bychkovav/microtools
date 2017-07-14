namespace Platform.Utils.Events.Manager.Interfaces
{
    using Domain.Objects;
    using Utils.Domain.Objects;

    public interface IEventInterceptor
    {
        ExecutionResult PreProcess(MsgContext ectx);

        bool SupportEvent(string eventKey);

        int Order { get; }
    }
}
