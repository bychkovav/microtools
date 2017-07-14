using System;
using System.Linq;
using System.Linq.Expressions;
using SimpleInjector;

namespace Platform.Utils.Ioc
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Init and set simple injector container
    /// </summary>
    public class IocContainerProvider
    {
        /// <summary>
        /// Sync object
        /// </summary>
        private static readonly object _lockObj = new object();

        /// <summary>
        /// Current SI container. 
        /// </summary>
        public static Container CurrentContainer { get; private set; }

        /// <summary>
        /// Initialize container from ioc modules in solution.
        /// </summary>
        public static void InitIoc()
        {   
            var container = GetContainer();

            // we need to sync it despite the fact of static member because Registering of objects is not thread safe operation.
            lock (_lockObj)
            {
                List<Type> list = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).ToList();
                foreach (Type module in list.Where(x => x.GetInterfaces().Contains(typeof(IPlatformIocModule))).DistinctBy(x=>x.FullName))
                {
                    LoadModule(module, container);
                }
            }

            CurrentContainer = container;
        }

        public static void InitIoc(List<Type> modules)
        {
            var container = GetContainer();
            lock (_lockObj)
            {
                foreach (
                    Type module in
                        modules.Where(x => x.GetInterfaces().Contains(typeof (IPlatformIocModule)))
                            .DistinctBy(x => x.FullName))
                {
                    LoadModule(module, container);
                }
            }

            CurrentContainer = container;
        }


        private static void LoadModule(Type module, Container container)
        {
            Trace.WriteLine(string.Format("IoC module found:{0} {1}", module.FullName, module.Assembly.FullName));
            var moduleInit = Expression.MemberInit(Expression.New(module));
            var callingMethod = Expression.Call(moduleInit, module.GetMethod("Register"),
                new[] { Expression.Constant(container, typeof(Container)) });
            Expression.Lambda<Action>(callingMethod).Compile().Invoke();
        }

        private static Container GetContainer()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.Options.SuppressLifestyleMismatchVerification = true;
            return container;
        }
    }
}
