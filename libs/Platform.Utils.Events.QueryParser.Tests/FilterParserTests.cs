namespace Platform.Utils.Events.QueryParser.Tests
{
    using Builders.MongoDb;
    using Builders.QueryLanguage;
    using Extensions;
    using Extensions.Preprocessors;
    using FilterationObjectParser;
    using MongoDb;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class FilterParserTests
    {
        #region Data sources

        public JObject jobj = JObject.Parse(@"
{
  ""shipment"":
  {
    ""vx.displayName"":
    {
      ""$eq"": ""abw""
    },
    ""$orderBy"": 
    [
      {
        ""path"": [""shipment"", ""vx.displayName""],
        ""direction"": ""asc""
      }
    ]
  }
}
");


        private JObject jobj0 = JObject.Parse(@"
{
  ""iSawYourMom"": {
    ""ae.Hernja"": {
    ""ae.Notes"": {
      ""vx.DeliveryDate"": {
        ""$eq"": ""2016-10-13T00:00:00+03:00""
      }
    }
    }
  }
}
");

        private JObject jobj1 = JObject.Parse(@"
{
  ""iSawYourMom"": {
    ""ae.Notes"": {
      ""vx.DeliveryDate"": {
        ""$eq"": ""2016-10-13T00:00:00+03:00""
      }
    }
  }
}
");

        private JObject jobj2 = JObject.Parse(@"
{
  ""iSawYourMom"": {
    ""$and"": [
      {
        ""ae.Notes"": {
          ""vx.DeliveryDate"": {
            ""$eq"": ""2016-10-13T00:00:00+03:00""
          }
        }
      },
      {
        ""vx.How"": {
          ""$eq"": ""asdasdasd""
        }
      }
    ]
  }
}
");

        private JObject jobj3 = JObject.Parse(@"
{
  ""iSawYourMom"": {
    ""$and"": [
      {
        ""ae.YourMomsFriends"": {
          ""$and"": [
            {
              ""ae.Comments"": {
                ""vx.Text"": {
                  ""$cnts"": ""test test""
                }
              }
            },
            {
              ""vx.FootSize"": {
                ""$eq"": 221
              }
            },
            {
              ""$or"": [
                {
                  ""vx.FootSize"": {
                    ""$btw"": [
                      3,
                      13
                    ]
                  }
                },
                {
                  ""vx.FootSize"": {
                    ""$eq"": 35
                  }
                }
              ]
            }
          ]
        }
      },
      {
        ""vx.How"": {
          ""$eq"": """"
        }
      }
    ]
  }
}
");
        #endregion

        [Test]
        public void Test()
        {
            var parser = new FilterParser();

//            var result = parser.Parse(jobj_small);
            var result = parser.Parse(this.jobj);

            var q = new QueryLanguageBuilder().RenderQuery(result);
        }

        [Test]
        public void GetFromMongoTest()
        {
            var parser = new FilterParser();

            var result = parser.Parse(this.jobj);
//            var result = parser.Parse(jobj0);
//            var result = parser.Parse(jobj1);
//            var result = parser.Parse(jobj2);
//            var result = parser.Parse(jobj3);

            var builder = new MongoFilterBuilder<BsonDocument>();

            var text = result.GetQueryLanguageString();
            result.ConvertIdToMasterId();

            var mongoDataProvider = new MongoDataProvider("mongodb://demo.mongo1.domination.win:27017/masterdb_db9e00fa_1e68_4663_9bc2_93c7d7794b17");
            var collection = mongoDataProvider.GetCollection<BsonDocument>("transactions");

            var documentsFinder = builder.RenderQuery(result, collection);

            var documents = documentsFinder.ToList();


            var q = new QueryLanguageBuilder().RenderQuery(result);
        }
    }
}