namespace Platform.Utils.Events.QueryGenerator.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;
    using FluentAssertions;
    using FluentAssertions.Json;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Interfaces;
    using Platform.Utils.Ioc;
    using Platform.Utils.Json;

    [TestFixture]
    public class DynamicApiWarderTest
    {
        [SetUp]
        public void SetUp()
        {
            IocContainerProvider.InitIoc();
            
            var modelElementStorage = new Moq.Mock<IModelElementStorage>();

            modelElementStorage.Setup(x => x.GetModelElementTree(It.IsAny<string>()))
                .Returns((string x) => AllClassDescriptions.FirstOrDefault(y => y.DisplayText == x));

            this.dynamicApiWarder = new DynamicApiWarder(modelElementStorage.Object);
        }

        private IList<ModelElementObjectBase> allClassDescriptions;

        private IList<ModelElementObjectBase> AllClassDescriptions
        {
            get
            {
                if (this.allClassDescriptions == null)
                {
                    var assembly = GetType().Assembly;
                    var resourceName = $"{assembly.GetName().Name}.TestshipmentClassDescriptions.txt";

                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Debug.Assert(stream != null, "stream != null");

                        using (var reader = new StreamReader(stream))
                        {
                            this.allClassDescriptions =
                                JsonConvert.DeserializeObject<IList<ModelElementObjectBase>>(reader.ReadToEnd());
                        }
                    }
                }

                return this.allClassDescriptions;
            }
        }

        private DynamicApiWarder dynamicApiWarder;

        [Test]
        public void CantRequestWithoutId()
        {

            
            var res = this.dynamicApiWarder.HandleRequest("testshipment/ae.testAddresses", null, HttpVerbs.Get);

            res.Success.Should().BeFalse();
            res.Errors.Should().ContainSingle()
                .Which.Key.Should().Be("404");
        }

        [Test]
        public void CantRequestWithWrongProperty()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/31b80747-54ba-4120-8b8e-a683016dab28/ae.cAmments",
                null, HttpVerbs.Get);

            res.Success.Should().BeFalse();
            res.Errors.Should().ContainSingle()
                .Which.Key.Should().Be("404");
        }

        [Test]
        public void CreateShipment()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/", new JObject(), HttpVerbs.Post);

            res.Success.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value.EventAction.Should().Be(EventAction.Initiate);
            res.Value.Path.Should().Be("testshipment");
            res.Value.EdmObject.Should().NotBeNull();
            res.Value.EdmObject.Should().HaveElement("testshipment")
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Should().HaveElement("$operation")
                .Which.Should().Be("initiate");
        }

        [Test]
        public void GetShipmentAeComments()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/31b80747-54ba-4120-8b8e-a683016dab28/ae.testAddresses",
                null, HttpVerbs.Get);

            res.Success.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value.EventAction.Should().Be(EventAction.Get);
            res.Value.Path.Should().Be("testshipment.ae.testAddresses");
            res.Value.EdmObject.Should().NotBeNull();
            res.Value.EdmObject.Should().HaveElement("testshipment")
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Should().HaveElement(ObjectHelper.IdPropName)
                .And.HaveElement("ae.testAddresses")
                .Which.Should().BeAssignableTo<JArray>()
                .Which.Children().Should().ContainSingle()
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Children().Should().BeEmpty();
        }

        [Test]
        public void GetShipmentAeCommentsWithCamelCase()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/31b80747-54ba-4120-8b8e-a683016dab28/aeTestAddresses",
                null, HttpVerbs.Get, useDots: false);

            res.Success.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value.EventAction.Should().Be(EventAction.Get);
            res.Value.Path.Should().Be("testshipment.ae.testAddresses");
            res.Value.EdmObject.Should().NotBeNull();
            res.Value.EdmObject.Should().HaveElement("testshipment")
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Should().HaveElement(ObjectHelper.IdPropName)
                .And.HaveElement("ae.testAddresses")
                .Which.Should().BeAssignableTo<JArray>()
                .Which.Children().Should().ContainSingle()
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Children().Should().BeEmpty();
        }


        [Test]
        public void GetSingleShipment()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/31b80747-54ba-4120-8b8e-a683016dab28", null,
                HttpVerbs.Get);

            res.Success.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value.EventAction.Should().Be(EventAction.Get);
            res.Value.Path.Should().Be("testshipment");
            res.Value.EdmObject.Should().NotBeNull();
            res.Value.EdmObject.Should().HaveElement("testshipment")
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Should().HaveElement(ObjectHelper.IdPropName);
        }

        [Test]
        public void QueryJobs()
        {
            var res = this.dynamicApiWarder.HandleRequest("testshipment/", null, HttpVerbs.Get);

            res.Success.Should().BeTrue();
            res.Value.Should().NotBeNull();
            res.Value.EventAction.Should().Be(EventAction.Query);
            res.Value.Path.Should().Be("testshipment");
            res.Value.EdmObject.Should().NotBeNull();
            res.Value.EdmObject.Should().HaveElement("testshipment")
                .Which.Should().BeAssignableTo<JObject>()
                .Which.Children().Should().BeEmpty();
        }
    }
}