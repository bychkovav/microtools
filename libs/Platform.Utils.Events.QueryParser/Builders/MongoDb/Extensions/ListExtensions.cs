namespace Platform.Utils.Events.QueryParser.Builders.MongoDb.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ListExtensions
    {
        /// <summary> Converts "dots" in names to "##" to use in mongo strorage </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T ConvertForMongo<T>(this T list) where T: class, IEnumerable<string> 
        {
            return list.Select(x => !x.Contains('[') ? x.Replace(".", "##") : x.Trim('[').Trim(']')).ToList() as T;
        }
    }
}
