namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Builders.JsonLinq.Extensions;
    using Domain.Enums;
    using Domain.Objects;
    using Helpers;

    public static class PivotDataExtensions
    {
        /// <summary>
        /// Property => Value
        /// </summary>
        /// <param name="pivotData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPivotValues(this PivotData pivotData)
        {
            if (pivotData == null)
                return null;

            var pivotValues = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(pivotData.MainValue))
                pivotValues[pivotData.PivotDefinition.MainProperty] = pivotData.MainValue;

            if (pivotData.SecondaryValues?.Any() == true)
                pivotValues[pivotData.PivotDefinition.SecondaryProperty] = pivotData.SecondaryValues;

            return pivotValues;
        }

        public static List<string> GetJTokenPath(this PivotData pivotData)
        {
            var result = new List<string>();

            var chunks = new List<string>();

            // Do not include pivot name for Transactions
            if (pivotData.PivotDefinition.Type != PivotType.Transaction)
                chunks.Add(ParserHelper.GetPivotName(pivotData.PivotDefinition.Type));

            // Do not include pivot main values for Activity
            if (pivotData.PivotDefinition.Type != PivotType.Activity)
            {
                if (!string.IsNullOrEmpty(pivotData.MainValue))
                    chunks.Add(pivotData.MainValue);
            }

            result.Add(string.Join(".", chunks));

            // Do not include pivot secondary values for Activity
            if (pivotData.PivotDefinition.Type != PivotType.Activity)
            {
                if (pivotData.SecondaryValues.Any())
                    result.Add(pivotData.SecondaryValues.First());
            }

            return result;
        }

        public static string GetJTokenName(this PivotData pivotData, bool escape = true)
        {
            var result = string.Empty;

            var tokens = pivotData.GetJTokenPath().Take(1).ToList();
            tokens = escape ? tokens.EscapeJTokenNames() : tokens;

            result = string.Join(".", tokens);

            return result;
        }


    }
}