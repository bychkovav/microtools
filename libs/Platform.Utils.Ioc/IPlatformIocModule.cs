using SimpleInjector;

namespace Platform.Utils.Ioc
{
    /// <summary>
    /// Is used to bind your interfaces to realization
    /// </summary>
    public interface IPlatformIocModule
    {
        /// <summary>
        /// Implement registering services here.
        /// </summary>
        /// <param name="container"></param>
        void Register(Container container);
    }
}
