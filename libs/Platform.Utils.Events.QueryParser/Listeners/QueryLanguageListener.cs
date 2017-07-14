namespace Platform.Utils.Events.QueryParser.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Domain.Syntax;
    using Extensions;
    using Helpers;

    internal class QueryLanguageListener : QueryLanguageBaseListener
    {
        public SingleQuery RootQuery { get; set; }

        private Stack<ProcessingObjectType> processingObjectTypeStack { get; } = new Stack<ProcessingObjectType>();

        private Stack<SingleQuery> queryStack { get; } = new Stack<SingleQuery>();
        private Stack<QueryNode> processingQueryNodeStack { get; } = new Stack<QueryNode>();

        private SingleQuery CurrentQuery => queryStack.Any() ? queryStack.Peek() : null;
        private QueryNode CurrentProcessingQueryNode => processingQueryNodeStack.Any() ? processingQueryNodeStack.Peek() : null;

        private ProcessingObjectType? CurrentProcessingObjectType
            => processingObjectTypeStack.Any() ? processingObjectTypeStack.Peek() : (ProcessingObjectType?) null;

        public override void EnterQuery(QueryLanguageParser.QueryContext context)
        {
            var newQuery = SingleQuery.InitiateQuery(CurrentQuery);
            queryStack.Push(newQuery);

            base.EnterQuery(context);
        }

        public override void ExitQuery(QueryLanguageParser.QueryContext context)
        {
            var preparedQuery = queryStack.Pop();

            switch (CurrentProcessingObjectType)
            {
                case ProcessingObjectType.Node:
                    break;
                case ProcessingObjectType.ConditionSubject:
                    CurrentProcessingQueryNode.CriteriaSubjectQuery = preparedQuery;
                    break;
                case ProcessingObjectType.ConditionValue:
                    CurrentProcessingQueryNode.CriteriaValueQuery = preparedQuery;
                    break;
                case ProcessingObjectType.MethodArgument:
                    break;
                case ProcessingObjectType.MethodArgumentSubject:
                    CurrentProcessingQueryNode.ArgumentSubjectQuery = preparedQuery;
                    break;
                case ProcessingObjectType.MethodArgumentValue:
                    CurrentProcessingQueryNode.ArgumentValueQuery = preparedQuery;
                    break;
                case ProcessingObjectType.Projection:
                    CurrentProcessingQueryNode.Projections[preparedQuery.NodesList.First.Value.GetPivotedName() ?? preparedQuery.NodesList.First.Value.Name] = preparedQuery;
                    break;
                case null:
                    RootQuery = preparedQuery;
                    break;
                default:

                    throw new ArgumentOutOfRangeException();
            }

            base.ExitQuery(context);
        }

        #region Helpers

        private QueryNode StartQueryNode(QueryNodeType type, ProcessingObjectType processAs)
        {
            var newNode = new QueryNode
            {
                Type = type,
            };

            processingObjectTypeStack.Push(processAs);
            processingQueryNodeStack.Push(newNode);

            return newNode;
        }

        private QueryNode FinishQueryNode()
        {
            var preparedQueryNode = processingQueryNodeStack.Pop();
            var processAs = processingObjectTypeStack.Pop();

            // TODO: Find better way
            preparedQueryNode.BelongsToQuery = CurrentQuery;

            switch (processAs)
            {
                case ProcessingObjectType.Node:
                    CurrentQuery.NodesList.AddLast(preparedQueryNode);
                    break;
                case ProcessingObjectType.ConditionSubject:
                    break;
                case ProcessingObjectType.ConditionValue:
                    break;
                case ProcessingObjectType.MethodArgument:
                    CurrentProcessingQueryNode.Arguments.Add(preparedQueryNode);
                    break;
                case ProcessingObjectType.MethodArgumentSubject:
                    break;
                case ProcessingObjectType.MethodArgumentValue:
                    break;
                case ProcessingObjectType.Projection:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return preparedQueryNode;
        }

        #endregion

        public override void EnterQueryNode(QueryLanguageParser.QueryNodeContext context)
        {
            StartQueryNode(QueryNodeType.Property, ProcessingObjectType.Node);

            base.EnterQueryNode(context);
        }

        public override void ExitQueryNode(QueryLanguageParser.QueryNodeContext context)
        {
            FinishQueryNode();

            base.ExitQueryNode(context);
        }

        public override void EnterRootNode(QueryLanguageParser.RootNodeContext context)
        {
            var rootVariable = context.variable()?.variableName.Text;
            var rootModel = context.model()?.stringType().GetText().Trim('\'').Trim('"');
            var inputData = context.EData()?.GetText();

            CurrentProcessingQueryNode.Type = QueryNodeType.Property;

            CurrentProcessingQueryNode.RootType = rootVariable != null ? QueryRootType.Variable : rootModel != null ? QueryRootType.Model : QueryRootType.InputData;

            CurrentProcessingQueryNode.Name = rootVariable ?? rootModel ?? inputData;

            base.EnterRootNode(context);
        }

        public override void ExitRootNode(QueryLanguageParser.RootNodeContext context)
        {
            base.ExitRootNode(context);
        }

        public override void EnterProperty(QueryLanguageParser.PropertyContext context)
        {
            var propertyName = context.GetText();
            CurrentProcessingQueryNode.Type = QueryNodeType.Property;
            CurrentProcessingQueryNode.Name = propertyName;
            base.EnterProperty(context);
        }

        #region Methods

        public override void EnterMethod(QueryLanguageParser.MethodContext context)
        {
            var method = ParserHelper.GetQueryMethodType(context.methodType.Type);

            CurrentProcessingQueryNode.Type = QueryNodeType.Method;
            CurrentProcessingQueryNode.MethodType = method;

            // Fill specific data for Add method
            if (CurrentProcessingQueryNode.MethodType == QueryMethodType.Add)
            {
                var previousNode = CurrentQuery.NodesList.Last.Value;
                CurrentProcessingQueryNode.Name = previousNode.Name;
                CurrentProcessingQueryNode.PivotData = previousNode.PivotData;
                CurrentQuery.NodesList.RemoveLast();
            }

            base.EnterMethod(context);
        }

        public override void ExitMethod(QueryLanguageParser.MethodContext context)
        {
            base.ExitMethod(context);
        }

        #region Set

        public override void EnterSetArgument(QueryLanguageParser.SetArgumentContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgument);

//            var subject = context.subject.GetText();
//            var valueConstant = context.constantValue.GetValue();

            var newQueryNode = new QueryNode
            {
                Type = QueryNodeType.MethodArgument,
//                ArgumentSubject = subject,
//                ArgumentValueConstant = valueConstant,
            };

            processingQueryNodeStack.Push(newQueryNode);

            base.EnterSetArgument(context);
        }

        public override void ExitSetArgument(QueryLanguageParser.SetArgumentContext context)
        {
            processingObjectTypeStack.Pop();
            var preparedArgumentNode = processingQueryNodeStack.Pop();
            CurrentProcessingQueryNode.Arguments.Add(preparedArgumentNode);

            base.ExitSetArgument(context);
        }

        public override void EnterSetArgumentSubject(QueryLanguageParser.SetArgumentSubjectContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgumentSubject);

            base.EnterSetArgumentSubject(context);
        }

        public override void ExitSetArgumentSubject(QueryLanguageParser.SetArgumentSubjectContext context)
        {
            processingObjectTypeStack.Pop();

            base.ExitSetArgumentSubject(context);
        }

        public override void EnterSetArgumentValue(QueryLanguageParser.SetArgumentValueContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgumentValue);

            var constantValue = context.typeValue()?.GetValue();
            CurrentProcessingQueryNode.ArgumentValueConstant = constantValue;

            base.EnterSetArgumentValue(context);
        }

        public override void ExitSetArgumentValue(QueryLanguageParser.SetArgumentValueContext context)
        {
            processingObjectTypeStack.Pop();
            base.ExitSetArgumentValue(context);
        }

        #endregion

        #region Add

        public override void EnterAddArgument(QueryLanguageParser.AddArgumentContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgument);

            var newQueryNode = new QueryNode
            {
                Type = QueryNodeType.MethodArgument,
            };

            processingQueryNodeStack.Push(newQueryNode);

            base.EnterAddArgument(context);
        }

        public override void ExitAddArgument(QueryLanguageParser.AddArgumentContext context)
        {
            processingObjectTypeStack.Pop();
            var preparedArgumentNode = processingQueryNodeStack.Pop();

            CurrentProcessingQueryNode.Arguments.Add(preparedArgumentNode);

            base.ExitAddArgument(context);
        }

        public override void EnterAddArgumentSubject(QueryLanguageParser.AddArgumentSubjectContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgumentSubject);

            base.EnterAddArgumentSubject(context);
        }

        public override void ExitAddArgumentSubject(QueryLanguageParser.AddArgumentSubjectContext context)
        {
            processingObjectTypeStack.Pop();

            base.ExitAddArgumentSubject(context);
        }

        public override void EnterAddArgumentValue(QueryLanguageParser.AddArgumentValueContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgumentValue);

            var constantValue = context.typeValue()?.GetValue();
            CurrentProcessingQueryNode.ArgumentValueConstant = constantValue;

            base.EnterAddArgumentValue(context);
        }

        public override void ExitAddArgumentValue(QueryLanguageParser.AddArgumentValueContext context)
        {
            processingObjectTypeStack.Pop();

            base.ExitAddArgumentValue(context);
        }

        #endregion

        #region ToMd

        // Handled by Generic String value argument

        #endregion

        #region ToT

        // Handled by Generic String value argument

        #endregion

        #region Take

        // Handled by Generic Int value argument

        #endregion

        #region Skip

        // Handled by Generic Int value argument

        #endregion

        #region Generic Int value argument

        public override void EnterGenericIntValueArgument(QueryLanguageParser.GenericIntValueArgumentContext context)
        {
            StartQueryNode(QueryNodeType.MethodArgument, ProcessingObjectType.MethodArgument);

            var valueConstant = int.Parse(context.value.GetText());

            CurrentProcessingQueryNode.ArgumentValueConstant = valueConstant;

            base.EnterGenericIntValueArgument(context);
        }

        public override void ExitGenericIntValueArgument(QueryLanguageParser.GenericIntValueArgumentContext context)
        {
            FinishQueryNode();

            base.ExitGenericIntValueArgument(context);
        }

        #endregion

        #region Generic String value argument

        public override void EnterGenericStringValueArgument(QueryLanguageParser.GenericStringValueArgumentContext context)
        {
            StartQueryNode(QueryNodeType.MethodArgument, ProcessingObjectType.MethodArgument);

            var valueConstant = context.value.GetText().Trim('\'').Trim('"');

            CurrentProcessingQueryNode.ArgumentValueConstant = valueConstant;

            base.EnterGenericStringValueArgument(context);
        }

        public override void ExitGenericStringValueArgument(QueryLanguageParser.GenericStringValueArgumentContext context)
        {
            FinishQueryNode();

            base.ExitGenericStringValueArgument(context);
        }

        #endregion
        
        #region Generic Query value argument

        public override void EnterGenericQueryValueArgument(QueryLanguageParser.GenericQueryValueArgumentContext context)
        {
            StartQueryNode(QueryNodeType.MethodArgument, ProcessingObjectType.MethodArgument);

            // Indicate we are going to gather argument values
            processingObjectTypeStack.Push(ProcessingObjectType.MethodArgumentValue);

            base.EnterGenericQueryValueArgument(context);
        }

        public override void ExitGenericQueryValueArgument(QueryLanguageParser.GenericQueryValueArgumentContext context)
        {
            // Indicate we finished to gather argument values
            processingObjectTypeStack.Pop();

            FinishQueryNode();

            base.ExitGenericQueryValueArgument(context);
        }

        #endregion

        #endregion

        #region Pivots

        public override void EnterPivotExpression(QueryLanguageParser.PivotExpressionContext context)
        {
            var pivotType = ParserHelper.GetPivotType(context.pivotType?.Type ?? -1);

            var pivotDefinition = ParserHelper.GetPivot(pivotType);
            CurrentProcessingQueryNode.PivotData = new PivotData
            {
                PivotDefinition = pivotDefinition
            };

            CurrentProcessingQueryNode.Name = pivotType.ToString();

            base.EnterPivotExpression(context);
        }

        public override void ExitPivotExpression(QueryLanguageParser.PivotExpressionContext context)
        {
            base.ExitPivotExpression(context);
        }

        public override void EnterPivotValues(QueryLanguageParser.PivotValuesContext context)
        {
            var pivotValue1 = context.pivotValue1.Text;
            var pivotValue2 = context.labelChainCollection()?.labelChain();

            if (pivotValue1 != null)
                CurrentProcessingQueryNode.PivotData.MainValue = pivotValue1;
            if (pivotValue2 != null && pivotValue2.Any())
                CurrentProcessingQueryNode.PivotData.SecondaryValues.AddRange(pivotValue2.Select(x => x.GetText()));

            base.EnterPivotValues(context);
        }

        public override void ExitPivotValues(QueryLanguageParser.PivotValuesContext context)
        {
            base.ExitPivotValues(context);
        }

        #endregion

        #region Collections

        public override void EnterCollection(QueryLanguageParser.CollectionContext context)
        {
            CurrentProcessingQueryNode.Type = QueryNodeType.Collection;

            base.EnterCollection(context);
        }

        #region Filters

        #region Filter group

        public override void EnterFilterExpressionGroup(QueryLanguageParser.FilterExpressionGroupContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.ConditionGroup);
            var appender = ParserHelper.GetCriteriaAppendType(context.appenderType.Type);

            var newCriteriaGroup = new QueryNode
            {
                Type = QueryNodeType.CriteriaGroup,
                Appender = appender,
            };

            processingQueryNodeStack.Push(newCriteriaGroup);
            base.EnterFilterExpressionGroup(context);
        }

        public override void ExitFilterExpressionGroup(QueryLanguageParser.FilterExpressionGroupContext context)
        {
            processingObjectTypeStack.Pop();

            var preparedConditionGroup = processingQueryNodeStack.Pop();
            CurrentProcessingQueryNode.Criterias.Add(preparedConditionGroup);

            base.ExitFilterExpressionGroup(context);
        }

        #endregion

        public override void EnterFilterCondition(QueryLanguageParser.FilterConditionContext context)
        {
            var notModifier = context.notModifier != null;
            var comparator = context.comparator?.Type;

            var condition = new QueryNode
            {
                Type = QueryNodeType.Criteria,
                Comparator = comparator.HasValue ? ParserHelper.GetCriteriaComparator(comparator.Value) : (CriteriaComparator?)null,
                NotModifier = notModifier,
            };

            // Not Equals workaround
            if (condition.Comparator == CriteriaComparator.NotEq)
            {
                condition.Comparator = CriteriaComparator.Eq;
                condition.NotModifier = !condition.NotModifier;
            }

            // Processed in EnterFilterConditionSubject
            // condition.CriteriaSubjectPath.AddRange(subjectPath);
            processingQueryNodeStack.Push(condition);
            base.EnterFilterCondition(context);
        }

        public override void ExitFilterCondition(QueryLanguageParser.FilterConditionContext context)
        {
            var preparedCondition = processingQueryNodeStack.Pop();
            CurrentProcessingQueryNode.Criterias.Add(preparedCondition);

            base.ExitFilterCondition(context);
        }

        public override void EnterFilterConditionValue(QueryLanguageParser.FilterConditionValueContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.ConditionValue);

            var valueConstant = context.typeValue()?.GetValue();
            CurrentProcessingQueryNode.CriteriaValueConstant = valueConstant;

            base.EnterFilterConditionValue(context);
        }

        public override void ExitFilterConditionValue(QueryLanguageParser.FilterConditionValueContext context)
        {
            processingObjectTypeStack.Pop();

            base.ExitFilterConditionValue(context);
        }

        public override void EnterFilterConditionSubject(QueryLanguageParser.FilterConditionSubjectContext context)
        {
            processingObjectTypeStack.Push(ProcessingObjectType.ConditionSubject);

            base.EnterFilterConditionSubject(context);
        }


        public override void ExitFilterConditionSubject(QueryLanguageParser.FilterConditionSubjectContext context)
        {
            processingObjectTypeStack.Pop();

            base.ExitFilterConditionSubject(context);
        }

        #endregion

        #endregion

        #region Projections

        public override void EnterProjection(QueryLanguageParser.ProjectionContext context)
        {
            var newQueryNode = StartQueryNode(QueryNodeType.Projection, ProcessingObjectType.Node);

            // TODO: Not sure we need it twice, maybe move to StartQueryNode call
            processingObjectTypeStack.Push(ProcessingObjectType.Projection);

            base.EnterProjection(context);
        }

        public override void ExitProjection(QueryLanguageParser.ProjectionContext context)
        {
            // TODO: Not sure we need it twice, maybe move to StartQueryNode call
            processingObjectTypeStack.Pop();

            FinishQueryNode();

            base.ExitProjection(context);
        }

        #endregion
    }
}