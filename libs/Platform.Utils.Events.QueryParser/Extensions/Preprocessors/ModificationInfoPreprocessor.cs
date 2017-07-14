namespace Platform.Utils.Events.QueryParser.Extensions.Preprocessors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Fluent;
    using Json;
    using Newtonsoft.Json.Linq;

    public static class ModificationInfoPreprocessor
    {
        public static SingleQuery MakeCopyUntil(this SingleQuery singleQuery, QueryNode queryNode, bool inclusive = true)
        {
            var newSingleQuery = SingleQuery.CreateQuery;

/*
            var root = singleQuery.NodesList.First.Value;

            if (root.Type == QueryNodeType.Property)
                newSingleQuery.RootProperty(root.RootType.Value);
            else
                newSingleQuery.RootCollection(root.RootType.Value);

            newSingleQuery.SetPivot(root.PivotData.PivotDefinition.Type, root.PivotData.MainValue, root.PivotData.SecondaryValues.ToArray());
*/

//            for (var queryNodeItem = singleQuery.NodesList.First; queryNodeItem?.Next?.Value != queryNode; queryNodeItem = queryNodeItem.Next)
            foreach (var queryNodeToCopy in singleQuery.NodesList)
            {
//                var queryNodeToCopy = queryNodeItem.Value;

                if (queryNodeToCopy == queryNode && !inclusive)
                    break;

                switch (queryNodeToCopy.Type)
                {
                    case QueryNodeType.Property:
                        newSingleQuery.NodesList.AddLast(queryNodeToCopy.MakeDeepCopy());

                        break;
                    case QueryNodeType.Collection:
                        newSingleQuery.NodesList.AddLast(queryNodeToCopy.MakeDeepCopy());

                        break;
                    case QueryNodeType.Method:
                        break;
                    case QueryNodeType.MethodArgument:
                        break;
                    case QueryNodeType.CriteriaGroup:
                        break;
                    case QueryNodeType.Criteria:
                        break;
                    case QueryNodeType.Projection:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (queryNodeToCopy == queryNode)
                    break;

            }

            return newSingleQuery;
        }

        /// <summary> Generates queries for modification info </summary>
        /// <param name="singleQuery"></param>
        /// <param name="modificationInfo"></param>
        /// <param name="isTransactionCreation"></param>
        /// <returns></returns>
        public static List<SingleQuery> GetModificationInfoQueries(this SingleQuery singleQuery, JToken modificationInfo, bool isTransactionCreation)
        {
            var result = new List<SingleQuery>();

            var addMethodNodes = singleQuery.NodesList.Where(x => x.MethodType == QueryMethodType.Add).ToList();
            var setMethodNodes = singleQuery.NodesList.Where(x => x.MethodType == QueryMethodType.Set).ToList();

            foreach (var methodNode in setMethodNodes)
            {
                for (var queryNodeItem = singleQuery.NodesList.First; queryNodeItem.Value != methodNode; queryNodeItem = queryNodeItem.Next)
                {
                    var queryNode = queryNodeItem.Value;
                    var newSingleQuery = singleQuery.MakeCopyUntil(queryNode);
                    newSingleQuery.AddMethod(QueryMethodType.Set)
                        .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.UpdatedInfoPropName),
                                     x => x.ArgumentValueConstant = modificationInfo.ToString());

                    if (isTransactionCreation)
                        newSingleQuery
                        .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.CreatedInfoPropName),
                                     x => x.ArgumentValueConstant = modificationInfo.ToString());

                    result.Add(newSingleQuery);
                }
            }

            foreach (var methodNode in addMethodNodes)
            {
                foreach (var queryNode in singleQuery.NodesList)
                {
                    SingleQuery newSingleQuery = null;
                    if (queryNode == methodNode)
                    {
                        var id =
                            methodNode.Arguments.Where(
                                x => x.ArgumentSubjectQuery.NodesList.Last.Value.Name == ObjectHelper.IdPropName)
                                .Select(x => x.ArgumentValueConstant)
                                .SingleOrDefault();

                        // Somehow Id is empty or absent...
                        if (id == null)
                            continue;

                        newSingleQuery = singleQuery.MakeCopyUntil(queryNode, false);

                        var pivotData = methodNode.PivotData;
                        newSingleQuery.AddCollection(pivotData.PivotDefinition.Type.ToString())
                            .SetPivot(pivotData.PivotDefinition.Type, pivotData.MainValue, pivotData.SecondaryValues.ToArray());

                        newSingleQuery.AddCriteriaGroup(CriteriaAppendType.And)
                            .AddCriteria(SingleQuery.CreateQuery.AddProperty(ObjectHelper.IdPropName), CriteriaComparator.Eq, id);

                        newSingleQuery.AddMethod(QueryMethodType.Set)
                            .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.CreatedInfoPropName),
                                         x => x.ArgumentValueConstant = modificationInfo.ToString());

                        result.Add(newSingleQuery);
                        break;
                    }

                    newSingleQuery = singleQuery.MakeCopyUntil(queryNode);
                    newSingleQuery.AddMethod(QueryMethodType.Set)
                        .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.UpdatedInfoPropName),
                            x => x.ArgumentValueConstant = modificationInfo.ToString());

                    if (isTransactionCreation)
                        newSingleQuery
                            .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty(ObjectHelper.CreatedInfoPropName),
                                x => x.ArgumentValueConstant = modificationInfo.ToString());

                    result.Add(newSingleQuery);
                }
            }

            return result;
        }

        public static List<SingleQuery> GetModificationInfoQueries(this IEnumerable<SingleQuery> singleQueryList, JToken modificationInfo, bool isTransactionCreation)
        {
            var result = singleQueryList.SelectMany(x => x.GetModificationInfoQueries(modificationInfo, isTransactionCreation)).ToList();

            return result;
        }
    }
}
