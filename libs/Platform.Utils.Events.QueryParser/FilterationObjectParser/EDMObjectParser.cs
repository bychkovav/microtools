using Platform.Utils.Events.QueryParser.Extensions;
using Platform.Utils.Json;

namespace Platform.Utils.Events.QueryParser.FilterationObjectParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Extensions.Fluent;
    using Helpers;
    using Listeners;
    using Newtonsoft.Json.Linq;

    public class EDMObjectParser
    {
        private Dictionary<string, Action<JProperty>> operators;

        private SingleQuery singleQuery = null;

        public SingleQuery Parse(JObject edm)
        {
            var root = edm.Properties().First();

            RootVisitor(root);

            singleQuery.AddMethod(QueryMethodType.Get);

            return this.singleQuery;
        }

        public void RootVisitor(JProperty property)
        {
            this.singleQuery = SingleQuery.CreateQuery.RootCollection(QueryRootType.InputData)
                .SetPivot(PivotType.Transaction, property.Name);

            var rootObject = property.Value as JObject;
            if (rootObject == null)
                throw new Exception($"{property.Name} is not JObject");

            ProcessJObject(rootObject);
        }

        public void ProcessJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                var propertyName = property.Name;

                switch (propertyName)
                {
                    case ObjectHelper.IdPropName:
                    case ObjectHelper.MasterIdPropName:
                        IdVisitor(property, propertyName);
                        break;
                    default:
                        PropertyVisitor(property);
                        break;
                }
            }
        }

        public void ProcessJArray(JArray jArray)
        {
            var jObject = jArray.FirstOrDefault() as JObject;
            if (jObject == null)
                return;

            ProcessJObject(jObject);
        }

        private QueryNode GetQueryNode(JProperty property)
        {
            var propertyName = property.Name;
            return GetQueryNode(propertyName);
        }

        private QueryNode GetQueryNode(string propertyName)
        {
            var query = SingleQuery.CreateQuery;
            var pivotData = ParserHelper.GetPivotData(propertyName);
            if (pivotData?.PivotDefinition != null)
            {
                var fluentDelegate = ParserHelper.GetFluentDelegate(pivotData.PivotDefinition.Type);
                fluentDelegate(query, pivotData.ToString(), null);

                // todo: add capacity/code/etc support
                query.SetPivot(pivotData.PivotDefinition.Type, pivotData.MainValue, null);
            }
            else
            {
                query.AddProperty(propertyName);
            }

            return query.NodesList.First.Value;
        }

        private QueryNode AddPropertyNode(SingleQuery query, JProperty property)
        {
            var queryNode = GetQueryNode(property);
            query.AddNode(queryNode);
            return queryNode;
        }

        public void PropertyVisitor(JProperty property)
        {
            var propertyObject = property.Value as JObject;
            if (propertyObject != null)
            {
                AddPropertyNode(singleQuery, property);

                ProcessJObject(propertyObject);

                return;
            }

            var propertyArray = property.Value as JArray;
            if (propertyArray != null)
            {
                AddPropertyNode(singleQuery, property);

                ProcessJArray(propertyArray);

                return;
            }
        }

        public void IdVisitor(JProperty property, string idName)
        {
            var id = Guid.Parse(property.Value.ToString());

            this.singleQuery
                .AddCriteriaGroup(CriteriaAppendType.And)
                .AddCriteria(SingleQuery.CreateQuery.AddProperty(idName), CriteriaComparator.Eq, id);
        }
    }
}