namespace Platform.Utils.ServiceBus.Core
{
    using System;

    public delegate object DependencyResolverFunc(Type type);

    public class LambdaDependencyResolver : IDependencyResolver
    {
        private readonly DependencyResolverFunc resolver;

        public LambdaDependencyResolver(DependencyResolverFunc resolver)
        {
            this.resolver = resolver;
        }

        public object Resolve(Type type)
        {
            return this.resolver(type);
        }
    }
}
