namespace Platform.Utils.Events.QueryParser.Builders.QueryLanguage.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Enums;
    using Domain.Objects;
    using Domain.Syntax;
    using Helpers;

    public static class QueryNodeExtensions
    {
        /// <summary> Returns Query Language representation of Query Node name </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        public static string GetNodeName(this QueryNode queryNode)
        {
            var name = string.Empty;
            if (queryNode.RootType.HasValue)
                name = queryNode.GetRootName();
            else
                name = queryNode.PivotData != null ? queryNode.GetPivotExpressionName() : queryNode.Name;

            return name;
        }

        public static string GetRootName(this QueryNode queryNode)
        {
            var result = string.Empty;
            switch (queryNode.RootType)
            {
                case QueryRootType.Variable:
                    result = $"${queryNode.Name}";
                    break;
                case QueryRootType.Model:
                    result = $"\"{queryNode.Name}\"";
                    break;
                case QueryRootType.InputData:
                    result = $"{ParserHelper.GetTokenName(QueryLanguageParser.EData)}";
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (queryNode.PivotData != null)
                result = $"{result}.{queryNode.GetPivotExpressionName()}";

            return result;
        }

        /// <summary> Calculates pivot name representation (e.g. objectCode&lt;capacity&gt;) </summary>
        /// <param name="queryNode"></param>
        /// <returns></returns>
        public static string GetPivotExpressionName(this QueryNode queryNode)
        {
            var chunks = new List<string>();

            if (queryNode.PivotData.PivotDefinition.Type != PivotType.Transaction)
                chunks.Add(ParserHelper.GetPivotName(queryNode.PivotData.PivotDefinition.Type));

            var value = string.Empty;

            if (!string.IsNullOrEmpty(queryNode.PivotData.MainValue))
            {
                value = queryNode.PivotData.MainValue;
            }

            // TODO: review for nested transactions
            if (queryNode.PivotData.SecondaryValues.Any() && queryNode.PivotData.SecondaryValues.All(x => !string.IsNullOrEmpty(x)))
                value = $"{value}<{string.Join(",", queryNode.PivotData.SecondaryValues)}>";

            if (!string.IsNullOrEmpty(value))
                chunks.Add(value);

            return string.Join(".", chunks);
        }

    }
}
