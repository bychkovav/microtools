namespace Platform.Utils.Events.QueryParser.Domain.Objects
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Newtonsoft.Json;

    [Serializable]
    public class QueryNode
    {
        /// <summary> 
        /// SingleQuery this QueryNode belongs to.
        /// * FOR INTERNAL USE ONLY *
        /// </summary>
        [JsonIgnore]
        public SingleQuery BelongsToQuery { get; set; }

        /// <summary> Node type </summary>
        public QueryNodeType Type { get; set; }

        /// <summary> Node name </summary>
        public string Name { get; set; }

        public PivotData PivotData { get; set; }

        #region

        /// <summary> Projections [alias, query] </summary>
        public Dictionary<string, SingleQuery> Projections { get; } = new Dictionary<string, SingleQuery>();

        #endregion

        #region Root type

        /// <summary> Root node type </summary>
        public QueryRootType? RootType { get; set; }

        #endregion

        #region Collection type

        public List<QueryNode> Criterias { get; } = new List<QueryNode>();
        
        #endregion

        #region Criteria type

        public int Priority { get; set; }

        public CriteriaAppendType? Appender { get; set; }

        public bool? NotModifier { get; set; }

        public CriteriaComparator? Comparator { get; set; }

        /// <summary> Criteria Subject sptilled to nodes: (a.b.c = 1) </summary>
        public List<string> CriteriaSubjectPath { get; } = new List<string>();

        public SingleQuery CriteriaSubjectQuery { get; set; }

        public object CriteriaValueConstant { get; set; }

        public SingleQuery CriteriaValueQuery { get; set; }

        #endregion

        #region Method type

        public QueryMethodType? MethodType { get; set; }

        public List<QueryNode> Arguments { get; } = new List<QueryNode>();

        #endregion

        #region Method argument type

        public SingleQuery ArgumentSubjectQuery { get; set; }
        [Obsolete]
        public string ArgumentSubject { get; set; }
        public object ArgumentValueConstant { get; set; }
        public SingleQuery ArgumentValueQuery { get; set; }

        #endregion
    }
}