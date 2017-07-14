namespace Platform.Utils.Events.QueryParser.Builders.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Extensions;
    using JsonLinq.Extensions;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using QueryParser.Extensions;
    using QueryParser.Extensions.Fluent;
    using QueryParser.Extensions.Preprocessors;

    public class MongoFilterBuilder<T>
    {
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;

        public IFindFluent<T, T> Build(string queryString, IMongoCollection<T> collection)
        {
            var parserEngine = new Engine();
            var query = parserEngine.Parse(queryString);

            var result = RenderQuery(query, collection);

            return result;
        }

        public IFindFluent<T, T> RenderQuery(SingleQuery query, IMongoCollection<T> collection)
        {
            var filter = RenderQueryFilter(query);

            var findFluent = collection.Find(filter);
            var result = RenderQueryMethods(query, findFluent);


            return result;
        }

        public FilterDefinition<T> RenderQueryFilter(SingleQuery query)
        {
            query = query.MakeCopy();

            // Convert Id to masterId if needed
            query.ConvertIdToMasterId();

            var filters = new List<FilterDefinition<T>>();

            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                if (queryNode.Type != QueryNodeType.Collection)
                    continue;

                var collectionFilter = RenderCollection(queryNodeItem);
                filters.Add(collectionFilter);
            }

            var resultingFilter = filters.Any() ? filters.Aggregate((a, b) => a & b) : this.filterBuilder.Empty;

            return resultingFilter;
        }

        public IFindFluent<T, T> RenderQueryMethods(SingleQuery query, IFindFluent<T, T> findFluent)
        {
            query = query.MakeCopy();

            var result = findFluent;

            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                if (queryNode.Type != QueryNodeType.Method)
                    continue;

                result = RenderMethod(queryNodeItem, result);
            }

            return result;
        }

        public FilterDefinition<T> RenderCollection(LinkedListNode<QueryNode> queryNodeItem)
        {
            var queryNode = queryNodeItem.Value;

            queryNode.ConvertPivotToCriterias();

            var pathToCollection = new List<string>(queryNodeItem.GetPathFromRoot(includeRoot: true));
            // TODO: Find better way
            pathToCollection = pathToCollection.ConvertForMongo();

            var resultingFilter = RenderCriteriaGroup(queryNode, pathToCollection);

            return resultingFilter;
        }

        public FilterDefinition<T> RenderCriteriaGroup(QueryNode queryNode, List<string> fullSubjectPath)
        {
            FilterDefinition<T> filter = null;

            var filters = new List<FilterDefinition<T>>();

            foreach (var criteria in queryNode.Criterias)
            {
                FilterDefinition<T> criteriaFilter;

                if (criteria.Type == QueryNodeType.CriteriaGroup)
                    criteriaFilter = RenderCriteriaGroup(criteria, fullSubjectPath);
                else
                {
                    var comparator = criteria.Comparator;
                    // "subject = value" type criteria
                    if (comparator.HasValue)
                    {
                        var subject = criteria.CriteriaSubjectQuery.NodesList.Last.GetPathFromRoot().ConvertForMongo();
                        var pathToNode = new List<string>(fullSubjectPath);
                        pathToNode.AddRange(subject);
                        criteriaFilter = RenderCriteria(criteria, pathToNode);
                    }
                    // "subject()" type criteria
                    else
                    {
                        var fullSubject = string.Join(".", fullSubjectPath);
                        criteria.CriteriaSubjectQuery.RootProperty(QueryRootType.Variable, x => x.Name = $"[{fullSubject}]");
                        criteriaFilter = RenderQueryFilter(criteria.CriteriaSubjectQuery);
                    }
                }

                filters.Add(criteriaFilter);
            }

            var appender = queryNode.Appender ?? CriteriaAppendType.And;

            switch (appender)
            {
                case CriteriaAppendType.And:
                    filter = this.filterBuilder.And(filters);
                    break;
                case CriteriaAppendType.Or:
                    filter = this.filterBuilder.Or(filters);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return filter;
        }

        public FilterDefinition<T> RenderCriteria(QueryNode criteria, List<string> fullSubjectPath)
        {
            FilterDefinition<T> filter;

            var comparator = criteria.Comparator;
            var value = criteria.CriteriaValueConstant;

            var fullSubject = string.Join(".", fullSubjectPath);
            switch (comparator)
            {
                case CriteriaComparator.Eq:
                    filter = this.filterBuilder.Eq(fullSubject, value);
                    break;
                case CriteriaComparator.NotEq:
                    filter = this.filterBuilder.Ne(fullSubject, value);
                    break;
                case CriteriaComparator.Gt:
                    filter = this.filterBuilder.Gt(fullSubject, value.ToString());
                    break;
                case CriteriaComparator.Ge:
                    filter = this.filterBuilder.Gte(fullSubject, value);
                    break;
                case CriteriaComparator.Lt:
                    filter = this.filterBuilder.Lt(fullSubject, value);
                    break;
                case CriteriaComparator.Le:
                    filter = this.filterBuilder.Lte(fullSubject, value);
                    break;
                case CriteriaComparator.In:
                    filter = this.filterBuilder.In(fullSubject, (IEnumerable<object>) value);
                    break;
                case CriteriaComparator.Between:
                    filter = this.filterBuilder.Gte(fullSubject, ((IEnumerable<object>) value).First()) & this.filterBuilder.Lte(fullSubject, ((IEnumerable<object>) value).Last());
                    break;
                case CriteriaComparator.Like:
                    filter = this.filterBuilder.Regex(fullSubject, new BsonRegularExpression(value.ToString(), "i"));
                    break;
                case CriteriaComparator.BeginsWith:
                    filter = this.filterBuilder.Regex(fullSubject, new BsonRegularExpression($"^{value.ToString()}", "i"));
                    break;
                case CriteriaComparator.EndsWith:
                    filter = this.filterBuilder.Regex(fullSubject, new BsonRegularExpression($"{value.ToString()}$", "i"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (criteria.NotModifier == true)
                filter = this.filterBuilder.Not(filter);

            return filter;
        }

        public IFindFluent<T, T> RenderMethod(LinkedListNode<QueryNode> queryNodeItem, IFindFluent<T, T> mongoQuery)
        {
            var queryNode = queryNodeItem.Value;
            var result = mongoQuery;
            switch (queryNode.MethodType)
            {
                case QueryMethodType.Set:
                case QueryMethodType.Add:
                case QueryMethodType.Get:
                case QueryMethodType.Delete:
                case QueryMethodType.ToMd:
                case QueryMethodType.ToT:
                case QueryMethodType.ToLocal:
                    break;
                case QueryMethodType.Take:
                    result = result.Limit(Convert.ToInt32(queryNode.Arguments.First().ArgumentValueConstant));
                    break;
                case QueryMethodType.Skip:
                    result = result.Skip(Convert.ToInt32(queryNode.Arguments.First().ArgumentValueConstant));
                    break;
                case QueryMethodType.OrderBy:
                {
                    foreach (var argument in queryNode.Arguments)
                    {
                        var pathFromRoot = queryNodeItem.GetPathFromRoot(true);
                        pathFromRoot.AddRange(argument.ArgumentValueQuery.NodesList.Last.GetPathFromRoot(true));

                        result = result.Sort(Builders<T>.Sort.Ascending(string.Join(".", pathFromRoot.ConvertForMongo())));
                    }
                }
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}
