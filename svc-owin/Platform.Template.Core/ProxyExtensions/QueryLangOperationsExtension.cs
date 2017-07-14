namespace Platform.Template.Core.ProxyExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Utils.Events.ScriptEngine;
    using Utils.Json;

    public class QueryLangOperationsExtension : IAppProxyExtension
    {
        public QueryLangOperationsExtension()
        {
        }

        public void AddToServiceContainer(dynamic container)
        {
            container.Delete = new Action<JToken, string>((o, path) =>
            {
                o[ObjectHelper.DeletePropName] = true;
            });

            container.Add = new Action<JToken, dynamic>((list, paramObj) =>
           {
               list = list as JArray;
               if (list == null)
               {
                   throw new Exception("List should be typeof JArray");
               }

               paramObj = ConvertFromWhereSelect(paramObj);
               foreach (var item in paramObj)
               {
                   ((JArray)list).Add(item);
               }
           });

            container.Set = new Action<JToken, IDictionary<string, object>>((currentObject, paramDict) =>
            {
                foreach (var o in paramDict)
                {
                    var path = o.Key;
                    var paramObj = o.Value;

                    currentObject[path] = paramObj != null ? JToken.FromObject(paramObj) : null;
                }
            });
        }

        private List<dynamic> ConvertFromWhereSelect(object data)
        {
            var result = new List<dynamic>();
            IEnumerable enumerable = data as IEnumerable;
            if (enumerable == null)
            {
                return new List<dynamic>() { data };
            }

            IEnumerator en = (data as IEnumerable).GetEnumerator();
            while (en.MoveNext())
            {
                result.Add(en.Current);
            }

            return result;
        }
    }
}
