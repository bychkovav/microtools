using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Template.Tests
{
    using NUnit.Framework;
    using Utils.Events.ScriptEngine;
    using Utils.Ioc;
    using Utils.Json;

    public class ScriptEngineTest : TestBase
    {
        private ScriptEngine scriptEngine;
        private ProxyMock proxyMock;

        public override void SetUp()
        {
            base.SetUp();
            this.scriptEngine = IocContainerProvider.CurrentContainer.GetInstance<ScriptEngine>();
            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
        }

        [Test]
        public void CompeleTest()
        {
            var c = ObjectHelper.ActivityDependencyPropName;
            this.scriptEngine.CompeleScript(
                @"Services.InvokeCommand(""perfTesting"", ""Initiate"", ""ApiSetResult"", e.Data);", this.proxyMock);
        }
    }
}
