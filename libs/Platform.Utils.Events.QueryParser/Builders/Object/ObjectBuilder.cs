namespace Platform.Utils.Events.QueryParser.Builders.Object
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Extensions;
    using Extensions.Preprocessors;
    using Json;
    using JsonLinq.Extensions;
    using Newtonsoft.Json.Linq;

    public class ObjectBuilder
    {
        private dynamic jObject = new ExpandoObject();
        private dynamic currentObject = new ExpandoObject();

        public JObject Build(string queryString)
        {
            var query = new Engine().Parse(queryString);
            var result = Build(query);

            return result;
        }

        public JObject Build(SingleQuery query)
        {
            return Build(new List<SingleQuery> { query });
        }

        public JObject Build(List<SingleQuery> queryList)
        {
            var obj = new ExpandoObject();
            
            foreach (var query in queryList)
            {
                var data = new ObjectBuilder().RenderQuery(query);
                obj = Merge(obj, data);
            }

            var result = JObject.FromObject(obj);

            return result;
        }

        public dynamic RenderQuery(SingleQuery query, dynamic value = null)
        {
            query = query.MakeCopy();
            query.PivotsToCriterias(true);

            this.currentObject = this.jObject;
            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                switch (queryNode.Type)
                {
                    case QueryNodeType.Property:
                    {
                        if (queryNodeItem == query.NodesList.Last && value != null)
                            RenderProperty(queryNode, value);
                        else
                            RenderProperty(queryNode);

                        break;
                    }
                    case QueryNodeType.Collection:
                    {
                        RenderCollection(queryNode);

                        break;
                    }
                    case QueryNodeType.Method:
                        RenderMethod(queryNode);
                        break;
                    case QueryNodeType.MethodArgument:
                        break;
                    case QueryNodeType.Criteria:
                        break;
                    case QueryNodeType.Projection:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return this.jObject;
        }

        public dynamic RenderProperty(QueryNode queryNode, dynamic value = null)
        {
            var name = queryNode.GetJTokenName(escape: false);

            value = value ?? new ExpandoObject();

            AddProperty(name, value);

            this.currentObject = value;

            return value;
        }

        public dynamic RenderCollection(QueryNode queryNode)
        {
            var name = queryNode.GetJTokenName(escape: false);

            dynamic value = new ExpandoObject();

            if (queryNode.RootType.HasValue)
                AddProperty(name, value);
            else
                AddProperty(name, new [] { value });

            this.currentObject = value;

            var id = queryNode.GetQueryNodeId();
            if (id.HasValue)
                AddProperty(ObjectHelper.IdPropName, id);

            var objectCode = queryNode.PivotData?.MainValue;
            if (!string.IsNullOrEmpty(objectCode))
                AddProperty(queryNode.PivotData.PivotDefinition.MainProperty, objectCode);

            return value;
        }


        public dynamic RenderMethod(QueryNode queryNode)
        {
            switch (queryNode.MethodType)
            {
                case QueryMethodType.Set:
                {
                    foreach (var argument in queryNode.Arguments)
                    {
                        var newObject = new ObjectBuilder().RenderQuery(argument.ArgumentSubjectQuery, argument.ArgumentValueConstant);

                        this.currentObject = Merge(this.currentObject, newObject);

                    }
                }
                    break;
                case QueryMethodType.Add:
                {
                    var newObject = new ExpandoObject();

                    AddProperty(queryNode.GetJTokenName(false), new List<object> { newObject });

                    foreach (var argument in queryNode.Arguments)
                    {
                        var argumentType = argument.GetArgumentType();

                        if (argumentType == MethodArgumentType.KeyValueConstant)
                        {
                            var newProperty = new ObjectBuilder().RenderQuery(argument.ArgumentSubjectQuery, argument.ArgumentValueConstant);
                            Merge(newObject, newProperty);
                        }
                    }
                }
                        break;
                case QueryMethodType.Get:
                    break;
                case QueryMethodType.Delete:
                    break;
                case QueryMethodType.ToMd:
                    break;
                case QueryMethodType.ToT:
                    break;
                case QueryMethodType.ToLocal:
                    break;
                case QueryMethodType.Take:
                    break;
                case QueryMethodType.Skip:
                    break;
                case QueryMethodType.OrderBy:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var name = queryNode.GetJTokenName(escape: false);


            return this.currentObject;
        }


        private void AddProperty(string name, dynamic value)
        {
            (this.currentObject as IDictionary<string, object>)[name] = value;
        }

        public dynamic Merge(object itemTo, object itemFrom)
        {

            var dictionaryTo = itemTo as IDictionary<string, object>;
            var dictionaryFrom = itemFrom as IDictionary<string, object>;
            if (dictionaryTo == null || dictionaryFrom == null)
            {
                var listTo = itemTo as IEnumerable<object>;
                var listFrom = itemFrom as IEnumerable<object>;

                if (listTo == null)
                    return itemTo;

                foreach (var o in listFrom)
                {
                    var exist =
                        listTo.FirstOrDefault(
                            x => (x as IDictionary<string, object>)?.ContainsKey(ObjectHelper.IdPropName) == true &&
                                 (o as IDictionary<string, object>)?.ContainsKey(ObjectHelper.IdPropName) == true &&
                                 (x as IDictionary<string, object>)[ObjectHelper.IdPropName].Equals((o as IDictionary<string, object>)[ObjectHelper.IdPropName]));
                    if (exist != null)
                        Merge(exist, o);
                    else
                        listTo = listTo.Concat(new []{ o });
                }
                // listTo = listTo.

                return listTo;
            }
            else
            {
                foreach (var keyValuePair in dictionaryFrom)
                {
                    if (dictionaryTo.ContainsKey(keyValuePair.Key))
                        dictionaryTo[keyValuePair.Key] = Merge(dictionaryTo[keyValuePair.Key], keyValuePair.Value);
                    else
                        dictionaryTo.Add(keyValuePair);
                }
            }

            return dictionaryTo;
        }
    }
}
