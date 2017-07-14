namespace Platform.Utils.Events.Manager.Helpers
{
    using System;

    public static class TypeHelper
    {
        public static string GetTypeFullName(Type type)
        {
            return $"{type.FullName},{type.Assembly.GetName().Name}";
        }
    }
}
