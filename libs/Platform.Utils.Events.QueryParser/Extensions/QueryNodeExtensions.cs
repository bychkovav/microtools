namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Helpers;
    using Json;

    public enum MethodArgumentType
    {
        KeyValueConstant,
        KeyValueQuery,
        ValueQuery,
    }

    public static class QueryNodeExtensions
    {
        /// <summary> Retreives specified QueryNode Id from criterias </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        public static Guid? GetQueryNodeId(this QueryNode queryNode)
        {
            foreach (var criteria in queryNode.Criterias)
            {
                if (criteria.Type == QueryNodeType.CriteriaGroup)
                    return criteria.GetQueryNodeId();

                var guidString = queryNode
                    .Criterias.FirstOrDefault(x => x.CriteriaSubjectQuery.NodesList.First.Value.Name.ToLower() == ObjectHelper.IdPropName.ToLower())?
                    .CriteriaValueConstant?.ToString();

                Guid? id = null;

                if (!string.IsNullOrEmpty(guidString))
                    id = Guid.Parse(guidString);
                //Guid.TryParse(guidString, out id);

                return id;
            }

            return null;
        }

        /// <summary> Calculates method argument type by subject/value combinations </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        public static MethodArgumentType GetArgumentType(this QueryNode queryNode)
        {
            if (queryNode.ArgumentSubjectQuery != null && queryNode.ArgumentValueQuery != null)
                return MethodArgumentType.KeyValueQuery;

            if (queryNode.ArgumentSubjectQuery == null && queryNode.ArgumentValueQuery != null)
                return MethodArgumentType.ValueQuery;

            if (queryNode.ArgumentSubjectQuery != null)
                return MethodArgumentType.KeyValueConstant;

            throw new InvalidOperationException("Unknown argument type");
        }

        public static List<IGrouping<int, QueryNode>> GetGroupedCriterias(this QueryNode queryNode)
        {
            var groupedCriterias = queryNode.Criterias.GroupBy(o => o.Priority).OrderBy(o => o.Key).ToList();
            return groupedCriterias;
        }

        public static string GetPivotedName(this QueryNode queryNode)
        {
            if (queryNode.PivotData == null)
                return null;

            var result = string.Empty;

            var chunks = new List<string>();

            if (queryNode.PivotData.PivotDefinition.Type != PivotType.Transaction)
                chunks.Add(ParserHelper.GetPivotName(queryNode.PivotData.PivotDefinition.Type));

            var value = string.Empty;

            if (!string.IsNullOrEmpty(queryNode.PivotData.MainValue))
            {
                value = $"{queryNode.PivotData.MainValue}<#0>";

                var secondValue = string.Empty;

                if (queryNode.PivotData.SecondaryValues.Any())
                    secondValue = string.Join(",", queryNode.PivotData.SecondaryValues);

                value = value.Replace("#0", secondValue);
            }

            if (!string.IsNullOrEmpty(value))
                chunks.Add(value);

            result = string.Join(".", chunks);

            return result;
        }
    }
}