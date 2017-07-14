using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Compatibility;
using Platform.Template.Core.Services;
using Platform.Utils.Events.Domain.Objects;
using Platform.Utils.Events.Manager;
using Platform.Utils.Events.Manager.Helpers;
using Platform.Utils.Ioc;
using Platform.Utils.Json;

namespace Platform.Template.Tests
{
    public class DelegatesTest : TestBase
    {
        private ProxyMock proxyMock;
        private StorageProvider storageProvider;
        private  ModelsHelper modelsHelper;


        private  InfraService infraService;

        public override void SetUp()
        {
            base.SetUp();
            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
            this.storageProvider = IocContainerProvider.CurrentContainer.GetInstance<StorageProvider>();
            this.modelsHelper = IocContainerProvider.CurrentContainer.GetInstance<ModelsHelper>();
            this.infraService = IocContainerProvider.CurrentContainer.GetInstance<InfraService>();
        }


        [Test]
        public void CreateTest()
        {
            var st = new Stopwatch();
            st.Start();

            for (var i = 0; i < 1000; i++)
            {
                JObject tr = new JObject()
                {
                    ["newPerfTest"] = new JObject()
                    {
                        ["vvx.Test"] = "asdasd"
                    }
                };

                var modelInfo = this.storageProvider.GetModelElementTree("newPerfTest");
                proxyMock.ServiceContainer.Create(modelInfo, tr);
            }

            st.Stop();
            var res = st.ElapsedMilliseconds;
            var k = 1 + 1;

        }
    }
}
