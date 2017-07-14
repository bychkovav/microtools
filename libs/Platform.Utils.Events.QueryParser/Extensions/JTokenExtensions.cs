using System.Linq;
using Newtonsoft.Json.Linq;
using Platform.Utils.Events.QueryParser.Domain.Objects;
using Platform.Utils.Events.QueryParser.Helpers;

namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;

    public static class JTokenExtensions
    {
        public static JProperty GetParentProperty(this JToken property)
        {
            if (property.Parent != null)
            {
                var prop = property.Parent as JProperty;
                return prop ?? GetParentProperty(property.Parent);
            }

            return null;
        }

        public static PivotData GetPivotData(this JProperty property)
        {
            var propertyName = property.Name;
            return ParserHelper.GetPivotData(propertyName);
        }
    }
}