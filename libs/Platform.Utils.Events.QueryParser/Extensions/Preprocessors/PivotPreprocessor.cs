namespace Platform.Utils.Events.QueryParser.Extensions.Preprocessors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Fluent;

    public static class PivotPreprocessor
    {
        /// <summary> Generates Criterias based on Pivots inside QueryNodes </summary>
        /// <param name="query"></param>
        /// <param name="withMethods">Should process Add methods as well?</param>
        public static void PivotsToCriterias(this SingleQuery query, bool withMethods)
        {
            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                // Skip pre Add method node pivot filtering
                if (!withMethods &&
                    queryNodeItem.Next?.Value.Type == QueryNodeType.Method &&
                    queryNodeItem.Next?.Value.MethodType == QueryMethodType.Add)
                    continue;

                if (queryNode.Type == QueryNodeType.Collection && queryNode.PivotData != null)
                    queryNode.ConvertPivotToCriterias();
            }
        }

        /// <summary> Converts all pivots (primary and secondary) in node to criterias </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        public static QueryNode ConvertPivotToCriterias(this QueryNode queryNode)
        {
            var pivotValues = queryNode?.PivotData.GetPivotValues();
            if (pivotValues == null)
                return queryNode;

            var secondaryOnly = !queryNode.PivotData.PivotDefinition.StorageFeatures.HasFlag(PivotStorageFeatures.FilterByMainPivotValue);

            if (secondaryOnly)
                pivotValues = pivotValues.Skip(1).ToDictionary(x => x.Key, x => x.Value);

            var additionCriteriaGroup = queryNode.AddCriteriaGroup(CriteriaAppendType.And);

            // Add additional filter
            foreach (var additionalValue in pivotValues)
            {
                var subjectQuery = SingleQuery.CreateQuery.AddProperty(additionalValue.Key);

                var comparator = additionalValue.Value is IEnumerable<string>
                    ? CriteriaComparator.In
                    : CriteriaComparator.Eq;

                additionCriteriaGroup.AddCriteria(subjectQuery, comparator, additionalValue.Value);
            }
            return queryNode;
        }

        /// <summary> Value to Dictionary access (Value['xxx'] </summary>
        /// <param name="queryNode"></param>
        [Obsolete("Not needed anymore")]
        public static void ConvertCriteriaValue(this QueryNode queryNode)
        {
            queryNode.Criterias
                .ForEach(criteria =>
                {
                    var valuePos = criteria.CriteriaSubjectPath.IndexOf("Value");
                    var valuePos2 = criteria.CriteriaSubjectPath.IndexOf("Value", valuePos + 1);

                    if (valuePos >= 0)
                    {
                        if (valuePos2 >= 0)
                            valuePos = valuePos2;

                        var valueName = criteria.CriteriaSubjectPath[valuePos + 1];

                        criteria.CriteriaSubjectPath[valuePos] = $"Value[\"{valueName}\"]";
                        criteria.CriteriaSubjectPath.RemoveAt(valuePos + 1);
                    }
                }
                );
        }
    }
}
