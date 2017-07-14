namespace Platform.Utils.Events.ScriptEngine
{
    using System;

    //TODO:NOTE: Move to a separate project
    public class ManagerDependencyResolver
    {
        private readonly Func<Type, object> resolver;

        public ManagerDependencyResolver(Func<Type, object> resolver)
        {
            this.resolver = resolver;
        }

        public object Resolve(Type t)
        {
            return this.resolver(t);
        }
    }
}
