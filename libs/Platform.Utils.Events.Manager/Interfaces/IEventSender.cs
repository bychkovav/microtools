namespace Platform.Utils.Events.Manager.Interfaces
{
    using System.Threading.Tasks;
    using Domain.Objects;
    using Domain.Objects.ExecutionContext;
    using Utils.Domain.Objects;

    public interface IEventSender
    {
        //void SendEvent(Envelope obj, ExecutionContextContainer contextContainer = null, TaskCompletionSource<ExecutionResult<object>> resultTsk = null);

        void SendEvent(Envelope env);
    }
}
