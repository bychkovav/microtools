using System;

namespace Platform.Utils.MongoDb.Conversion
{
    using Json;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DotInNamesConverter : JsonConverter
    {
        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jobj = (JObject)value;
            var alteredObject = (JObject)jobj.Rename(n => n.Replace(".", "##"));
            alteredObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            serializer.Converters.Remove(this);
            //            var obj = JsonConvert.DeserializeObject<JObject>(reader.ReadAsString());
            var obj = serializer.Deserialize<JObject>(reader);
            serializer.Converters.Add(this);
            var renamedObj = obj.Rename(n => n.Replace("##", "."));
            return renamedObj;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(JObject).IsAssignableFrom(objectType);
        }
    }
}
