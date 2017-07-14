using System;

namespace Platform.Utils.Events.QueryParser.Builders.JsonLinq.Extensions
{
    public static class PredicateExtensions
    {
        public static Func<T, bool> Not<T>(this Func<T, bool> predicate)
        {
            return a => !predicate(a);
        }

        public static Func<T, bool> And<T>(this Func<T, bool> left, Func<T, bool> right)
        {
            return a => left(a) && right(a);
        }

        public static Func<T, bool> Or<T>(this Func<T, bool> left, Func<T, bool> right)
        {
            return a => left(a) || right(a);
        }

    }
}
