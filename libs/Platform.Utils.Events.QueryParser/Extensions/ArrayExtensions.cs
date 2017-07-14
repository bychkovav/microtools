namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;

    public static class ArrayExtensions
    {
        public static void ForEach<T>(this T[] arr, Action<T> action)
        {
            foreach (var elem in arr)
            {
                action(elem);
            }
        }
    }
}