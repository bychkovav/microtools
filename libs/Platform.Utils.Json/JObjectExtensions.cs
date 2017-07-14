using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Utils.Json
{
    using Newtonsoft.Json.Linq;

    public static class JObjectExtensions
    {
        public static JToken Rename(this JToken json, Dictionary<string, string> map)
        {
            return Rename(json, name => map.ContainsKey(name) ? map[name] : name);
        }

        public static JToken Rename(this JToken json, Func<string, string> map)
        {
            var prop = json as JProperty;
            if (prop != null)
            {
                return new JProperty(map(prop.Name), Rename(prop.Value, map));
            }

            var arr = json as JArray;
            if (arr != null)
            {
                var cont = arr.Select(el => Rename(el, map));
                return new JArray(cont);
            }

            var o = json as JObject;
            if (o != null)
            {
                var cont = o.Properties().Select(el => Rename(el, map));
                return new JObject(cont);
            }

            return json;
        }

        public static JToken FromCamelCaseToDots(this JToken json)
        {
            return json.Rename(n =>
            {
                if (n.StartsWith("ae"))
                {
                    return $"ae.{n.Substring(2, 1).ToLowerInvariant()}{n.Substring(3)}";
                }

                if (n.StartsWith("vx"))
                {
                    return $"vx.{n.Substring(2, 1).ToLowerInvariant()}{n.Substring(3)}";
                }

                if (n.StartsWith("md"))
                {
                    return $"md.{n.Substring(2, 1).ToLowerInvariant()}{n.Substring(3)}";
                }

                if (n.StartsWith("hd"))
                {
                    return $"hd.{n.Substring(2, 1).ToLowerInvariant()}{n.Substring(3)}";
                }

                return n;
            });
        }

        public static JToken FromDotsToCamelCase(this JToken json)
        {
            return json.Rename(n =>
            {
                if (n.StartsWith("ae."))
                {
                    return $"ae{n.Substring(3, 1).ToUpperInvariant()}{n.Substring(4)}";
                }

                if (n.StartsWith("vx."))
                {
                    return $"vx{n.Substring(3, 1).ToUpperInvariant()}{n.Substring(4)}";
                }

                if (n.StartsWith("md."))
                {
                    return $"md{n.Substring(3, 1).ToUpperInvariant()}{n.Substring(4)}";
                }

                if (n.StartsWith("hd."))
                {
                    return $"hd{n.Substring(3, 1).ToUpperInvariant()}{n.Substring(4)}";
                }

                return n;
            });
        }

        public static void Traverse(this JToken node, Action<JObject> action)
        {
            switch (node.Type)
            {
                case JTokenType.Object:
                    action((JObject)node);

                    foreach (JProperty child in node.Children<JProperty>())
                    {
                        Traverse(child.Value, action);
                    }
                    break;
                case JTokenType.Array:
                    foreach (JToken child in node.Children())
                    {
                        Traverse(child, action);
                    }
                    break;
            }
        }

        public static JToken ApplyPatch(this JToken source, JToken operation)
        {
            var ops = FindOperations(operation);
            foreach (var op in ops)
            {
                ApplyItemPatch(source, op);
            }

            return source;
        }

        public static Stack<JToken> GetWayFromRoot(this JToken dest, Stack<JToken> reversedWay = null)
        {
            if (reversedWay == null)
            {
                reversedWay = new Stack<JToken>();
            }

            if (dest.Parent != null)
            {
                if (dest is JProperty)
                {
                    reversedWay.Push(dest);
                }

                GetWayFromRoot(dest.Parent, reversedWay);
            }

            return reversedWay;
        }

        public static Stack<JToken> GetWayFromLeaf(this JToken dest, Stack<JToken> reversedWay = null)
        {
            var c = GetWayFromRoot(dest);

            Stack<JToken> reversed = new Stack<JToken>();

            while (c.Count != 0)
                reversed.Push(c.Pop());

            return reversed;
        }

        public static IList<JToken> FindOperations(this JToken source)
        {
            IList<JToken> result = new List<JToken>();

            var children = source.Children<JProperty>();

            if (children.Any(x => x.Name.StartsWith("$")))
            {
                result.Add(source);
            }

            foreach (var prop in children.Where(x => !x.Name.StartsWith("$")))
            {
                var val = prop.Value;
                if (val is JObject)
                {
                    var nested = FindOperations(val);
                    foreach (var n in nested)
                    {
                        result.Add(n);
                    }
                }
                else if (val is JArray)
                {
                    foreach (var child in val as JArray)
                    {
                        var nested = FindOperations(child);
                        foreach (var n in nested)
                        {
                            result.Add(n);
                        }
                    }
                }
            }

            return result;
        }

        private static void ApplyItemPatch(JToken s, JToken o)
        {
            var operation = o.SelectToken(ObjectHelper.OperationPropName);
            if (operation == null)
            {
                throw new Exception("Operations is not set");
            }

            var st = GetWayFromRoot(o.Parent);
            switch (operation.Value<string>())
            {
                case ObjectHelper.AddOperation:

                    //var newOne = st.Pop();
                    var addLeaf = GetLeaf(s, st) as JArray;
                    if (addLeaf == null)
                    {
                        throw new Exception("Container should be type of array");
                    }
                    addLeaf.Add(o);
                    break;
                case ObjectHelper.SetOperation:
                case ObjectHelper.InitiateOperation:
                    var setLeaf = GetLeaf(s, st);
                    foreach (var child in o.Children<JProperty>().Where(x => !x.Name.StartsWith("$")))
                    {
                        setLeaf[child.Name] = child.Value;
                    }
                    break;
                case ObjectHelper.DeleteOperation:
                    var deleteLeaf = GetLeaf(s, st);
                    deleteLeaf[ObjectHelper.DeletePropName] = true;
                    break;
                default:
                    throw new Exception("No such type of operation");
            }
        }



        private static JToken GetLeaf(JToken s, Stack<JToken> wayFromRoot)
        {
            var currentSource = s;
            while (wayFromRoot.Count > 0)
            {
                var currentDest = wayFromRoot.Pop();

                var jProperty = currentDest as JProperty;
                if (jProperty != null)
                {
                    currentSource = currentSource.SelectToken(jProperty.Name);
                    if (jProperty.Value is JArray)
                    {
                        currentSource = currentSource as JArray;
                        if (currentSource == null)
                        {
                            throw new Exception("Difference between state and edm object structure");
                        }

                        var addLeaf = ((JArray)jProperty.Value).FirstOrDefault();
                        if (addLeaf == null)
                        {
                            throw new Exception("Add leaf is null");
                        }

                        var destId = addLeaf[ObjectHelper.IdPropName];

                        var sourceById = currentSource.FirstOrDefault(x => x[ObjectHelper.IdPropName].Value<string>() == destId.Value<string>());
                        if (sourceById == null)
                        {
                            throw new Exception("Difference between state and edm object structure");
                        }

                        currentSource = sourceById;
                    }
                }
            }

            return currentSource;
        }
    }
}
