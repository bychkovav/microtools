using Platform.Utils.Events.QueryParser.Extensions;

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

    public class FilterParser
    {
        private Dictionary<string, Action<JProperty>> operators;

        private SingleQuery singleQuery = null;

        private Stack<ProcessingObjectType> processingObjectTypeStack { get; } = new Stack<ProcessingObjectType>();

        private Stack<QueryNode> queryNodesStack { get; } = new Stack<QueryNode>();

        private QueryNode CurrentQueryNode => queryNodesStack.Any() ? queryNodesStack.Peek() : null;

        private Stack<List<string>> pathFromLastCollectionStack = new Stack<List<string>>();

        private ProcessingObjectType? CurrentProcessingObjectType => processingObjectTypeStack.Any() ? processingObjectTypeStack.Peek() : (ProcessingObjectType?)null;
        private List<string> CurrentPathFromLastCollection => this.pathFromLastCollectionStack.Any() ? this.pathFromLastCollectionStack.Peek() : new List<string>();

        private Dictionary<string, KeyValuePair<CriteriaComparator, bool>> comparatorMappings = new Dictionary<string, KeyValuePair<CriteriaComparator, bool>>
        {
            ["$eq"]     = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Eq, false),
            ["$ne"]     = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.NotEq, false),
            ["$lt"]     = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Lt, false),
            ["$lte"]    = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Le, false),
            ["$gt"]     = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Gt, false),
            ["$gte"]    = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Ge, false),
            ["$btw"]    = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Between, false),
            ["$nbtw"]   = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Between, true),
            ["$in"]     = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.In, false),
            ["$nin"]    = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.In, true),
            ["$cnts"]   = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Like, false),
            ["$ncnts"]  = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.Like, true),
            ["$begw"]  = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.BeginsWith, false),
            ["$nbegw"]  = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.BeginsWith, true),
            ["$endw"]  = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.EndsWith, false),
            ["$nendw"]  = new KeyValuePair<CriteriaComparator, bool>(CriteriaComparator.EndsWith, true),
        }; 

        public FilterParser()
        {
            this.operators = new Dictionary<string, Action<JProperty>>
            {
                ["$skip"] = SkipVisitor,
                ["$take"] = TakeVisitor,
                ["$orderby"] = OrderByVisitor,

                ["$and"] = AndVisitor,
                ["$or"] = OrVisitor,

                ["$eq"] = ComparatorVisitor,
                ["$ne"] = ComparatorVisitor,
                ["$lt"] = ComparatorVisitor,
                ["$lte"] = ComparatorVisitor,
                ["$gt"] = ComparatorVisitor,
                ["$gte"] = ComparatorVisitor,
                ["$btw"] = ComparatorVisitor,
                ["$nbtw"] = ComparatorVisitor,
                ["$in"] = ComparatorVisitor,
                ["$nin"] = ComparatorVisitor,
                ["$cnts"] = ComparatorVisitor,
                ["$ncnts"] = ComparatorVisitor,
                ["$begw"] = ComparatorVisitor,
                ["$nbegw"] = ComparatorVisitor,
                ["$endw"] = ComparatorVisitor,
                ["$nendw"] = ComparatorVisitor,
            };
        }

        private List<JProperty> GetPathUp(JProperty property)
        {
            var result = new List<JProperty>();
            var stopLevels = new List<string> {"$and", "$or"};

            var parent = property.GetParentProperty();
            if (!stopLevels.Contains(property.Name) & parent != null)
            {
//                var pivotData = GetPivotType(property);
//                if (pivotData == null || !collectionNodes.Contains(pivotData.PivotDefinition.Type))
                {
                    var parents = GetPathUp(parent);
                    result.AddRange(parents);
                }

                if (!property.Name.StartsWith("$"))
                    result.Add(property);
            }

            return result;
        }

        public SingleQuery Parse(JObject filter)
        {
            var root = filter.Properties().Single(x => !x.Name.StartsWith("$"));
            RootVisitor(root);

            var rootOperations = filter.Properties().Where(x => x.Name.StartsWith("$")).ToList();
            foreach (var rootOperation in rootOperations)
            {
                Action<JProperty> visitor;
                var propertyName = rootOperation.Name.ToLower();
                this.operators.TryGetValue(propertyName, out visitor);

                visitor?.Invoke(rootOperation);
            }

            return this.singleQuery;
        }

        public void RootVisitor(JProperty property)
        {
            this.singleQuery = SingleQuery.CreateQuery.RootCollection(QueryRootType.InputData)
                .SetPivot(PivotType.Transaction, property.Name);

            processingObjectTypeStack.Push(ProcessingObjectType.Node);
            queryNodesStack.Push(this.singleQuery.NodesList.Last.Value);

            var rootObject = property.Value as JObject;
            if (rootObject == null)
                throw new Exception($"{property.Name} is not JObject");

            ProcessJObject(rootObject);

            processingObjectTypeStack.Pop();
            queryNodesStack.Pop();
        }

        public void ProcessJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                var propertyName = property.Name.ToLower();

                Action<JProperty> visitor;

                this.operators.TryGetValue(propertyName, out visitor);

                (visitor ?? PropertyVisitor).Invoke(property);
            }
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
            if (propertyObject == null)
                throw new Exception($"{property.Name} is not JObject");

            ProcessJObject(propertyObject);
        }

        public void SkipVisitor(JProperty property)
        {
            var limit = Convert.ToInt32(property.Value);

            this.singleQuery.AddMethod(QueryMethodType.Skip)
                .AddArgument(x => x.ArgumentValueConstant = limit);
        }

        public void TakeVisitor(JProperty property)
        {
            var limit = Convert.ToInt32(property.Value);

            this.singleQuery.AddMethod(QueryMethodType.Take)
                .AddArgument(x => x.ArgumentValueConstant = limit);
        }

        public void OrderByVisitor(JProperty property)
        {
            var array = property.Value as JArray;
            if (array == null)
                throw new Exception($"{"OrderBy"} is not JArray");

            this.singleQuery.AddMethod(QueryMethodType.OrderBy);

            foreach (var token in array)
            {
                var orderByQueryArgument = SingleQuery.CreateQuery;
                this.singleQuery.AddArgument(x => x.ArgumentValueQuery = orderByQueryArgument);

                var path = token["path"] as JArray;
                if (path == null)
                    throw new Exception($"{"OrderBy.path"} is not JArray");

                foreach (var pathNode in path)
                {
                    var nodeName = pathNode.ToString();
                    var queryNode = GetQueryNode(nodeName);
                    orderByQueryArgument.AddNode(queryNode);
                }
            }

        }

        #region Grouping

        private int currentGroup = 0;

        private void AndOrVisitor(JProperty property, CriteriaAppendType appendType)
        {
            var criterias = property.Value as JArray;
            if (criterias == null)
                throw new Exception($"{property.Name} is not JArray");

            var criteriaGroup = CurrentQueryNode.AddCriteriaGroup(appendType);

            processingObjectTypeStack.Push(ProcessingObjectType.ConditionGroup);
            queryNodesStack.Push(criteriaGroup);

            foreach (var criteria in criterias)
            {
                var criteriaObject = criteria as JObject;
                if (criteriaObject == null)
                    throw new Exception($"{"Criteria"} is not JObject");

                ProcessJObject(criteriaObject);
            }

            processingObjectTypeStack.Pop();
            queryNodesStack.Pop();
        }

        public void AndVisitor(JProperty property)
        {
            AndOrVisitor(property, CriteriaAppendType.And);
        }

        public void OrVisitor(JProperty property)
        {
            AndOrVisitor(property, CriteriaAppendType.Or);
        }

        #endregion

        private Dictionary<int, CriteriaAppendType> groups = new Dictionary<int, CriteriaAppendType>();

        private void ComparatorVisitor(JProperty property)
        {
            // ConditionGroup absent workaround
            if (CurrentProcessingObjectType != ProcessingObjectType.ConditionGroup)
            {
                var criteriaGroup = CurrentQueryNode.AddCriteriaGroup(CriteriaAppendType.And);

                processingObjectTypeStack.Push(ProcessingObjectType.ConditionGroup);
                queryNodesStack.Push(criteriaGroup);

                ComparatorVisitor(property);

                processingObjectTypeStack.Pop();
                queryNodesStack.Pop();

                return;
            }

            var path = GetPathUp(property);
            var subjectQuery = SingleQuery.CreateQuery;
            var query = subjectQuery;
            QueryNode queryNode = null;
            var isValuelessCriteria = false;
            var isPrevNodeCollection = false;

            foreach (var pathNode in path)
            {
                queryNode = GetQueryNode(pathNode);
                if (queryNode.PivotData == null || !queryNode.PivotData.PivotDefinition.IsCollection)
                {
                    if (isValuelessCriteria && pathNode == path.Last())
                        continue;
                    query.AddNode(queryNode);
                    isPrevNodeCollection = false;
                }
                else
                {
                    isValuelessCriteria = true;
                    if (!isPrevNodeCollection)
                    {
                        query.AddNode(queryNode);
                    }
                    else
                    {
                        var subQuery = SingleQuery.CreateQuery;
                        subQuery.AddNode(queryNode);
                        query.AddCriteriaGroup(CriteriaAppendType.And).AddCriteria(subQuery);
                        query = subQuery;
                    }
                    isPrevNodeCollection = true;
                }
            }


            var comparatorMapping = this.comparatorMappings[property.Name.ToLower()];
            var comparator = comparatorMapping.Key;
            var notModifier = comparatorMapping.Value;

            var value = GetValue(property.Value);


            if (isValuelessCriteria)
            {
                var newSubject = SingleQuery.CreateQuery.AddNode(queryNode);
                query.AddCriteriaGroup(CriteriaAppendType.And).AddCriteria(newSubject, comparator, value, x => x.NotModifier = notModifier);
                CurrentQueryNode.AddCriteria(subjectQuery);
            }
            else
                CurrentQueryNode.AddCriteria(subjectQuery, comparator, value, x => x.NotModifier = notModifier);
        }

        private object GetValue(JToken jToken)
        {
            object result = null;

            switch (jToken.Type)
            {
                case JTokenType.Array:
                    result = new List<object>();
                    var jArray = jToken.Value<JArray>();
                    foreach (var item in jArray)
                        (result as List<object>).Add(GetValue(item));

                    break;
                case JTokenType.Integer:
                    result = jToken.Value<int>();
                    break;
                case JTokenType.Float:
                    result = jToken.Value<float>();
                    break;
                case JTokenType.String:
                    result = jToken.Value<string>();
                    break;
                case JTokenType.Boolean:
                    result = jToken.Value<bool>();
                    break;
                case JTokenType.Null:
                    break;
                case JTokenType.Date:
                    result = jToken.Value<DateTime>();
                    break;
                case JTokenType.Guid:
                    result = jToken.Value<Guid>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}