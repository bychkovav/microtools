namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions
{
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static List<string> EscapeJTokenNames(this List<string> list)
        {
            var result = new List<string>();
            list.ForEach(x => result.Add($"['{x}']"));
            return result;
        }
    }
}