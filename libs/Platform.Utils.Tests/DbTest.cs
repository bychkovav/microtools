using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Tests
{
    using System.Xml.Linq;
    using Cassandra;
    using global::Cassandra.Mapping;
    using Ioc;
    using NUnit.Framework;
    using ServiceBus.CassandraRetry;

    [TestFixture]
    public class DbTest : TestBase
    {
        [Test]
        public void TaskCustom()
        {
            var items = new[]
            {
                    new XElement(XName.Get("package"), new XAttribute("id", "Test.Ult.fe"),
                        new XAttribute("version", "1.1.0.0"), new XAttribute("targetFramework", "net45")),
                    new XElement(XName.Get("package"), new XAttribute("id", "Test.Ult.fe"),
                        new XAttribute("version", "1.1.0.0"), new XAttribute("targetFramework", "net45")),
                };
            var filteredItems = items.Select(
                x =>
                    new
                    {
                        id = x.Attribute("id").Value,
                        version = x.Attribute("version").Value,
                        targetFramework = x.Attribute("targetFramework").Value
                    }).GroupBy(x => x.id).Select(x => x.OrderByDescending(y => y.version).First()).ToList();

        }

        [Test]
        public void CreateDb()
        {
            var provider = IocContainerProvider.CurrentContainer.GetInstance<AttemptDataProvider>();

            provider.Session.Execute("DROP COLUMNFAMILY send_attempt");
            provider.Session.Execute("DROP COLUMNFAMILY not_sent_attempt");

            provider.Session.Execute(
                 "CREATE COLUMNFAMILY send_attempt ("
                 + "retrymessageid uuid,"
                 + "id uuid,"
                 + "name text,"
                 + "specificroute text, "
                 + "exchangename text, "
                 + "messagejson text, "
                 + "messageheadersjson text, "
                 + "createdate timestamp, "
                 + "data text,"
                 + "status int,"
                 + "PRIMARY KEY (retrymessageid, createdate));");

            provider.Session.Execute(
                 "CREATE COLUMNFAMILY not_sent_attempt ("
                 + "retrymessageid uuid,"
                 + "id uuid,"
                 + "name text,"
                 + "specificroute text, "
                 + "exchangename text, "
                 + "messagejson text, "
                 + "messageheadersjson text, "
                 + "createdate timestamp, "
                 + "data text,"
                 + "messagetype text,"
                 + "PRIMARY KEY (retrymessageid));");

        }

    }
}
