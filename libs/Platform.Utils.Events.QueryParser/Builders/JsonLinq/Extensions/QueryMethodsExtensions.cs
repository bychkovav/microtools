using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Platform.Utils.Json;

namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions
{
    public static class QueryMethodsExtensions
    {

        public static JToken ToMd(this JToken self, string capacity)
        {
            var res = new JObject
            {
                [ObjectHelper.ObjectCodePropName] = self.SelectToken(ObjectHelper.ObjectCodePropName),
                [ObjectHelper.CapacityPropName] = capacity,
                [ObjectHelper.MasterIdPropName] = self.SelectToken(ObjectHelper.MasterIdPropName)
            };

            var propsToCopy = ((JObject)self).Properties().Where(x => x.Name.Contains("vx.") || x.Name.Contains("ae."));
            foreach (var propToCopy in propsToCopy)
            {
                res[propToCopy.Name] = propToCopy.Value;
            }

            return res;
        }

        public static JToken ToT(this JToken self)
        {
            var res = new JObject
            {
                [ObjectHelper.MasterIdPropName] = self.SelectToken(ObjectHelper.MasterIdPropName),
                [ObjectHelper.ObjectCodePropName] = self.SelectToken(ObjectHelper.ObjectCodePropName),
                [ObjectHelper.CodePropName] = self.SelectToken(ObjectHelper.CapacityPropName),
            };

            var propsToCopy = ((JObject)self).Properties().Where(x => x.Name.Contains("vx.") || x.Name.Contains("ae."));
            foreach (var propToCopy in propsToCopy)
            {
                res[propToCopy.Name] = propToCopy.Value;
            }

            return res;
        }

        public static void Delete(this JToken self, dynamic services, string path)
        {
            services.Delete(self, path);
        }

        public static JToken Add(this JToken obj, dynamic services, object paramObj)
        {
            if (obj == null)
            {
                return null;
            }
            services.Add(obj, paramObj);
            return obj;
        }

        public static object Get(this object obj, dynamic services)
        {
            return services.Get(obj);
        }

        public static JToken Set(this JToken obj, dynamic services, IDictionary<string, object> objectDict)
        {
            services.Set(obj, objectDict);
            return obj;
        }
    }
}
