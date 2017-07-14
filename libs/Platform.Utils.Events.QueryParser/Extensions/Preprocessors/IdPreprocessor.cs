namespace Platform.Utils.Events.QueryParser.Extensions.Preprocessors
{
    using System;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Fluent;
    using Json;

    public static class IdPreprocessor
    {
        /// <summary> Pregenerates Id for model initiates, possible occured in query </summary>
        /// <param name="query"></param>
        public static void GenerateIds(this SingleQuery query)
        {
            for (var queryNodeItem = query.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

/*
                var shouldGenerate = queryNode.RootType == QueryRootType.Model ||
                                        queryNode.MethodType == QueryMethodType.Add;

                    // TODO: more checks
*/

                // Deeply iterate over queries
                foreach (var argument in queryNode.Arguments)
                {
                    argument.ArgumentValueQuery?.GenerateIds();
                }

                // Generate new id and set if needed
                var newId = Guid.NewGuid();

                if (queryNode.MethodType == QueryMethodType.Add)
                {
                    // Skip ID generation if we have ID already assigned in ._Add(id = xxx)
                    if (queryNode.Arguments.Any(x => x.ArgumentSubjectQuery.NodesList.First().Name == ObjectHelper.IdPropName))
                        continue;

                    queryNodeItem.AddArgument(
                        x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.IdPropName),
                        x => x.ArgumentValueConstant = newId);
                }
                else if (queryNode.RootType == QueryRootType.Model)
                {
                    var tempQuery = SingleQuery.InitiateQuery()
                        .RootProperty(QuerySource.API)
                        .AddMethod(QueryMethodType.Set)
                        .AddArgument(
                            x =>
                                x.ArgumentSubjectQuery =
                                    SingleQuery.CreateQuery.AddProperty(ObjectHelper.IdPropName),
                            x => x.ArgumentValueConstant = newId
                        );

                    var newNode = tempQuery
                        .NodesList
                        .Last
                        .Value;

                    query.NodesList.AddAfter(queryNodeItem, newNode);
                }
            }
        }

        /// <summary> Convert Id to masterId if applicable </summary>
        /// <param name="collectionNode"></param>
        public static void ConvertIdToMasterId(this QueryNode collectionNode)
        {
            return; // switched of conversion
            if (collectionNode.PivotData != null &&
                !collectionNode.PivotData.PivotDefinition.CommonProperties.Contains(ObjectHelper.MasterIdPropName))
                return;

            Action<QueryNode> processCriteriaNode = null;

            processCriteriaNode = criteriaNode =>
            {
                if (criteriaNode.Type == QueryNodeType.Criteria)
                {
                    var idNode = criteriaNode.CriteriaSubjectQuery.NodesList.Last.Value;

                    if (idNode.Type == QueryNodeType.Property && idNode.Name == ObjectHelper.IdPropName)
                        idNode.Name = ObjectHelper.MasterIdPropName;
                }
                else
                {
                    foreach (var criteria in criteriaNode.Criterias)
                        processCriteriaNode(criteria);
                }
            };

            processCriteriaNode(collectionNode);
        }

        /// <summary> Convert Id to masterId if applicable </summary>
        /// <param name="singleQuery"></param>
        public static void ConvertIdToMasterId(this SingleQuery singleQuery)
        {
            for (var queryNodeItem = singleQuery.NodesList.First; queryNodeItem != null; queryNodeItem = queryNodeItem.Next)
            {
                var queryNode = queryNodeItem.Value;

                if (queryNode.Type == QueryNodeType.Collection)
                {
                    queryNode.ConvertIdToMasterId();
                }
            }
        }
    }
}
