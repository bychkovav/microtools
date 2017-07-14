namespace Platform.Utils.ServiceBus.Core
{
    using System;

    /// <summary>
    /// Interface for determination of dependences while configuring SB
    /// </summary>
    public interface IDependencyResolver
    {
        object Resolve(Type type);
    }
}
