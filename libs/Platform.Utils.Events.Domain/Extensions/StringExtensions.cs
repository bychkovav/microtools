namespace Platform.Utils.Events.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string ToUpperCamelCaseName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            return name.Substring(0, 1).ToUpperInvariant() + name.Substring(1);
        }
    }
}