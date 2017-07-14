namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using QueryParser.Extensions;

    public static class QueryNodeExtensions
    {
        /// <summary>
        /// Gets jpath node representation
        /// </summary>
        /// <param name="queryNode"></param>
        /// <param name="escape">wrap into ['...']</param>
        /// <returns></returns>
        public static string GetJTokenName(this QueryNode queryNode, bool escape = true)
        {
            var result = string.Empty;

            var tokens = queryNode.GetPivotedNodes();

            if (queryNode.PivotData?.PivotDefinition.StorageFeatures.HasFlag(PivotStorageFeatures.StoreBySecondPivotValue) != true)
                tokens = tokens.Take(1).ToList();

            tokens = escape ? tokens.EscapeJTokenNames() : tokens;

            result = string.Join(".", tokens);

            return result;
        }

        public static List<string> GetPivotedNodes(this QueryNode queryNode)
        {
            var result = new List<string>();

            if (queryNode.PivotData != null)
            {
                result = queryNode.PivotData.GetJTokenPath();
            }
            else
            {
                // For root node name is not JSON node
                if (!queryNode.RootType.HasValue)
                    result.Add(queryNode.Name);
            }

            return result;
        }

        public static List<string> GetPivotTokens(this QueryNode queryNode, bool primaryOnly = false)
        {
            var nodes = queryNode.GetPivotedNodes();
            if (primaryOnly)
                nodes = nodes.Take(1).ToList();

            var result = nodes.EscapeJTokenNames();

            return result;
        }

        /// <summary>
        /// Get path from root node
        /// </summary>
        /// <param name="listNode"></param>
        /// <returns></returns>
        public static List<string> GetPathFromRoot(this LinkedListNode<QueryNode> listNode, bool includeRoot = false)
        {
            var pathFromRoot = new List<string>();

            var currentNode = listNode;

            while (currentNode != null)
            {
                if (currentNode.Value.Type != QueryNodeType.Method && (!currentNode.Value.RootType.HasValue || includeRoot))
                    pathFromRoot.Add(currentNode.Value.GetJTokenName(escape: false));

                currentNode = currentNode.Previous;
            }

            pathFromRoot.Reverse();

            return pathFromRoot;
        }
    }
}