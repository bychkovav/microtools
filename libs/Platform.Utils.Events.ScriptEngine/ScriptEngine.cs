namespace Platform.Utils.Events.ScriptEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Domain.Objects.Scripts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CSharp.RuntimeBinder;
    using NLog;
    using QueryParser;
    using QueryParser.Domain.Objects;

    public class ScriptEngine
    {
        #region [Constants]


        #endregion

        #region [Fields]

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Engine parserEngine;

        #endregion

        #region [Properties]

        protected IList<string> Namespaces { get; set; }

        protected IList<Assembly> Assemblies { get; set; }

        #endregion

        public ScriptEngine(Engine parserEngine)
        {
            this.parserEngine = parserEngine;
            Assemblies = new List<Assembly>();
            Namespaces = new List<string>();
            var ass = AssemblyScanner.GetAssembliesAndNamespaces();

            var assemblies = ass.Select(x => x.Item1).ToList();
            var namespaces = ass.Select(x => x.Item2).SelectMany(x => x).Distinct().ToList();

            //TODO: NOTE: Refactor and FIX
            var csharpAss = typeof(CSharpArgumentInfo).Assembly;
            if (!assemblies.Contains(csharpAss))
            {
                assemblies.Add(csharpAss);
            }

            var parseDomain = typeof (SingleQuery).Assembly;
            if (!assemblies.Contains(parseDomain))
            {
                assemblies.Add(parseDomain);
            }

            if (!namespaces.Contains("System.Dynamic"))
            {
                namespaces.Add("System.Dynamic");
            }

            if (!namespaces.Contains("Platform.Utils.Events.QueryParser.Domain.Objects"))
            {
                namespaces.Add("Platform.Utils.Events.QueryParser.Domain.Objects");
            }

            AddAssemblies(assemblies, namespaces);
        }

        public void IncludeAdditionalAssemblies(IList<Assembly> additionalToInclude = null)
        {
            var ass = AssemblyScanner.GetAssembliesAndNamespaces(additionalToInclude);

            var assemblies = ass.Select(x => x.Item1).ToList();
            var namespaces = ass.Select(x => x.Item2).SelectMany(x => x).Distinct().ToList();
            AddAssemblies(assemblies, namespaces);
        }

        public ScriptExecutionResult Execute(string script)
        {
            return Execute(new ScriptDefinition { Script = script });
        }

        public ScriptExecutionResult Execute(string script, ApplicationProxyBase proxy, string scriptId, object inputData)
        {
            if (script == null)
            {
                throw new Exception("Proxy is null");
            }

            var inputType = inputData?.GetType() ?? typeof(object);
            AddAdditionalNamespaces(inputType);

            if (!proxy.CheckExist(scriptId))
            {
                try
                {
                    var listRef = new List<MetadataReference>();

                    foreach (var ass in Assemblies)
                    {
                        AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(ass.Location));
                        listRef.Add(MetadataReference.CreateFromFile(ass.Location));
                    }

                    script = PreprocessScriptText(script, listRef);

                    //script = Regex.Replace(script, QueryLangPattern, match => parserEngine.GetTextLinq(match.Groups[0].Value), RegexOptions.Multiline);

                    proxy.CompileAndLoad(scriptId, script, listRef);
                }
                catch (Exception ex)
                {
                    var error = $"Error while compilation {ex}";
                    this.logger.Error(error);
                    return new ScriptExecutionResult { Success = false, Errors = new List<string> { error } };
                }
            }

            try
            {
                proxy.Exec(scriptId, inputData);
                return new ScriptExecutionResult { Success = true };
            }
            catch (Exception ex)
            {
                var error = $"Error while executing {ex}";
                this.logger.Error(error);
                return new ScriptExecutionResult
                {
                    Success = false,
                    Errors = new List<string> { error }
                };
            }
        }

        public ScriptExecutionResult Execute(ScriptDefinition scriptDefinition)
        {
            return Execute(scriptDefinition.Script, scriptDefinition.Proxy, scriptDefinition.ScriptUniqueId, scriptDefinition.InputData);
        }

        //Public for test purposes
        public string PreprocessScriptText(string input, List<MetadataReference> listRef)
        {
            //TODO:NOTE: We can't move it to reciver mecause it is better to be able to cache it. 

            var result = this.parserEngine.PreprocessScript(input);

            this.logger.Trace(result);

            var methodTemplate = @"{0}
                                       public class DynamicClass{{
                                        public void DynamicMethod(EventContext e, dynamic Services){{
                                            {1}             
                                      }}
                                      }}";

            var usingsStr = string.Join("\n", Namespaces.Select(x => $"using {x};"));
            result = string.Format(methodTemplate, usingsStr, result);
            return result;
        }

        private void AddAdditionalNamespaces(Type inputType)
        {
            if (!Assemblies.Contains(inputType.Assembly))
            {
                Assemblies.Add(inputType.Assembly);
            }
            if (!Namespaces.Contains(inputType.Namespace))
            {
                Namespaces.Add(inputType.Namespace);
            }
        }

        private void AddAssemblies(IEnumerable<Assembly> asseblies, IEnumerable<string> namespaces)
        {
            foreach (var additional in asseblies)
            {
                if (!Assemblies.Contains(additional))
                {
                    Assemblies.Add(additional);
                }
            }

            foreach (var namespaceAdditional in namespaces)
            {
                if (!Namespaces.Contains(namespaceAdditional))
                {
                    Namespaces.Add(namespaceAdditional);
                }
            }
        }
    }
}