namespace Platform.Template.Tests.Mongo
{
    using MongoDB.Bson;
    using NUnit.Framework;
    using Utils.Events.ScriptEngine;
    using Utils.MongoDb;

    public class MongoBasicTests : TestBase
    {
        private ScriptEngine scriptEngine;
        private ProxyMock proxyMock;

        public override void SetUp()
        {
            base.SetUp();
//            this.scriptEngine = IocContainerProvider.CurrentContainer.GetInstance<ScriptEngine>();
//            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
        }

        [Test]
        public void Test()
        {
            var mongoProvider = new MongoDataProvider("mongodb://dmz.mongo1.test.domination.win:27017/test");
            var coll = mongoProvider.GetCollection<BsonDocument>("testCollection");
            coll.InsertOne(new BsonDocument());
        }
    }
}
