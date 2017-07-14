namespace Platform.Utils.Events.QueryParser.Domain.Objects
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class SingleQuery
    {
        [Obsolete("Use InitiateQuery(parentQuery)")]
        public static SingleQuery CreateQuery => new SingleQuery();

        public static SingleQuery InitiateQuery(SingleQuery parentQuery = null)
        {
            var modelInfo = parentQuery?.NodesList.First?.Value.PivotData;

            var newQuery = new SingleQuery { ParentQueryModelInfo = modelInfo};
            return newQuery;
        } 

        public SingleQuery AddSubQuery()
        {
            return InitiateQuery(this);
        } 

        public PivotData ParentQueryModelInfo { get; set; }

        public LinkedList<QueryNode> NodesList { get; } = new LinkedList<QueryNode>(); 
    }
}