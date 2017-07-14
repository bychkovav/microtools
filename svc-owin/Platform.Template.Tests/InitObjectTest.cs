namespace Platform.Template.Tests
{
    using Utils.Events.ScriptEngine;
    using Utils.Ioc;

    public class InitObjectTest : TestBase
    {
        private ScriptEngine scriptEngine;
        private ProxyMock proxyMock;

        public override void SetUp()
        {
            base.SetUp();
            this.scriptEngine = IocContainerProvider.CurrentContainer.GetInstance<ScriptEngine>();
            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
        }

        //[Test]
        //public async Task InitDelegateTest()
        //{
        //    var body = "var x = Services.Init(\"userRegistration\");" +
        //               "Services.SetResult(e, x);";

        //    var obj = new RealineObject()
        //    {
        //        Attributes = new List<AttributeRealineObject>()
        //        {
        //            new AttributeRealineObject()
        //            {
        //                Code = "Chlen"
        //            }
        //        }
        //    };

        //    var resultTsk = new TaskCompletionSource<ExecutionResult<dynamic>>();

        //    EventContext ctx = new EventContext()
        //    {
        //        Event = new EventMessage()
        //        {
        //            Content = JsonConvert.SerializeObject(obj),
        //            ContentType = obj.GetType().FullName
        //        },
        //        Data = obj,
        //        ExecutionContext = null,
        //        ResultTsk = resultTsk,
        //    };

        //    var scriptDefinition = new ScriptDefinition()
        //    {
        //        InputData = ctx,
        //        Proxy = this.proxyMock,
        //        Script = body,
        //        ScriptUniqueId = Guid.NewGuid().ToString()
        //    };
        //    var exec = this.scriptEngine.Execute(scriptDefinition);

        //    Assert.IsTrue(exec.Success);

        //    var res = await resultTsk.Task;
        //    Assert.NotNull(res);

        //    body = "var x = Services.Init(\"userRegistration.processing\");" +
        //           "var z = #InputData.T().add(x)#;" +
        //           "Services.SetResult(e, z);";
        //    resultTsk = new TaskCompletionSource<ExecutionResult<dynamic>>();

        //    ctx = new EventContext()
        //    {
        //        Event = new EventMessage()
        //        {
        //            Content = JsonConvert.SerializeObject(obj),
        //            ContentType = res.Value.GetType().FullName
        //        },
        //        Data = res.Value,
        //        ExecutionContext = null,
        //        ResultTsk = resultTsk,
        //    };

        //    scriptDefinition = new ScriptDefinition()
        //    {
        //        InputData = ctx,
        //        Proxy = this.proxyMock,
        //        Script = body,
        //        ScriptUniqueId = Guid.NewGuid().ToString()
        //    };
        //    exec = this.scriptEngine.Execute(scriptDefinition);

        //    Assert.IsTrue(exec.Success);

        //    res = await resultTsk.Task;
        //    Assert.NotNull(res);
        //}
    }
}
