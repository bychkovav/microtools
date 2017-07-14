namespace Platform.Utils.Events.QueryGenerator.Extensions
{
    using System;
    using Newtonsoft.Json.Linq;

    public static class JTokenExtensions
    {
        public static object ToPrimitiveObject(this JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.Boolean:
                    return value.Value<bool>();
                case JTokenType.Date:
                    return value.Value<DateTime>();
                case JTokenType.Float:
                    return value.Value<float>();
                case JTokenType.Guid:
                    return value.Value<Guid>();
                case JTokenType.Integer:
                    return value.Value<int>();
                case JTokenType.String:
                    return value.Value<string>();
                default:
                    throw new InvalidOperationException("JToken must be primitive object");
            }
        }
    }
}