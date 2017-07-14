namespace Platform.Utils.Events.QueryParser.Builders.QueryLanguage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Extensions;
    using Helpers;
    using QueryParser.Extensions;

    public class QueryLanguageBuilder
    {
        public string RenderQuery(SingleQuery query)
        {
            var queryNode = query.NodesList.First;

            var chunks = new List<string>();

            while (queryNode != null)
            {
                switch (queryNode.Value.Type)
                {
                    case QueryNodeType.Property:
                        chunks.Add(RenderProperty(queryNode.Value));

                        break;
                    case QueryNodeType.Collection:
                        chunks.Add(RenderCollection(queryNode.Value));

                        break;
                    case QueryNodeType.Method:
                        chunks.Add(RenderMethod(queryNode.Value));

                        break;
                    case QueryNodeType.MethodArgument:
                        break;
                    case QueryNodeType.Criteria:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                queryNode = queryNode.Next;
            }

            var queryText = string.Join(".", chunks);

            return queryText;
        }


        public string RenderProperty(QueryNode queryNode)
        {
            var result = string.Empty;

            result = queryNode.GetNodeName();

            return result;
        }

        public string RenderCollection(QueryNode queryNode)
        {
            var result = string.Empty;
            var collectionName = queryNode.GetNodeName();

            result = $"{collectionName}(#0)";

            var collectionFilter = RenderCriteriaGroup(queryNode);

            return result.Replace("#0", collectionFilter);
        }

        public string RenderCriteriaGroup(QueryNode queryNode)
        {
            var result = string.Empty;

            var criterias = new List<string>();

            foreach (var criteria in queryNode.Criterias)
            {
                var criteriaString = string.Empty;

                if (criteria.Type == QueryNodeType.CriteriaGroup)
                    criteriaString = RenderCriteriaGroup(criteria);
                else
                    criteriaString = RenderCriteria(criteria);

                criterias.Add(criteriaString);
            }

            var appender = ParserHelper.GetTokenName(ParserHelper.GetToken(queryNode.Appender ?? CriteriaAppendType.And));
            
            result = $"{string.Join($" {appender} ", criterias)}";

            if (criterias.Count > 1)
                result = $"({result})";

            return result;
        }

        public string RenderCriteria(QueryNode queryNode)
        {
            var result = String.Empty;

            var criteriaValue = queryNode.CriteriaValueQuery != null
                ? RenderQuery(queryNode.CriteriaValueQuery)
                : queryNode.CriteriaValueConstant.ToQueryLanguageRepresentation();

            var comparator = queryNode.Comparator;

            // "subject = value" type criteria
            if (comparator.HasValue)
                result = $"{RenderQuery(queryNode.CriteriaSubjectQuery)} {ParserHelper.GetTokenName(ParserHelper.GetToken(comparator.Value))} {criteriaValue}";
            // "subject()" type criteria
            else
            {
                result = RenderQuery(queryNode.CriteriaSubjectQuery);
            }

            return result;
        }

        public string RenderMethod(QueryNode queryNode)
        {
            var result = $"_{ParserHelper.GetTokenName(ParserHelper.GetToken(queryNode.MethodType.Value))}(#0)";

            switch (queryNode.MethodType)
            {
                case QueryMethodType.Set:
                {
//                    result = $"_{ParserHelper.GetTokenName((int)QueryMethodType.Set)}(#0)";

                    var arguments = new List<string>();

                    foreach (var argument in queryNode.Arguments)
                    {
                        var argumentValue = string.Empty;
                        if (argument.ArgumentValueQuery != null)
                            argumentValue = RenderQuery(argument.ArgumentValueQuery);
                        else
                            argumentValue = argument.ArgumentValueConstant.ToQueryLanguageRepresentation();

                        arguments.Add($"{RenderQuery(argument.ArgumentSubjectQuery)} = {argumentValue}");
                    }

                    result = result.Replace("#0", string.Join(", ", arguments));

                    break;
                }
                case QueryMethodType.Add:
                {
//                    result = $"{ParserHelper.GetTokenName((int)QueryMethodType.Add)}(#0)";
                    var arguments = new List<string>();

                    if (queryNode.Arguments.FirstOrDefault()?.ArgumentValueQuery != null)
                    {
                        arguments.Add(RenderQuery(queryNode.Arguments.First().ArgumentValueQuery));
                    }
                    else
                    {
                        foreach (var argument in queryNode.Arguments)
                        {
                            var argumentValue = string.Empty;
                            if (argument.ArgumentValueQuery != null)
                                argumentValue = RenderQuery(argument.ArgumentValueQuery);
                            else
                                argumentValue = argument.ArgumentValueConstant.ToQueryLanguageRepresentation();

                            arguments.Add($"{RenderQuery(argument.ArgumentSubjectQuery)} = {argumentValue}");
                        }
                    }

                    result = result.Replace("#0", string.Join(", ", arguments));

                    result = $"{queryNode.GetNodeName()}().{result}";

                    break;
                }
                case QueryMethodType.Get:
                case QueryMethodType.Delete:
                    result = result.Replace("#0", string.Empty);
                    break;
                case QueryMethodType.ToMd:
                    break;
                case QueryMethodType.ToT:
                    break;
                case QueryMethodType.ToLocal:
                    break;
                case QueryMethodType.Take:
                case QueryMethodType.Skip:
                    result = result.Replace("#0", queryNode.Arguments.Single().ArgumentValueConstant.ToQueryLanguageRepresentation());
                    break;
                case QueryMethodType.OrderBy:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // TODO: Arguments

            return result;
        }
    }
}
