namespace Platform.Utils.MongoDb.Conversion
{
    using System.Collections.Generic;
    using Platform.Utils.Json;
    using MongoDB.Bson;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class JObjectExtensions
    {
        public static BsonDocument SerializeToBsonDocument(this JObject jObject)
        {
            var masterId = ObjectHelper.GetMasterId(jObject);

            var jss = new JsonSerializerSettings();
            jss.Converters.Add(new DotInNamesConverter());
            var mJobj = JsonConvert.SerializeObject(jObject, jss);

            var bson = BsonDocument.Parse(mJobj);

            bson.Add(new BsonElement("_id", masterId));

            return bson;
        }

        public static JObject DeserializeToJObject(this BsonDocument bsonDocument)
        {
            bsonDocument.Remove("_id");

            var jss = new JsonSerializerSettings();
            //            jss.Converters.Add(new ObjectIdConverter());
            //            jss.Converters.Add(new BsonObjectIdConverter());
            jss.Converters.Add(new DotInNamesConverter());

            var jobjects = JsonConvert.DeserializeObject<JObject>(bsonDocument.ToJson(), jss);

            return jobjects;
        }

        public static List<JObject> DeserializeToJObject(this IEnumerable<BsonDocument> bsonDocuments)
        {
            var jObjects = new List<JObject>();

            foreach (var bsonDocument in bsonDocuments)
                jObjects.Add(bsonDocument.DeserializeToJObject());

            // Combine results into one JObject
            // var tmpJson = JsonConvert.SerializeObject(jObjects);
            // var jObject = JObject.Parse(tmpJson);

            return jObjects;
        }
    }
}
