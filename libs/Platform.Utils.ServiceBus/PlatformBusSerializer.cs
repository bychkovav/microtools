namespace Platform.Utils.ServiceBus
{
    using System;
    using System.Dynamic;
    using EasyNetQ;

    public class PlatformTypeNameSerializer : ITypeNameSerializer
    {
        public Type DeSerialize(string typeName)
        {
            var nameParts = typeName.Split(':');
            if (nameParts.Length != 2)
            {
                throw new EasyNetQException(
                    "type name {0}, is not a valid EasyNetQ type name. Expected Type:Assembly",
                    typeName);
            }
            var type = Type.GetType(nameParts[0] + ", " + nameParts[1]) ?? typeof(ExpandoObject);
            return type;
        }

        public string Serialize(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var typeName = type.FullName + ":" + type.Assembly.GetName().Name;
            if (typeName.Length > 255)
            {
                throw new EasyNetQException("The serialized name of type '{0}' exceeds the AMQP" +
                    "maximum short string lengh of 255 characters.",
                    type.Name);
            }
            return typeName;
        }
    }
}
