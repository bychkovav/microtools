namespace Platform.Utils.Events.QueryParser.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Builders.MongoDb;
    using Builders.Object;
    using Builders.QueryLanguage;
    using Domain.Enums;
    using Domain.Objects;
    using Extensions;
    using Extensions.Fluent;
    using Extensions.Preprocessors;
    using MongoDb;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Serializers;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NUnit.Framework;

    public class BasicTests
    {
        [Test]
        public void BasicTest()
        {
            var query = @"$x().bp.ac.delivery()._Add()";
            var engine = new Engine();
            var qlBuilder = new QueryLanguageBuilder();
            var singleQuery = engine.Parse(query);
            singleQuery.PivotsToCriterias(true);

            singleQuery.NodesList.First.Next.Value.PivotData.GetPivotValues();

            var text = qlBuilder.RenderQuery(singleQuery);

            Console.WriteLine(text);
        }

        [Test]
        public void ModificationInfoTest()
        {
            var query = @"$x.job.ae.notes(id = 11111111-1111-1111-1111-111111111111)._Add(vx.name = 'Lukas')";
            var engine = new Engine();
            var qlBuilder = new QueryLanguageBuilder();
            var singleQuery = engine.Parse(query);

            singleQuery.GenerateIds();

            var newQueries = singleQuery.GetModificationInfoQueries(JObject.Parse("{'xxx': 'zzz'}"), false);

            var text = qlBuilder.RenderQuery(singleQuery);

            Console.WriteLine(text);
        }

        public void ConvertIdToMasterIdTest()
        {
            var query = @"$x.job(id = 00000000-0000-0000-0000-000000000000).ae.notes(id = 11111111-1111-1111-1111-111111111111)._Get()";
            var engine = new Engine();
            var qlBuilder = new QueryLanguageBuilder();
            var singleQuery = engine.Parse(query);

            singleQuery.ConvertIdToMasterId();

            var text = qlBuilder.RenderQuery(singleQuery);

            Console.WriteLine(text);
        }

        [Test]
        public void GenerateIdsTest()
        {
            var query = @"$x.ae.notes()._add(id = 064ADE4C-3BE6-4377-87CC-9B30329C1B6B)";
            var engine = new Engine();
            var qlBuilder = new QueryLanguageBuilder();
            var singleQuery = engine.Parse(query);

            singleQuery.GenerateIds();

            var text = qlBuilder.RenderQuery(singleQuery);

            Console.WriteLine(text);
        }

        [Test]
        public void ObjectBuilderTest()
        {
            var queries = new List<string>()
            {
                $"$t.job(id = '064ADE4C-3BE6-4377-87CC-9B30329C1B6B').ae.note<document>(id = '064ADE4C-1111-4377-87CC-9B30329C1B6B')._Set(vx.name = 'xxx')",
                $"$t.job(id = '064ADE4C-3BE6-4377-87CC-9B30329C1B6B').ae.note<document>(id = '064ADE4C-1111-4377-87CC-9B30329C1B6B')._Set(vx.lastName = 'yyy')",
                $"$t.job(id = '064ADE4C-3BE6-4377-87CC-9B30329C1B6B').ae.note<document>()._Add(vx.lastName = 'yyy')",
            };
            var queryList = new List<SingleQuery>();
            foreach (var query in queries)
            {
                queryList.Add(new Engine().Parse(query));
            }

            var result = new ObjectBuilder().Build(queryList);

        }

        [Test]
        public void QuerySeparationTest()
        {
            var engine = new Engine();

            var queryBatch = @"
firstApiTest(Id='7a815255-58e7-42e2-87db-e04baf9a3920')._Set(vx.firstName = 'Test_123', vx.lastName='Test_234')
firstApiTest(Id='7a815255-58e7-42e2-87db-e04baf9a3920').ae.note<documents>()._Add(vvx.fileSize=1234, vvx.fileName='xxx.gif')
$x.ae()._Get()
";
            var queryList = engine.GetQueries(queryBatch);
            queryList.ForEach(Console.WriteLine);
        }

        [Test]
        public void GetTransactionIdTest()
        {
            var testId = "7a815255-58e7-42e2-87db-e04baf9a3920";
            var engine = new Engine();

            var query = $"$T.firstApiTest(Id='{testId}')._Set(vx.firstName = 'Test_123', vx.lastName='Test_234')";
//            var singleQuery = engine.Parse(query);
            var singleQuery = SingleQuery.CreateQuery
                  .RootCollection(QuerySource.API)
                    .SetPivot(PivotType.Transaction, "job")
                    .AddCriteria(CriteriaAppendType.And, "Id", CriteriaComparator.Eq, Guid.Parse(testId));
            var id = singleQuery.MakeCopy().GetTransactionId();

            Assert.AreEqual(id, Guid.Parse($"{testId}"));
        }

        [Test]
        public void QueryAndSubquery()
        {
            var firstQuery = SingleQuery.CreateQuery
                .RootCollection(QueryRootType.Model, node => node.Name = "job");
                //.AddCollection("ae")
                

            var secondQuery = SingleQuery.CreateQuery
                .RootCollection(QueryRootType.Model, node => node.Name = "job2")
                .AddCollection("ae")
                .SetPivot(PivotType.Attributes, "yourMomsFriends");

            firstQuery.AddMethod(QueryMethodType.Add, x => x.ArgumentValueQuery = secondQuery).SetPivot(PivotType.Attributes, "yourMomsFriends"); 

            var builder = new QueryLanguageBuilder();
            var query = builder.RenderQuery(firstQuery);
        }

        [Test]
        public void GetTransactionNameTest()
        {
            var testName = "job";
            var engine = new Engine();

            var query = $"$T.{testName}()._Set(vx.firstName = 'Test_123', vx.lastName='Test_234')";
            var name = engine.Parse(query).GetTransactionName();

            Assert.AreEqual(name, testName);
        }

        [Test]
        public void ApiSetQueriesTests()
        {
            var singleQuery = SingleQuery.CreateQuery
                .RootProperty(QuerySource.API)
                .SetPivot(PivotType.Transaction, "job")
                // TODO: review and fix it according to TS logic
                /* no more EE collection. Now TS objects
                                .AddCollection("ee")
                                .SetPivot(PivotType.Task, "pickup")
                                .MethodAdd()
                                .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty("capacity"), x => x.ArgumentValueConstant = "111")
                                .AddArgument(x => x.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty("vx").SetPivot(PivotType.Value, "text"), x => x.ArgumentValueConstant = 123)
                */
                //.AddArgument(
                //    x => x.ArgumentValueQuery = SingleQuery.CreateQuery
                //            .AddCollection("ae")
                //            .SetPivot(PivotType.Attributes, "aeCode", "PropertyCode")
                //            .MethodSet()
                //            .AddArgument(
                //                    x1 => x1.ArgumentSubjectQuery = SingleQuery.CreateQuery.AddProperty("vx").SetPivot(PivotType.Value, "text"), 
                //                    x1 => x1.ArgumentValueConstant = 123
                //                    )
                //            )
                ;

            var builder = new QueryLanguageBuilder();

            var query = builder.RenderQuery(singleQuery);

            Console.WriteLine(query);
        }

        [Test]
        public void ApiGetQueriesTests()
        {
            var singleQuery = SingleQuery.CreateQuery
                .RootCollection(QueryRootType.Variable, x => x.Name = "T")
                .SetPivot(PivotType.Transaction, "job")
                .AddCriteria(CriteriaAppendType.And, "Id", CriteriaComparator.Eq, "42")
                // TODO: review and fix it according to TS logic
                /* no more EE collection. Now TS objects
                                .AddCollection("ee")
                                .SetPivot(PivotType.Task, "pickup")
                                .AddCriteria(CriteriaAppendType.And, "Id", CriteriaComparator.Eq, "42")
                */
                .AddCollection("md").SetPivot(PivotType.MasterData, "company", "owner")
                .AddCriteria(CriteriaAppendType.And, "Id", CriteriaComparator.Eq, "42")
                .AddProperty("vx")
                .SetPivot(PivotType.Value, "something")
                ;

            var builder = new QueryLanguageBuilder();

            var query = builder.RenderQuery(singleQuery);

            Console.WriteLine(query);
        }

        [Test]
        public void QueryBuilderTest()
        {
            var engine = new Engine();
            var builder = new QueryLanguageBuilder();

//            var originalQuery = "job.ae.notes(vx.color = 'red')";
//            var originalQuery = "job.ae.notes((vx.name = 'xxx' && (vx.age = 11 || cc=33)) || vx.color = 'red')";
            var originalQuery = "job(vx.color = 'red' && ae.notes(vx.name = 'xxx'))";
            var singleQuery = engine.Parse(originalQuery);

            var query = builder.RenderQuery(singleQuery);

            Console.WriteLine(query);
        }

        [Test]
        public void JsonModificationTests()
        {
            var json = @"{ 'z.z' : 1 }";


            var jobj = JsonConvert.DeserializeObject<JObject>(json);



            var ss = new JsonSerializerSettings();
//            ss.Converters.Add(new DotInNamesConverter());
            var mJobj = JsonConvert.SerializeObject(jobj, ss);

            var bson = BsonDocument.Parse(mJobj);

            var mongoDataProvider = new MongoDataProvider("mongodb://dmz.mongo1.test.domination.win:27017/test");
            var collection = mongoDataProvider.GetCollection<BsonDocument>("test");
            collection.InsertOne(bson);

            //            var ser = JsonConvert.SerializeObject(), Formatting.Indented, new JsonSerializerSettings {ContractResolver = new ShouldSerializeContractResolver()});

        }

        [Test]
        public void MongoFilterBuilderTest()
        {
            // var query = @"$jobj.job().ee.Pickup<>().ae.notes<general>()._Add(vx.dataAdded = 'xxx', md()._add(vx.text = 'zzz'))";
//            var query = @"$jobj.job(ae.instructions<driver>() && hd.tags(vx.name = 'jahahah') && ae.instructions<driver>().ae.comments<early>(vx.isRead = 'true'))";
            // var query = @"$jobj.job(ae.instructions<driver>().ae.comments<early>(vx.isRead = 'true') && ae.instructions<driver>().md.company<shipper>(vx.name = 'GE Transportation'))";
            //var query = @"$jobj.user(vx.firstName = 'New' && (id = 'f90d36be-7d10-4ed1-9753-5ea7ea87ea46' || vx.lastName = 'Name1'))";
            var query = @"$jobj.user(vmd.user<driver>.vx.text = 'xxx')";
            var builder = new MongoFilterBuilder<BsonDocument>();


//            var mongoDataProvider = new MongoDataProvider("mongodb://dmz.mongo1.test.domination.win:27017/main");
            var mongoDataProvider = new MongoDataProvider("mongodb://demo.mongo1.domination.win:27017/masterdb_d00ffca1_07ad_4c0a_a984_89f6a7a75d15");
            var collection = mongoDataProvider.GetCollection<BsonDocument>("transactions");

            var documentsFinder = builder.Build(query, collection);
            
            var documents = documentsFinder.ToList();

            JsonConvert.DefaultSettings = () =>
            {
                var defaultSettings = new JsonSerializerSettings();
//                defaultSettings.Converters.Add(new ObjectIdConverter());
                defaultSettings.Converters.Add(new BsonObjectIdConverter());
//                defaultSettings.Converters.Add(new DotInNamesConverter());
                return defaultSettings;
            };


            var y = JsonConvert.DeserializeObject<JObject>(documents[0].ToJson());


            Console.WriteLine(documents);
        }

        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);


                return property;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization);
            }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                return base.CreateObjectContract(objectType);
            }
        }

        public class BSCustom : BsonDocumentSerializer
        {
            protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, BsonDocument value)
            {
                ToM(value);
                base.SerializeValue(context, args, value);
            }

            private void ToM(BsonDocument doc)
            {
                int elementCount = doc.ElementCount;
                for (int index = 0; index < elementCount; ++index)
                {
                    BsonElement element = doc.GetElement(index);
                    var value = element.Value;
                    if (value is BsonDocument)
                        ToM(value.AsBsonDocument);

                    var newElement = new BsonElement(element.Name.Replace("##", "."), element.Value);
                    doc.SetElement(index, newElement);
                }
            }

            public override BsonDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return base.Deserialize(context, args);
            }
        }

    }
}