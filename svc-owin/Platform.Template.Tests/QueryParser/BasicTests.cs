namespace Platform.Template.Tests.QueryParser
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Utils.Events.QueryParser;
    using Utils.Events.QueryParser.Builders.JsonLinq;
    using Utils.Events.QueryParser.Builders.Object;
    using Utils.Events.QueryParser.Extensions;
    using Utils.Events.ScriptEngine;
    using Utils.Ioc;
    using Utils.Json;

    public class BasicTests : TestBase
    {
        private ScriptEngine scriptEngine;
        private ProxyMock proxyMock;

        #region Sources

        public JObject json = JObject.Parse(@"
{
    ""job"" : {
        ""ee"" : [ 
            {
                ""ee"" : [],
                ""code"" : ""delivery"",
                ""ae.attachments"" : [],
                ""ae.checkListItems"" : [],
                ""ae.dateTimes"" : [],
                ""ae.notes"" : [],
                ""ae.references"" : [],
                ""md.companies"" : [],
                ""md.locations"" : []
            }, 
            {
                ""ee"" : [],
                ""code"" : ""pickup"",
                ""ae.attachment"" : [],
                ""ae.checkListItem"" : [],
                ""ae.dateTime"" : [],
                ""ae.note"" : [],
                ""ae.reference"" : [],
                ""md.company"" : [],
                ""md.location"" : []
            }
        ],
        ""objectCode"" : ""job"",
        ""ae.addresses"" : [],
        ""ae.attachments"" : [],
        ""ae.checkListItems"" : [],
        ""ae.dateTimes"" : [],
        ""ae.notes"" : [ 
            {
                ""ee"" : [],
                ""objectCode"" : ""note"",
                ""md.users"" : [],
                ""code"" : ""hotChicka"",
                ""id"" : ""35fdf34a-aabc-4739-90a4-d20cf9ef96b3"",
                ""vx.text"" : ""text 99""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""note"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 2"",
                ""id"" : ""6fba99bc-974c-47ba-92c9-7ffa65c1d5ee""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""note"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 3"",
                ""id"" : ""6d7921c9-5cd5-49d6-81cb-dde007eb5018""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""note"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 4"",
                ""id"" : ""c6cc8afa-76a8-4150-a9b7-1cf9d67baad1""
            }
        ],
        ""ae.references"" : [],
        ""md.companies"" : [],
        ""md.users"" : [],
        ""localMasterId"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""globalMasterId"" : ""c54ac468-879d-46f9-86ab-44b8f078abca"",
        ""localOwnerId"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""globalOwnerId"" : ""c54ac468-879d-46f9-86ab-44b8f078abca"",
        ""id"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""revision"" : 0
    },
    ""propsChanged"" : []
}");

        public JObject json_iSawYourMom = JObject.Parse(@"
{
    ""iSawYourMom"" : {
        ""ee.sundayDinner"" : [ 
            {
                ""ee"" : [],
                ""code"" : ""boughtFlowers"",
                ""ae.attachments"" : [],
                ""ae.checkListItems"" : [],
                ""ae.dateTimes"" : [],
                ""ae.notes"" : [],
                ""ae.references"" : [],
                ""md.companies"" : [],
                ""md.locations"" : []
            }, 
            {
                ""ee"" : [],
                ""code"" : ""ateDinner"",
                ""ae.attachment"" : [],
                ""ae.checkListItem"" : [],
                ""ae.dateTime"" : [],
                ""ae.note"" : [],
                ""ae.reference"" : [],
                ""md.company"" : [],
                ""md.location"" : []
            }
        ],
        ""objectCode"" : ""iSawYourMom"",
        ""ae.yourMomsFriends"" : [ 
            {
                ""ee"" : [],
                ""objectCode"" : ""yourMomsFriend"",
                ""md.users"" : [],
                ""code"" : ""hotChicka"",
                ""id"" : ""35fdf34a-aabc-4739-90a4-d20cf9ef96b3"",
                ""vx.text"" : ""text 99""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""yourMomsFriend"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 2"",
                ""id"" : ""6fba99bc-974c-47ba-92c9-7ffa65c1d5ee""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""yourMomsFriend"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 3"",
                ""id"" : ""6d7921c9-5cd5-49d6-81cb-dde007eb5018""
            }, 
            {
                ""ee"" : [],
                ""objectCode"" : ""yourMomsFriend"",
                ""md.users"" : [],
                ""code"" : null,
                ""vx.text"" : ""text 4"",
                ""id"" : ""c6cc8afa-76a8-4150-a9b7-1cf9d67baad1""
            }
        ],
        ""ae.references"" : [],
        ""md.companies"" : [],
        ""md.users"" : [],
        ""localMasterId"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""globalMasterId"" : ""c54ac468-879d-46f9-86ab-44b8f078abca"",
        ""localOwnerId"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""globalOwnerId"" : ""c54ac468-879d-46f9-86ab-44b8f078abca"",
        ""id"" : ""471533a2-1c26-48cc-9ea6-2883d8a0c4f4"",
        ""revision"" : 0
    },
    ""propsChanged"" : []
}");

        #endregion

        public override void SetUp()
        {
            base.SetUp();
            this.scriptEngine = IocContainerProvider.CurrentContainer.GetInstance<ScriptEngine>();
            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
        }

        [Test]
        public void Test()
        {
            //            var queryString = "eData.job().ae.notes(id = 35fdf34a-aabc-4739-90a4-d20cf9ef96b3)._Set(vx.text = 'updated text')";
            //            var queryString = "eData.job().ae.notes<comment>()._Add(vx.text = 'new text')";
            //            var queryString = "eData.job().md.users<personnel>()._Add(id = 2cb72685-ca4c-4971-a017-98796c228490)";
            //            var queryString = "eData.job().ae.notes()._orderBy(vx.text)";
            //            var queryString = "eData.job().ae.notes()[vx.text, id]";
            //            var queryString = "eData.job().ae.notes().id";
            //            var queryString = "\"notes\".ae.notes()._Add(vx.text = 'new text')";
            //            var queryString = "eData.job(objectCode = 'job' && id = 471533a2-1c26-48cc-9ea6-2883d8a0c4f4)";
            //            var queryString = "eData.job().ae.notes(z = 'pp')"; // Not existing field
            //            var queryString = "eData.job().ae.notes<hotChicka>()"; // full pivot search
//            var queryString = "eData.shipment(id = '8b223180-9fcb-4cc7-9da5-3a4d370f7f79').md.users<watcher>()._Add(id = 'bafe4e85-f529-4b94-b3b4-0c5f5c3dd7fa')";
            var queryString = "eData.shipment().bp.ac.delivery()._Add()";

            var j =
                JObject.Parse(@"{""shipment"": {""objectCode"":""shipment"", ""bp"": {""ac"": [{""objectCode"": ""delivery""}, {""objectCode"": ""arrange""}] }}}");

            var query = new Engine().Parse(queryString);

            query.GetQueryLanguageString();

            var zzz = new ObjectBuilder().Build(query);

//            query.GenerateIds();

            var result = JsonLinqExecutor.GetExecutor.Run(new[] { query }, j, (ExpandoObject)this.proxyMock.ServiceContainer);

            var str = result[0].ToString();
        }

        [Test]
        public void WhereTest()
        {
            var queryString = "eData.iSawYourMom().ae.yourMomsFriends((vx.text = 'text 2' || vx.text = 'text 3') && vx.text = 'text 3')._Get()";

            var query = new Engine().Parse(queryString);

            var result = JsonLinqExecutor.GetExecutor.Run(new[] { query }, this.json_iSawYourMom, (ExpandoObject)this.proxyMock.ServiceContainer);

            var str = result[0].ToString();
        }

        [Test]
        public void SetTest()
        {
            var queryString = "eData.iSawYourMom(id = 471533a2-1c26-48cc-9ea6-2883d8a0c4f4).ae.yourMomsFriends(id=35fdf34a-aabc-4739-90a4-d20cf9ef96b3)._set(vx.text = 'xxxxxx')"; // event add
            var result = JsonLinqExecutor.GetExecutor.Run(queryString, this.json_iSawYourMom, this.proxyMock.ServiceContainer);

            Console.WriteLine(result[0].ToString());
        }

        [Test]
        public void GetTest()
        {
            var queryString = "eData.iSawYourMom(id = 471533a2-1c26-48cc-9ea6-2883d8a0c4f4).ae.yourMomsFriends(id=35fdf34a-aabc-4739-90a4-d20cf9ef96b3)._Get()"; // get
                                                                                                                                                                 //            var queryString = "eData.iSawYourMom(id = 471533a2-1c26-48cc-9ea6-2883d8a0c4f4).ae.yourMomsFriends(id=35fdf34a-aabc-4739-90a4-d20cf9ef96b3)"; // no get
            var result = JsonLinqExecutor.GetExecutor.Run(queryString, this.json_iSawYourMom, this.proxyMock.ServiceContainer);

            Console.WriteLine(result[0].ToString());
        }

        [Test]
        public void VariablesTest()
        {
            var services = this.proxyMock.ServiceContainer;
            IList<JToken> variable = JsonLinqExecutor.GetExecutor.Run("\"iSawYourMom\"", null, services);
            /*
                        var pd = new PivotData
                        {
                            MainValue = "iSawYourMom",
                            PivotDefinition = ParserHelper.GetPivot(PivotType.Transaction)
                        };
                        var variable = services.Init("iSawYourMom", pd);
            */
            services.Variables = new Dictionary<string, object>();
            services.Variables.Add("variable", variable);
            var result = JsonLinqExecutor.GetExecutor.Run("$variable.iSawYourMom()._Set(vx.test = 'qwerty')", null,
                services);

            Console.WriteLine(result[0].ToString());
        }

        [Test]
        public void InfrastructureFieldsTest()
        {
            var services = this.proxyMock.ServiceContainer;

            var result = JsonLinqExecutor.GetExecutor.Run($"\"iSawYourMom\".iSawYourMom._Set({ObjectHelper.MasterIdPropName} = eData.iSawYourMom<>.id)", this.json_iSawYourMom,
                services);

            Console.WriteLine(result[0].ToString());
        }
    }
}
