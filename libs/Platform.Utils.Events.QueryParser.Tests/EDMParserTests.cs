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

    public class EDMParserTests
    {
        #region Data sources

        public JObject jobj = JObject.Parse(@"
{
  ""shipment"":
  {
    ""id"" : ""11111111-631C-41E9-8986-74D2F13D61A7"",
    ""vx.xxx"": 123,
    ""ae.comments"": 
    [
      {
        ""id"": ""22222222-631C-41E9-8986-74D2F13D61A7""
      }
    ]
  }
}
");

        #endregion

        [Test]
        public void Test()
        {
            var parser = new EDMObjectParser();

//            var result = parser.Parse(jobj_small);
            var result = parser.Parse(this.jobj);

            var q = result.GetQueryLanguageString();
        }
    }
}