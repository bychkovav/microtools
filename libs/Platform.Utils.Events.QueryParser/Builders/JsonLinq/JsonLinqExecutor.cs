using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions;
using Platform.Utils.Events.QueryParser.Builders.Object;
using Platform.Utils.Events.QueryParser.Domain.Enums;
using Platform.Utils.Events.QueryParser.Domain.Objects;
using Platform.Utils.Events.QueryParser.Extensions;
using Platform.Utils.Events.QueryParser.Extensions.Fluent;
using Platform.Utils.Events.QueryParser.Extensions.Preprocessors;
using Platform.Utils.Events.QueryParser.Helpers;

namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq
{
    public class JsonLinqExecutor
    {
        private ExpandoObject Services;
        private IQueryable<JToken> initialDataSource;
        private IQueryable<JToken> eData;

        public static JsonLinqExecutor GetExecutor => new JsonLinqExecutor();

        private JsonLinqExecutor()
        {
        }

        private IList<SingleQuery> GetSingleQueries(IEnumerable<string> stringQueries)
        {
            var singleQueries = new List<SingleQuery>();
            var engine = new Engine();

            foreach (var stringQuery in stringQueries)
            {
                var singleQuery = engine.Parse(stringQuery);
                singleQueries.Add(singleQuery);
            }

            return singleQueries;
        }

        public IList<JToken> Run(string stringQuery, JToken dataSource, ExpandoObject services)
        {
            return Run(new[] { stringQuery }, dataSource, services);
        }

        public IList<JToken> Run(IEnumerable<string> stringQueries, JToken dataSource, ExpandoObject services)
        {
            var singleQueries = GetSingleQueries(stringQueries);

            return Run(singleQueries, dataSource, services);
        }

        public IList<JToken> Run(IEnumerable<SingleQuery> singleQueries, JToken dataSource, ExpandoObject services)
        {
            var resultingData = new[] { dataSource }.AsQueryable();

            foreach (var singleQuery in singleQueries)
            {
                var executionResult = ExecuteQuery(singleQuery, resultingData, services);

                var lastMethod = singleQuery.NodesList?.LastOrDefault()?.MethodType;

                switch (lastMethod)
                {
                    case QueryMethodType.Get:
                        resultingData = executionResult.Select(x => x.BuildEDMObject());
                        break;
                    case QueryMethodType.GetValue:
                        resultingData = executionResult;
                        break;
                    default:
                        resultingData = initialDataSource;
                        break;
                }
            }

            return resultingData.ToList();
        }

        public IQueryable<JToken> ExecuteQuery(SingleQuery singleQuery, JToken dataSource, ExpandoObject services)
        {
            var result = new[] { dataSource }.AsQueryable();

            result = ExecuteQuery(singleQuery, result, services);

            return result;
        }

        public IQueryable<JToken> ExecuteQuery(SingleQuery query, IQueryable<JToken> source, ExpandoObject services)
        {
            query = query.MakeCopy();

            // Convert Id to masterId if needed
            query.ConvertIdToMasterId();

            initialDataSource = source;
            eData = source;
            var result = initialDataSource;

            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                // BelongsTo workaround
                queryNode.BelongsToQuery = query;

                // For root node
                if (queryNode.RootType.HasValue)
                {
                    switch (queryNode.RootType)
                    {
                        case QueryRootType.Variable:
                            var variable = ((dynamic)services).Variables[queryNode.Name];

                            if (variable is JToken)
                                initialDataSource = new[] { (JToken)variable }.AsQueryable();
                            else
                                initialDataSource = ((IEnumerable<JToken>)variable).AsQueryable();
                            break;
                        case QueryRootType.Model:
                            var modelPivotData = queryNode.PivotData ?? new PivotData
                            {
                                MainValue = queryNode.Name,
                                PivotDefinition = ParserHelper.GetPivot(PivotType.Transaction)
                            };

                            var newModel = ((dynamic)services).Init(queryNode.BelongsToQuery.ParentQueryModelInfo?.MainValue ?? queryNode.Name, modelPivotData) as JToken;
                            initialDataSource = new[] { newModel }.AsQueryable();
                            break;
                        case QueryRootType.InputData:
                            initialDataSource = eData;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    result = initialDataSource;
                }

                switch (queryNode.Type)
                {
                    case QueryNodeType.Property:
                        if (queryNode.RootType.HasValue && queryNode.PivotData == null)
                            break;

                        result = ExecuteProperty(queryNode, result);

                        break;
                    case QueryNodeType.Collection:
                        queryNode.ConvertPivotToCriterias();

                        result = ExecuteCollection(queryNode, result, services);
                        break;
                    case QueryNodeType.Method:
                        var postFiltratingNodes = new List<QueryNode>();
                        var currentListNode = query.NodesList.Find(queryNode)?.Next;

                        /*
                                                while (currentListNode != null)
                                                {
                                                    if (currentListNode.Value.Type == QueryNodeType.Method)
                                                        break;

                                                    postFiltratingNodes.Add(currentListNode.Value);
                                                    currentListNode = currentListNode.Next;
                                                }

                                                if (postFiltratingNodes.Any())
                                                {
                                                    var postFiltrationQuery = SingleQuery.CreateQuery;

                                                    postFiltrationQuery.RootCollection(QueryRootType.Variable);

                                                    postFiltratingNodes.ForEach(x =>
                                                    {
                                                        query.NodesList.Remove(x);
                                                        postFiltrationQuery.NodesList.AddLast(x);
                                                    });

                                                    var where = RenderFiltratingLinq(postFiltrationQuery).Trim('.');
                                                    chunks.Add(where);
                                                }
                        */

                        result = ExecuteMethod(queryNodeItem, result, services);
                        break;
                    case QueryNodeType.MethodArgument:
                        break;
                    case QueryNodeType.Criteria:
                        break;
                    case QueryNodeType.Projection:
                        result = ExecuteProjection(queryNodeItem, result, services);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        public IQueryable<JToken> ExecuteProperty(QueryNode queryNode, IQueryable<JToken> source)
        {
            var result = source;

            result = result.Select(x => x.SelectToken(queryNode.GetJTokenName(true)));

            return result;
        }

        public IQueryable<JToken> ExecuteCollection(QueryNode queryNode, IQueryable<JToken> source, ExpandoObject services)
        {
            var result = source;

            if (queryNode.RootType.HasValue)
                result = result.SelectMany(x => x.SelectTokens(queryNode.GetJTokenName(true)));
            else
                result = result.SelectMany(x => x.SelectTokens($"{queryNode.GetJTokenName(true)}[*]"));

            Expression<Func<JToken, bool>> wherePredicate = ProcessCriteriaGroup(queryNode, result, services);

            if (wherePredicate != null)
                result = result.Where(wherePredicate);

            return result;
        }

        private Expression<Func<JToken, bool>> ProcessCriteriaGroup(QueryNode queryNode, IQueryable<JToken> source, ExpandoObject services)
        {
            Expression<Func<JToken, bool>> result = null;
            var predicates = new List<Expression<Func<JToken, bool>>>();

            foreach (var criteria in queryNode.Criterias)
            {
                Expression<Func<JToken, bool>> criteriaPredicate;
                if (criteria.Type == QueryNodeType.CriteriaGroup)
                {
                    criteriaPredicate = ProcessCriteriaGroup(criteria, source, services);
                }
                else
                {
                    // Column accessor
                    var subjectQuery = criteria.CriteriaSubjectQuery;

                    var comparator = criteria.Comparator;
                    var value = criteria.CriteriaValueQuery != null
                        ? JsonLinqExecutor.GetExecutor.ExecuteQuery(criteria.CriteriaValueQuery, source, services)
                        : criteria.CriteriaValueConstant;

                    // Default accessor
                    Func<JToken, IQueryable<JToken>> baseAccessor =
                        z => JsonLinqExecutor.GetExecutor.ExecuteQuery(subjectQuery, z, services);
                    Func<JToken, dynamic> accessor;

                    // Just query or comparison 
                    if (criteria.Comparator.HasValue)
                    {
                        accessor = z => baseAccessor(z).SingleOrDefault()?.Value<dynamic>();
                    }
                    else
                    {
                        accessor = z => baseAccessor(z).Any();
                        comparator = CriteriaComparator.Eq;
                        value = true;
                    }

                    // Get WHERE predicate
                    criteriaPredicate = WhereExpressionBuilder.GetPredicate(comparator.Value, criteria.NotModifier == true, accessor, value);
                }

                predicates.Add(criteriaPredicate);
            }

            var appender = queryNode.Appender ?? CriteriaAppendType.And;

            if (predicates.Any())
            {
                switch (appender)
                {
                    case CriteriaAppendType.And:
                        result = predicates.Aggregate((a, b) => a.And(b));
                        break;
                    case CriteriaAppendType.Or:
                        result = predicates.Aggregate((a, b) => a.Or(b));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        public IQueryable<JToken> ExecuteMethod(LinkedListNode<QueryNode> queryNodeItem, IQueryable<JToken> source, ExpandoObject services)
        {
            var queryNode = queryNodeItem.Value;
            var methodName = ParserHelper.GetMethod(queryNode.MethodType.Value);
            var result = source;

            switch (queryNode.MethodType)
            {
                #region Set

                case QueryMethodType.Set:
                    var methodContents = new Dictionary<string, object>();
                    foreach (var argument in queryNode.Arguments)
                    {
                        object value;
                        if (argument.ArgumentValueConstant != null)
                            value = argument.ArgumentValueConstant;
                        else
                            value = JsonLinqExecutor.GetExecutor.ExecuteQuery(argument.ArgumentValueQuery, eData, services).FirstOrDefault();

                        // build argument query path
                        var chunks = new List<string>();

                        for (var queryItem = argument.ArgumentSubjectQuery.NodesList.First; queryItem != null; queryItem = queryItem.Next)
                        {
                            if (queryItem.Value.Type == QueryNodeType.Property || queryItem.Value.Type == QueryNodeType.Collection)
                                chunks.Add(queryItem.Value.GetJTokenName(false));
                        }

                        methodContents.Add(string.Join(".", chunks), value);
                    }

                    result = result.DoOver(x => x.Set((ExpandoObject)services, methodContents));
                    break;

                #endregion

                #region Add

                case QueryMethodType.Add:
                    var rootInstance = eData;

                    var newQuery = queryNode.BelongsToQuery.AddSubQuery();

                    if (queryNode.Arguments.Any() &&
                        queryNode.Arguments.First().ArgumentValueQuery != null &&
                        queryNode.Arguments.First().ArgumentValueQuery.NodesList.First.Value.RootType.HasValue)
                    {
                        newQuery = queryNode.Arguments.First().ArgumentValueQuery;
                    }
                    // Prepare query to generate Set methods
                    else
                    {
                        var pivotValues = queryNode.PivotData.GetPivotValues();
                        JToken rootSource;

                        switch (queryNode.PivotData.PivotDefinition.Type)
                        {
                            case PivotType.MasterData:
                                // TODO: Here may be crash if "capacity" is passed by argument instead of pivot
                                var capacityPair = pivotValues.Skip(1).FirstOrDefault().Value ?? new List<string>();
                                var capacity = ((List<string>)capacityPair).FirstOrDefault();
                                var id = queryNode.Arguments.FirstOrDefault()?.ArgumentValueConstant;

                                rootSource = (JToken)((dynamic)services).GetMd(id.ToString(), capacity);
                                rootInstance = new[] { rootSource }.AsQueryable();
                                newQuery.RootProperty(QueryRootType.InputData);
                                break;

                            default:
                                newQuery.RootProperty(QueryRootType.Model, m => m.Name = queryNode.PivotData.MainValue)
                                    .SetPivot(queryNode.PivotData.PivotDefinition.Type, queryNode.PivotData.MainValue, queryNode.PivotData.SecondaryValues.ToArray());
                                break;
                        }

                        if (queryNode.Arguments.Any())
                        {
                            var simpleSetArguments =
                                queryNode.Arguments.Where(o => o.ArgumentSubjectQuery != null).ToList();
                            if (simpleSetArguments.Any())
                            {
                                newQuery.NodesList.Last.MethodSet(x => x.Arguments.AddRange(simpleSetArguments));
                            }

                            var querySetArguments =
                                queryNode.Arguments.Where(
                                    o => o.ArgumentValueQuery != null && o.ArgumentSubjectQuery == null).ToList();
                            foreach (var querySetArgument in querySetArguments)
                            {
                                foreach (var node in querySetArgument.ArgumentValueQuery.NodesList)
                                {
                                    newQuery.NodesList.AddLast(node);
                                }
                            }
                        }
                    }

                    var addValue = JsonLinqExecutor.GetExecutor.ExecuteQuery(newQuery, rootInstance, services).ToList();

                    result =
                        result.DoMany(x =>
                            x.Select(x1 => x1.SelectToken(queryNode.GetJTokenName(true)))
                                .DoOver(x1 =>
                                    x1.Add((ExpandoObject)services, addValue)));

                    break;

                #endregion

                case QueryMethodType.Get:
                    break;
                case QueryMethodType.Delete:
                    result = result.DoOver(x => x.Delete((ExpandoObject)services, string.Join(".", queryNodeItem.GetPathFromRoot(false))));
                    break;
                case QueryMethodType.ToMd:
                    result = result.DoOver(x => x.ToMd(queryNode.Arguments.First().ArgumentValueConstant.ToString()));
                    break;
                case QueryMethodType.ToT:
                    result = result.DoOver(x => x.ToT());
                    break;
                case QueryMethodType.ToLocal:
                    //                    result = result.DoOver(x => x.ToLocal(queryNode.Arguments.First().ArgumentValueConstant));
                    break;
                case QueryMethodType.Take:
                    result = result.Take(Convert.ToInt32(queryNode.Arguments.First().ArgumentValueConstant));
                    break;
                case QueryMethodType.Skip:
                    result = result.Skip(Convert.ToInt32(queryNode.Arguments.First().ArgumentValueConstant));
                    break;
                case QueryMethodType.OrderBy:

                    var orderBy = result.OrderBy(x => true);

                    foreach (var argument in queryNode.Arguments)
                    {
                        var newArgumentQuery = argument.ArgumentValueQuery.MakeCopy();
                        if (!newArgumentQuery.NodesList.First.Value.RootType.HasValue)
                            newArgumentQuery.RootProperty(QueryRootType.InputData);

                        var orderByLambda =
                            new Func<JToken, IQueryable<JToken>>(x => JsonLinqExecutor.GetExecutor.ExecuteQuery(newArgumentQuery, x, services));

                        orderBy = orderBy.ThenBy(x => orderByLambda(x).SingleOrDefault());

                    }

                    result = orderBy;
                    break;
                case null:
                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public IQueryable<JToken> ExecuteProjection(LinkedListNode<QueryNode> queryNodeItem, IQueryable<JToken> source, ExpandoObject services)
        {
            var result = source;
            var queryNode = queryNodeItem.Value;

            var properties = new List<Func<JToken, JProperty>>();

            foreach (var keyValuePair in queryNode.Projections)
            {
                var projectionValue = keyValuePair.Value;
                var projectionName = projectionValue.NodesList.First.Value.GetJTokenName(false);

                projectionValue.RootProperty(QueryRootType.InputData);

                var jprop = new Func<JToken, JProperty>(x => new JProperty(projectionName, JsonLinqExecutor.GetExecutor.ExecuteQuery(projectionValue, x, services).SingleOrDefault()));
                properties.Add(jprop);
            }

            var jobj = new Func<JToken, JObject>(x =>
            {
                var props = properties.Select(p => p(x)).ToArray();
                return new JObject(props);
            });

            result = result.Select(x => jobj(x));

            return result;
        }
    }
}
