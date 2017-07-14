namespace Platform.Utils.ServiceBus.Receiving
{
    /// <summary>
    /// Consumer that is able to handle messages from sb
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumer<T> where T : class
    {
        /// <summary>
        /// handle message from sb
        /// </summary>
        /// <param name="ctx"></param>
        void Handle(IConsumerContext<T> ctx);
    }
}
