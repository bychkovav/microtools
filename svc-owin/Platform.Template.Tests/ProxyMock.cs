using Platform.Utils.Events.Domain.Objects;

namespace Platform.Template.Tests
{
    using Utils.Events.Manager;
    using Utils.Events.ScriptEngine;

    public class ProxyMock : EventApplicationProxy
    {
        public dynamic ServiceContainer
        {
            get
            {
                dynamic c = base.GetServicesContainer();
                c.Context = new EventContext();
                return c;
            }
        }

        public ProxyMock(ManagerDependencyResolver resolver) : base(resolver)
        {
        }
    }
}
