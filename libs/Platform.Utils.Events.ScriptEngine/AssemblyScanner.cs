namespace Platform.Utils.Events.ScriptEngine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class AssemblyScanner
    {
        private const string Pattern =
            @"^(?!Platform|System)|^(Platform|System(?!\.Data))(?!.*\.Test|.*\.Web|.*\.CodeDom|.*\.Xml|.*\.Configuration|.*\.Windows|.*\.Moq)";

        public static List<Tuple<Assembly, List<string>>> GetAssembliesAndNamespaces(IList<Assembly> additional = null)
        {
            //TODO: NOTE: Refactor it!
            List<Assembly> loadedAssemblies = additional == null ? AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.FullName.Contains("vshost32")).ToList() : additional.ToList();

            var result = new List<Tuple<Assembly, List<string>>>(loadedAssemblies.Count);
            object sync = new Object();

            foreach (var asm in loadedAssemblies)
            {
                try
                {
                    if (asm.IsDynamic) continue;
                    Type[] exportedTypes = asm.GetExportedTypes();

                    List<string> namespaces = GetUniqueNamespaces(exportedTypes);
                    if (namespaces == null || namespaces.Count == 0) continue;

                    var temp = new List<string>(namespaces.Count);
                    foreach (string @namespace in namespaces)
                    {
                        if (string.IsNullOrWhiteSpace(@namespace)) continue;
                        if (!new Regex(Pattern).IsMatch(@namespace)) continue;

                        temp.Add(@namespace);
                    }
                    temp.TrimExcess();
                    lock (sync)
                    {
                        if (result.FirstOrDefault(x => x.Item1.FullName == asm.FullName) == null)
                            result.Add(new Tuple<Assembly, List<string>>(asm, temp));
                    }
                }
                catch (Exception exception)
                {
                    throw;
                }
            }


            return result;
        }

        private static List<string> GetUniqueNamespaces(IEnumerable<Type> types)
        {
            var uniqueNamespaces = new ConcurrentBag<string>();

            Parallel.ForEach(types, type =>
            {
                if (!uniqueNamespaces.Contains(type.Namespace) && !string.IsNullOrEmpty(type.Namespace))
                    uniqueNamespaces.Add(type.Namespace);
            });

            return uniqueNamespaces.ToList();
        }
    }
}
