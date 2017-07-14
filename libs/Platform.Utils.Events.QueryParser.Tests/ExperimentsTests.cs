namespace Platform.Utils.Events.QueryParser.Tests
{
    using System;
    using Builders.Object;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    public class ExperimentsTests
    {
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
                ""code"" : null,
                ""id"" : ""35fdf34a-aabc-4739-90a4-d20cf9ef96b3"",
                ""vx.text"" : ""text""
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

        #endregion

        [Test]
        public void Test1()
        {
            var queryString = $"$t.job(id = {Guid.NewGuid()}).ae.notes(id = {Guid.NewGuid()}).ae.data()._Add(vx.data.vx.name = 'zzz', vx.data.vx.age = 48)";

            var jo = new ObjectBuilder().Build(queryString);
        } 

        [Test]
        public void Test2()
        {
//            var queryString = "$t.job().ae.notes(id = 'c6cc8afa-76a8-4150-a9b7-1cf9d67baad1').vx.text";
//            var queryString = "$t.job().ae.notes(id = 'c6cc8afa-76a8-4150-a9b7-1cf9d67baad1')._Set(vx.text = 'new text')";
            var queryString = "$t.job().ae.notes().vx.text._Take(2)";
//            var executor = new JsonLinqExecutor();
//            var builder = new JsonLinqBuilder();

//            var jlinq = builder.Build(queryString);
//            var result = executor.Execute(new[] {queryString}, json);

//            var str = result[0].ToString();
        }
    }
}
