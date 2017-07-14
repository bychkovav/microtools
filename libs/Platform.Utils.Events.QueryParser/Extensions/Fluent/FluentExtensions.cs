namespace Platform.Utils.Events.QueryParser.Extensions.Fluent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Helpers;

    public static class FluentExtensions
    {
        #region Root

        /// <summary> Add property root node based on query source </summary>
        /// <param name="singleQuery"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SingleQuery RootProperty(this SingleQuery singleQuery, QuerySource type)
        {
            var rootNode = singleQuery.RootProperty(QueryRootType.InputData);

            return singleQuery;
        }

        /// <summary> Add collection root node based on query source </summary>
        /// <param name="singleQuery"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SingleQuery RootCollection(this SingleQuery singleQuery, QuerySource type)
        {
            var rootNode = singleQuery.RootCollection(QueryRootType.InputData);

            return singleQuery;
        }

        public static SingleQuery RootProperty(this SingleQuery singleQuery, QueryRootType type,
            params Action<QueryNode>[] actions)
        {
            var rootNode = CreateRoot(QueryNodeType.Property, type, actions);

            var newNode = singleQuery.NodesList.AddFirst(rootNode);

            return singleQuery;
        }

        public static SingleQuery RootCollection(this SingleQuery singleQuery, QueryRootType type,
            params Action<QueryNode>[] actions)
        {
            var rootNode = CreateRoot(QueryNodeType.Collection, type, actions);

            var newNode = singleQuery.NodesList.AddFirst(rootNode);

            return singleQuery;
        }

        public static QueryNode CreateRoot(QueryNodeType type, QueryRootType rootType,
            params Action<QueryNode>[] actions)
        {
            var rootNode = new QueryNode
            {
                Type = type,
                RootType = rootType,
            };

            actions?.ForEach(x => x.Invoke(rootNode));

            return rootNode;
        }

        #endregion

        #region Properties

        /// <summary> Adds property as last node </summary>
        /// <param name="singleQuery"></param>
        /// <param name="name"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery AddProperty(this SingleQuery singleQuery, string name,
            params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.AddProperty(name, actions);

            return singleQuery;
        }

        /// <summary> Adds property as last node </summary>
        /// <param name="nodeList"></param>
        /// <param name="name"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddProperty(this LinkedList<QueryNode> nodeList, string name,
            params Action<QueryNode>[] actions)
        {
            var queryNode = CreateProperty(name, actions);

            var newNode = nodeList.AddLast(queryNode);

            return newNode;
        }

        /// <summary> Adds property after specified node </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddProperty(this LinkedListNode<QueryNode> node, string name,
            params Action<QueryNode>[] actions)
        {
            var queryNode = CreateProperty(name, actions);
            var newNode = node.List.AddAfter(node, queryNode);

            return newNode;
        }

        public static QueryNode CreateProperty(string name, params Action<QueryNode>[] actions)
        {
            var queryNode = new QueryNode
            {
                Type = QueryNodeType.Property,
                Name = name,
            };

            actions?.ForEach(x => x.Invoke(queryNode));

            return queryNode;
        }

        #endregion

        #region Collections

        public static SingleQuery AddCollection(this SingleQuery singleQuery, string name,
            params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.AddCollection(name, actions);

            return singleQuery;
        }

        public static LinkedListNode<QueryNode> AddCollection(this LinkedList<QueryNode> nodeList, string name,
            params Action<QueryNode>[] actions)
        {
            var queryNode = CreateCollection(name, actions);
            var newNode = nodeList.AddLast(queryNode);

            return newNode;
        }

        public static LinkedListNode<QueryNode> AddCollection(this LinkedListNode<QueryNode> node, string name,
            params Action<QueryNode>[] actions)
        {
            var queryNode = CreateCollection(name, actions);
            var newNode = node.List.AddAfter(node, queryNode);

            return newNode;
        }

        #region Criterias

        /// <summary> Adds Criteria to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="appender"></param>
        /// <param name="name"></param>
        /// <param name="comparator"></param>
        /// <param name="value"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery AddCriteria(this SingleQuery singleQuery,
            CriteriaAppendType appender, string name, CriteriaComparator comparator, object value,
            params Action<QueryNode>[] actions)
        {
            singleQuery.NodesList.Last.AddCriteria(appender, name, comparator, value, actions);

            return singleQuery;
        }

        public static SingleQuery AddCriteria(this SingleQuery singleQuery,
            CriteriaAppendType appender, SingleQuery subjectQuery, CriteriaComparator comparator, object value,
            params Action<QueryNode>[] actions)
        {
            singleQuery.NodesList.Last.AddCriteria(appender, subjectQuery, comparator, value, actions);

            return singleQuery;
        }

        /// <summary> Adds Criteria to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="appender"></param>
        /// <param name="name"></param>
        /// <param name="comparator"></param>
        /// <param name="value"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddCriteria(this LinkedListNode<QueryNode> node,
            CriteriaAppendType appender, string name, CriteriaComparator comparator, object value,
            params Action<QueryNode>[] actions)
        {
            var criteriaSubjectQuery = SingleQuery.CreateQuery.AddProperty(name);

            node.AddCriteria(appender, criteriaSubjectQuery, comparator, value, actions);

            return node;
        }

        public static LinkedListNode<QueryNode> AddCriteria(this LinkedListNode<QueryNode> node,
            CriteriaAppendType appender, SingleQuery subjectQuery, CriteriaComparator comparator, object value,
            params Action<QueryNode>[] actions)
        {
            node.Value.AddCriteria(subjectQuery, comparator, value, actions);

            return node;
        }

        public static QueryNode AddCriteria(this QueryNode node, SingleQuery subjectQuery, CriteriaComparator comparator, object value,
            params Action<QueryNode>[] actions)
        {
            var newNode = new QueryNode
            {
                Type = QueryNodeType.Criteria,
                CriteriaValueConstant = value,
                Comparator = comparator,
                CriteriaSubjectQuery = subjectQuery,
            };

            actions?.ForEach(x => x.Invoke(newNode));

            node.Criterias.Add(newNode);

            return node;
        }

        public static QueryNode AddCriteria(this QueryNode node, SingleQuery subjectQuery, params Action<QueryNode>[] actions)
        {
            var newNode = new QueryNode
            {
                Type = QueryNodeType.Criteria,
//                CriteriaValueConstant = value,
//                Comparator = comparator,
                CriteriaSubjectQuery = subjectQuery,
            };

            actions?.ForEach(x => x.Invoke(newNode));

            node.Criterias.Add(newNode);

            return node;
        }

        /// <summary> Adds new criteria group to last node in SigleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="appender"></param>
        /// <param name="actions"></param>
        /// <returns> Newly created Criteria Group QueryNode </returns>
        public static QueryNode AddCriteriaGroup(this SingleQuery singleQuery, CriteriaAppendType appender,
            params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.Last.Value.AddCriteriaGroup(appender, actions);

            return newNode;
        }

        /// <summary> Adds new criteria group to specified QueryNode </summary>
        /// <param name="node"></param>
        /// <param name="appender"></param>
        /// <param name="actions"></param>
        /// <returns> Newly created Criteria Group QueryNode </returns>
        public static QueryNode AddCriteriaGroup(this QueryNode node, CriteriaAppendType appender,
            params Action<QueryNode>[] actions)
        {
            var newNode = new QueryNode
            {
                Type = QueryNodeType.CriteriaGroup,
                Appender = appender,
            };

            actions?.ForEach(x => x.Invoke(newNode));

            node.Criterias.Add(newNode);

            return newNode;
        }

        #endregion

        public static QueryNode CreateCollection(string name, params Action<QueryNode>[] actions)
        {
            var queryNode = new QueryNode
            {
                Type = QueryNodeType.Collection,
                Name = name,
            };

            actions?.ForEach(x => x.Invoke(queryNode));

            return queryNode;
        }

        #endregion

        #region Methods

        #region Set

        /// <summary> Adds method "Set" to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery MethodSet(this SingleQuery singleQuery, params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.Last.MethodSet(actions);

            return singleQuery;
        }

        /// <summary> Adds method "Set" to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> MethodSet(this LinkedListNode<QueryNode> node,
            params Action<QueryNode>[] actions)
        {
            var newNode = node.AddMethod(QueryMethodType.Set, actions);

            return newNode;
        }

        #endregion

        #region Add

        /// <summary> Adds method "Add" to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery MethodAdd(this SingleQuery singleQuery, params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.Last.MethodAdd(actions);

            return singleQuery;
        }

        /// <summary> Adds method "Add" to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> MethodAdd(this LinkedListNode<QueryNode> node,
            params Action<QueryNode>[] actions)
        {
            var newNode = node.AddMethod(QueryMethodType.Add, actions);

            return newNode;
        }

        #endregion

        #region Delete

        /// <summary> Adds method "Delete" to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery MethodDelete(this SingleQuery singleQuery,
            params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.Last.MethodDelete(actions);

            return singleQuery;
        }

        /// <summary> Adds method "Delete" to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> MethodDelete(this LinkedListNode<QueryNode> node,
            params Action<QueryNode>[] actions)
        {
            var newNode = node.AddMethod(QueryMethodType.Delete, actions);

            return newNode;
        }

        #endregion

        #region Generic

        /// <summary> Adds specified method to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="type">Method type to add</param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery AddMethod(this SingleQuery singleQuery, QueryMethodType type,
            params Action<QueryNode>[] actions)
        {
            var newNode = singleQuery.NodesList.Last.AddMethod(type, actions);

            return singleQuery;
        }

        /// <summary> Adds specified method to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="type">Method type to add</param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddMethod(this LinkedListNode<QueryNode> node, QueryMethodType type,
            params Action<QueryNode>[] actions)
        {
            var queryNode = CreateMethod(type, actions);

            var newNode = node.List.AddLast(queryNode);

            return newNode;
        }

        #endregion

        #region Arguments

        /// <summary> Adds argument to last node in SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery AddArgument(this SingleQuery singleQuery, params Action<QueryNode>[] actions)
        {
            singleQuery.NodesList.Last.AddArgument(actions);

            return singleQuery;
        }

        /// <summary> Adds argument to specified node in SingleQuery </summary>
        /// <param name="node"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddArgument(this LinkedListNode<QueryNode> node,
            params Action<QueryNode>[] actions)
        {
            var argument = new QueryNode
            {
                Type = QueryNodeType.MethodArgument,
            };

            actions?.ForEach(x => x.Invoke(argument));

            node.Value.Arguments.Add(argument);

            return node;
        }

        #endregion

        public static QueryNode CreateMethod(QueryMethodType type, params Action<QueryNode>[] actions)
        {
            var queryNode = new QueryNode
            {
                Type = QueryNodeType.Method,
                MethodType = type,
            };

            actions?.ForEach(x => x.Invoke(queryNode));

            return queryNode;
        }

        #endregion

        #region Pivots

        public static QueryNode SetPivot(this QueryNode queryNode, PivotType type, string mainValue,
            params string[] secondaryValues)
        {
            var pivotData = new PivotData
            {
                MainValue = mainValue,
                SecondaryValues = secondaryValues?.ToList() ?? new List<string>(),
                PivotDefinition = ParserHelper.GetPivot(type)
            };

            queryNode.PivotData = pivotData;

            return queryNode;
        }

        public static SingleQuery SetPivot(this SingleQuery singleQuery, PivotType type,
            string mainValue, params string[] secondaryValues)
        {
            singleQuery.NodesList.SetPivot(type, mainValue, secondaryValues);

            return singleQuery;
        }

        public static LinkedListNode<QueryNode> SetPivot(this LinkedList<QueryNode> nodeList, PivotType type,
            string mainValue, params string[] secondaryValues)
        {
            var node = nodeList.Last.SetPivot(type, mainValue, secondaryValues);

            return node;
        }

        public static LinkedListNode<QueryNode> SetPivot(this LinkedListNode<QueryNode> node, PivotType type,
            string mainValue, params string[] secondaryValues)
        {
            node.Value.SetPivot(type, mainValue, secondaryValues);

            return node;
        }

        #endregion

        #region Projections

        /// <summary> Adds projection to last node </summary>
        /// <param name="singleQuery"></param>
        /// <param name="projection">Projection query</param>
        /// <param name="projectionName">Projection alias</param>
        /// <returns></returns>
        public static SingleQuery AddProjection(this SingleQuery singleQuery, SingleQuery projection, string projectionName)
        {
            singleQuery.NodesList.Last.AddProjection(projection, projectionName);

            return singleQuery;
        }

        /// <summary> Adds projection to specified node </summary>
        /// <param name="node"></param>
        /// <param name="projection">Projection query</param>
        /// <param name="projectionName">Projection alias</param>
        /// <returns></returns>
        public static LinkedListNode<QueryNode> AddProjection(this LinkedListNode<QueryNode> node, SingleQuery projection, string projectionName)
        {
            node.Value.Projections.Add(projectionName, projection);

            return node;
        }

        #endregion

        /// <summary> Just adds specified QueryNode as last node to SingleQuery </summary>
        /// <param name="singleQuery"></param>
        /// <param name="queryNode"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static SingleQuery AddNode(this SingleQuery singleQuery, QueryNode queryNode, params Action<QueryNode>[] actions)
        {
            actions?.ForEach(x => x.Invoke(queryNode));

            singleQuery.NodesList.AddLast(queryNode);

            return singleQuery;
        }
    }
}