namespace Platform.Utils.Events.Manager.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Objects;
    using Newtonsoft.Json.Linq;
    using QueryParser.Domain.Objects;
    using QueryParser.Extensions;

    public class EventContextHelper
    {
        public static IList<SingleQuery> GetQueries(MsgContext ctx)
        {
            IList<SingleQuery> result = new List<SingleQuery>();
            if (ctx.Data is JArray)
            {
                foreach (var item in (JArray)ctx.Data)
                {
                    result.Add(GetQuery(item));
                }
            }
            else
            {
                result.Add(GetQuery(ctx.Data));
            }

            return result;
        }

        public static IList<string> GetQueriesJson(MsgContext ctx)
        {
            IList<string> result = new List<string>();
            if (ctx.Data is JArray)
            {
                foreach (var item in (JArray)ctx.Data)
                {
                    result.Add(item.ToString());
                }
            }
            else
            {
                result.Add(ctx.Data.ToString());
            }

            return result;
        }

        public static string GetScriptFromJsonList(IList<string> queriesJson)
        {
            return string.Join(";", queriesJson.Select(x => $"<json>{x}</json>;"));
        }

        private static SingleQuery GetQuery(JToken jQuery)
        {
            var criteriaJson = jQuery.ToString();
            SingleQuery singleQ = criteriaJson.ToSingleQuery();
            return singleQ;
        }
    }
}
