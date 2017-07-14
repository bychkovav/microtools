namespace Platform.Utils.Events.ScriptEngine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Fasterflect;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Utils.Domain.Objects.Exceptions;

    public abstract class ApplicationProxyBase
    {
        protected readonly ConcurrentDictionary<string, object> ScriptObjects = new ConcurrentDictionary<string, object>();

        protected readonly ManagerDependencyResolver Resolver;

        protected readonly IList<IAppProxyExtension> ProxyExtensions;

        protected ApplicationProxyBase(ManagerDependencyResolver resolver)
        {
            this.Resolver = resolver;

            var proxyExtensionsTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IAppProxyExtension)
                .IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            this.ProxyExtensions = proxyExtensionsTypes.Select(obsType => (IAppProxyExtension)this.Resolver.Resolve(obsType)).ToList();
        }

        public virtual ExpandoObject GetServicesContainer()
        {
            dynamic serviceContainer = new ExpandoObject();
            foreach (var appProxyExtension in this.ProxyExtensions)
            {
                if (appProxyExtension == null)
                {
                    throw new EntityNotFoundException("Please make sure you added proxy extension to ioc module");
                }
                appProxyExtension.AddToServiceContainer(serviceContainer);
            }

            return serviceContainer;
        }

        internal bool CheckExist(string scriptUniqId)
        {
            return this.ScriptObjects.ContainsKey(scriptUniqId);
        }

        internal void CompileAndLoad(string scriptUniqueId, string sctipt, List<MetadataReference> listRef)
        {
            if (!this.ScriptObjects.ContainsKey(scriptUniqueId))
            {
                using (var ms = new MemoryStream())
                {
                    var assemblyFileName = "gen" + Guid.NewGuid().ToString().Replace("-", "") + ".dll";
                    var compilation = CSharpCompilation.Create(assemblyFileName,
                        new[] { CSharpSyntaxTree.ParseText(sctipt) },
                        listRef,
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        );

                    var res = compilation.Emit(ms);

                    if (res.Success)
                    {
                        var bytes = ms.GetBuffer();

                        Assembly ass = Assembly.Load(bytes);

                        var dynamicObject = ass.CreateInstance("DynamicClass");
                        if (dynamicObject == null)
                            throw new TypeLoadException("DynamicClass");

                        this.ScriptObjects.TryAdd(scriptUniqueId, dynamicObject);
                    }
                    else
                    {
                        throw new Exception(string.Format("Can't compile because of errors: {0}",
                            string.Join("\n", res.Diagnostics.Select(x => x.GetMessage()))));
                    }
                }
            }
        }

        public void Exec(string scriptId, object inputData)
        {
            object cl;
            this.ScriptObjects.TryGetValue(scriptId, out cl);
            if (cl == null)
            {
                throw new ArgumentException(string.Format("No script for id {0}", scriptId));
            }

            dynamic sc = GetServicesContainer();
            sc.Context = inputData;

            cl.CallMethod("DynamicMethod", inputData, (ExpandoObject)sc);
        }
    }
}
