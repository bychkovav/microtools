namespace Platform.Utils.ServiceBus.Helpers
{
    using System;
    using System.Configuration;
    using System.Linq;

    public class ReflectionHelper
    {
        /// <summary>
        /// resolve type of message
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static Type ResolveType(string messageType)
        {
            Type type = Type.GetType(messageType);
            if (type != null)
            {
                return type;
            }

            var mesTypeFullName = messageType.Split(',')[0];
            type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == mesTypeFullName);

            if (type != null)
            {
                return type;
            }

            throw new ConfigurationErrorsException(string.Format("Unknown type [{0}]", messageType));
        }
    }
}
