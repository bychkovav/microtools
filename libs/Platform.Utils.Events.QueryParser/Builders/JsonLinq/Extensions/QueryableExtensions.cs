using System;
using System.Linq;

namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> DoOver<T>(this IQueryable<T> query, Action<T> method)
        {
            var l = query;//.ToList().AsQueryable();
            foreach (var obj in l)
                method(obj);

            return l;
        }

        public static T DoMany<T>(this T item, Action<T> method)
        {
            method(item);
            return item;
        }

    }
}
