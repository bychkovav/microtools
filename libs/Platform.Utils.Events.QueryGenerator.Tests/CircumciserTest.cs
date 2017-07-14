namespace Platform.Utils.Events.QueryGenerator.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator;
    using Platform.Utils.Events.QueryGenerator.Interfaces;
    using Platform.Utils.Ioc;

    [TestFixture]
    public class CircumciserTest
    {
        private IList<ModelElementObjectBase> allClassDescriptions;
        private IList<ModelElementObjectBase> AllClassDescriptions
        {
            get
            {
                if (this.allClassDescriptions == null)
                {
                    var assembly = GetType().Assembly;
                    var resourceName = $"{assembly.GetName().Name}.ShipmentClassDescriptions.txt";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Debug.Assert(stream != null, "stream != null");

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            this.allClassDescriptions = JsonConvert.DeserializeObject<IList<ModelElementObjectBase>>(reader.ReadToEnd());
                        }
                    }
                }

                return this.allClassDescriptions;
            }
        }

        private IDictionary<string, IList<ModelElementObjectBase>> allViewModels;
        private IDictionary<string, IList<ModelElementObjectBase>> AllViewModels
        {
            get
            {
                if (this.allViewModels == null)
                {
                    var assembly = GetType().Assembly;
                    var resourceName = $"{assembly.GetName().Name}.ApiShipmentControllerViewModelDescriptions.txt";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        Debug.Assert(stream != null, "stream != null");

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            this.allViewModels = JsonConvert.DeserializeObject<IDictionary<string, IList<ModelElementObjectBase>>>(reader.ReadToEnd());
                        }
                    }
                }

                return this.allViewModels;
            }
        }

        private Circumciser circumciser;

        [SetUp]
        public void SetUp()
        {
            IocContainerProvider.InitIoc();

            var modelElementStorage = new Moq.Mock<IModelElementStorage>();

            modelElementStorage.Setup(x => x.GetModelElementTree(It.IsAny<string>()))
                .Returns((string x) => AllClassDescriptions.FirstOrDefault(y => y.DisplayText == x));

            modelElementStorage.Setup(x => x.GetModelElementTree(It.IsAny<Guid>()))
                .Returns((Guid x) => AllClassDescriptions.FirstOrDefault(y => y.ModelDefinitionId == x));

            this.circumciser = new Circumciser(modelElementStorage.Object);
        }

        #region Response Text

        private readonly string responseText = @"
[{
    'id': '7ef59eff-377c-4fda-b02e-e3560f71876f',
    'masterId': '7ef59eff-377c-4fda-b02e-e3560f71876f',
    'createdInfo': {
        'author': {
            'firstName': 'Lucas',
            'lastName': 'Kafarski'
        },
        'date': '2016-10-26T09:48:18.6286478Z'
    },
    'updatedInfo': {
        'author': {
            'firstName': 'Lucas',
            'lastName': 'Kafarski'
        },
        'date': '2016-10-26T09:48:18.6286478Z'
    },
    'objectCode': 'shipment',
    'bp': { },
    'ae.comments': [ ],
    'md.locations': [ ],
    'md.organizations': [ ],
    'ae.stops': [ ],
    'md.users': [ ]
},
{
    'id': '2dd6bf60-d925-46eb-a376-948ca2759faa',
    'masterId': '2dd6bf60-d925-46eb-a376-948ca2759faa',
    'createdInfo': {
        'author': {
            'firstName': 'Lucas',
            'lastName': 'Kafarski'
        },
        'date': '2016-10-26T09:52:50.7668019Z'
    },
    'updatedInfo': {
        'author': {
            'firstName': 'Lucas',
            'lastName': 'Kafarski'
        },
        'date': '2016-10-26T09:52:50.7668019Z'
    },
    'objectCode': 'shipment',
    'bp': { },
    'ae.comments': [
        {
            'id': 'f08ccc68-ac56-475f-a94f-35ddb9075d77',
            'createdInfo': {
                'author': {
                    'firstName': 'Lucas',
                    'lastName': 'Kafarski'
                },
                'date': '2016-10-26T09:49:52.0028041Z'
            },
            'objectCode': 'comment',
            'vx.text': 'Test '
        },
        {
            'id': '7a35d081-99ea-4a2a-89b8-3bd9ddf7c917',
            'createdInfo': {
                'author': {
                    'firstName': 'Lucas',
                    'lastName': 'Kafarski'
                },
                'date': '2016-10-26T09:50:53.377259Z'
            },
            'objectCode': 'comment',
            'vx.text': 'Test'
        },
        {
            'id': 'cece00d2-2177-4483-aaba-2a44e1633ccb',
            'createdInfo': {
                'author': {
                    'firstName': 'Lucas',
                    'lastName': 'Kafarski'
                },
                'date': '2016-10-26T09:51:43.173661Z'
            },
            'objectCode': 'comment',
            'vx.text': 'More '
        },
        {
            'id': '4801e72d-2be4-491f-9442-909351f7636f',
            'createdInfo': {
                'author': {
                    'firstName': 'Lucas',
                    'lastName': 'Kafarski'
                },
                'date': '2016-10-26T09:52:50.7668019Z'
            },
            'objectCode': 'comment',
            'vx.text': 'Test1'
        }
    ],
    'vx.displayName': 'abc',
    'md.locations': [ ],
    'md.organizations': [ ],
    'ae.stops': [ ],
    'md.users': [ ]
}]
";

        #endregion

        [Test]
        public void CircumciseAndRemoveDots()
        {
            var inputJson = JArray.Parse(this.responseText);

            var res = this.circumciser.Circumcise(inputJson[1] as JObject, this.AllClassDescriptions.First(), "shipment",
                this.AllViewModels, true);

            res.Should().NotBeNull();

            res["aeComments"].Should().NotBeNull();

            res["aeComments"].Type.Should().Be(JTokenType.Array);

            var comments = res["aeComments"] as JArray;

            comments.Should().NotBeNullOrEmpty();

            comments.Last()["vxText"].ShouldBeEquivalentTo("Test1");
        }

        #region Filter

        private string filter = @"
{
  ""shipment"": {
    ""aeComments"": {
      ""$and"": [
        {
          ""vxText"": {
            ""$cnts"": ""Test""
          },
          ""code"": {
            ""$eq"": ""capacity""
          }
        }
      ]
    }
  }
}
";

        #endregion

        [Test]
        public void AddDotsToFilter()
        {
            var inputJson = JObject.Parse(this.filter);

            var res = this.circumciser.CamelCaseToDotsInFilter(inputJson);

            res.Should().NotBeNull();
        }

    }
}