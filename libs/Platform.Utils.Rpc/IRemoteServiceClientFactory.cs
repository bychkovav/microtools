namespace Platform.Utils.Rpc
{
    public interface IRemoteServiceClientFactory
    {
        object GetClient(string name);
    }
}
