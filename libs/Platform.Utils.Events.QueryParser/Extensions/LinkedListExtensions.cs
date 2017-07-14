namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System.Collections.Generic;
    using Domain.Enums;
    using Domain.Objects;

    public static class LinkedListExtensions
    {
        public static int IndexOf<T>(this LinkedList<T> list, T item)
        {
            var count = 0;
            for (var node = list.First; node != null; node = node.Next, count++)
            {
                if (item.Equals(node.Value))
                    return count;
            }
            return -1;
        }

        public static List<string> GetPathFromRoot(this LinkedList<QueryNode> nodesList, QueryNode queryNode)
        {
            var pathFromRoot = new List<string>();
            foreach (var node in nodesList)
            {
                if (node == queryNode)
                    break;

                if (node.Type == QueryNodeType.Method ||
                    node.RootType.HasValue)
                    continue;

                pathFromRoot.Add(node.Name);
            }

            return pathFromRoot;
        }
    }
}