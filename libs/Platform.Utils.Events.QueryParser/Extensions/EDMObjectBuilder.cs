using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Platform.Utils.Json;

namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;

    public static class EDMObjectBuilder
    {
        public static JToken BuildEDMObject(this JToken jToken)
        {
            var root = new JObject();
            var token = jToken.GetParentProperty();

            var props = new List<JProperty>
            {
                token
            };

            token = token.GetParentProperty();
            
            while (token != null)
            {
                var newValue = token.GetEDM();
                props.Add(newValue);
                token = token.GetParentProperty();

                //newObject.Add(newValue);
                //newObject = new JObject {newObject};

            }

            props.Reverse();
            var jtmp = root;
            foreach (var prop in props)
            {
                jtmp.Add(prop);
                jtmp = prop.Value as JObject;
            }

            return root;
        }

        private static JProperty GetEDM(this JProperty jProperty)
        {
            var value = jProperty.Value;
            var propertyName = jProperty.Name;
            var jObject = (value is JArray ? value[0] : value) as JObject;

            var pivotData = jProperty.GetPivotData();
            var newValue = new JObject();

            foreach (var commonProperty in pivotData.PivotDefinition.CommonProperties)
            {
                JToken x;
                if (jObject.TryGetValue(commonProperty, out x))
                {
                    newValue.Add(commonProperty, x);
                }
            }


            var newToken = new JProperty(propertyName, newValue);

            return newToken;
        }
    }
}