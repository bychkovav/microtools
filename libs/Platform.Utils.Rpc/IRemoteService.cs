namespace Platform.Utils.Rpc
{
    public interface IRemoteService
    {
        void Start(params object[] startParams);

        void Stop();
    }
}
