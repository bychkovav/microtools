namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders.QueryLanguage;
    using Domain.Objects;
    using Newtonsoft.Json;

    public static class SingleQueryExtensions
    {
        /// <summary> Retreives requested transaction Id </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static Guid GetTransactionId(this SingleQuery singleQuery)
        {

            var id = singleQuery.NodesList.First?.Value.GetQueryNodeId();

            return id ?? Guid.Empty;
        }

        /// <summary> Retreives requested transaction Name (Model name) </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static string GetTransactionName(this SingleQuery singleQuery)
        {
            var name = singleQuery.GetModelInfo()?.MainValue;

            return name;
        }

        /// <summary> Retreives transaction model info (Model name) </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static PivotData GetModelInfo(this SingleQuery singleQuery)
        {
            var modelInfo = singleQuery.NodesList.First?.Value.PivotData;

            return modelInfo;
        }

        /// <summary> Serialize SingleQuery to JSON </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static string Serialize(this SingleQuery singleQuery)
        {
            var jss = new JsonSerializerSettings();
            return JsonConvert.SerializeObject(singleQuery, jss);
        }

        /// <summary> Serialize IEnumerable SingleQuery to JSON </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static string Serialize(this IEnumerable<SingleQuery> singleQuery)
        {
            var jss = new JsonSerializerSettings();
            return JsonConvert.SerializeObject(singleQuery.ToList(), jss);
        }

        /// <summary> Deserialize SingleQuery from JSON </summary>
        /// <param name="jsonSingleQuery"></param>
        /// <returns></returns>
        public static SingleQuery ToSingleQuery(this string jsonSingleQuery)
        {
            var jss = new JsonSerializerSettings();
            return JsonConvert.DeserializeObject<SingleQuery>(jsonSingleQuery, jss);
        }

        public static SingleQuery MakeCopy(this SingleQuery singleQuery)
        {
            return singleQuery.MakeDeepCopy();
        }

        /// <summary> Renders string QueryLanguage representation </summary>
        /// <param name="singleQuery"></param>
        /// <returns></returns>
        public static string GetQueryLanguageString(this SingleQuery singleQuery)
        {
            return new QueryLanguageBuilder().RenderQuery(singleQuery);
        }
    }
}