namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Objects;

    public static class PivotDefinitionExtensions {
        public static IList<string> GetAllCommonProperties(this PivotDefinition pivotDefinition)
        {
            var result = pivotDefinition.CommonProperties.ToList();

            if (pivotDefinition.MainProperty != null)
            {
                result.Add(pivotDefinition.MainProperty);
            }

            if (pivotDefinition.SecondaryProperty != null)
            {
                result.Add(pivotDefinition.SecondaryProperty);
            }

            return result;
        }
    }
}