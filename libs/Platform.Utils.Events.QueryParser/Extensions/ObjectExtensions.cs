namespace Platform.Utils.Events.QueryParser.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class ObjectExtensions
    {
        public static string ToStringRepresentation(this object value)
        {
            var result = string.Empty;

            if (value == null)
                return "null";

            var typeCode = Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    result = (bool)value ? "true" : "false";
                    break;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    result = value.ToString();
                    break;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    result = value.ToString();
                    break;

                case TypeCode.DateTime:
                    result = $"DateTime.Parse({value.ToString().ToStringRepresentation()})";

                    break;

                case TypeCode.String:
                    result = $"\"{value}\"";
                    break;
                case TypeCode.Char:
                    result = $"\'{value}\'";
                    break;

                case TypeCode.Object:
                    var listValue = value as IEnumerable<dynamic>;
                    if (listValue != null)
                    {
                        result = $"new [] {{ {string.Join(",", listValue.Select(o => ((object)o).ToStringRepresentation()).ToList())} }}";
                    }
                    else
                    {
                        Guid guidValue;
                        var parseResult = Guid.TryParse(value.ToString(), out guidValue);
                        if (parseResult)
                        {
                            result = $"new Guid({guidValue.ToString().ToStringRepresentation()})";
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public static string ToQueryLanguageRepresentation(this object value)
        {
            var result = string.Empty;

            if (value == null)
                return "null";

            var typeCode = Type.GetTypeCode(value.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    result = (bool)value ? "true" : "false";
                    break;

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    result = value.ToString();
                    break;

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    result = value.ToString();
                    break;

                case TypeCode.DateTime:
                    result = value.ToString();

                    break;

                case TypeCode.String:
                    result = $"'{value}'";
                    break;
                case TypeCode.Char:
                    result = $"\'{value}\'";
                    break;

                case TypeCode.Object:
                    var listValue = value as IEnumerable<dynamic>;
                    if (listValue != null)
                    {
                        result = $"[{string.Join(",", listValue.Select(o => ((object)o).ToQueryLanguageRepresentation()).ToList())}]";
                    }
                    else
                    {
                        Guid guidValue;
                        var parseResult = Guid.TryParse(value.ToString(), out guidValue);
                        if (parseResult)
                        {
                            result = $"{guidValue}";
                        }
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public static T MakeDeepCopy<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}