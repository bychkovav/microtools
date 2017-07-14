namespace Platform.Template.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Utils.Events.QueryParser.Domain.Enums;
    using Utils.Events.QueryParser.Helpers;
    using Utils.Json;

    [TestFixture]
    public class EdmTest : TestBase
    {
        [Test]
        public void SplitEdmTest()
        {
            var edm = JObject.Parse(
              "{" +
                  "\"userMaster\":" +
                      "{" +
                      "\"vvx.FirstName\": \"First\"," +
                      "\"vvx.LastName\": \"Last\"," +
                      "\"vx.Email\":" +
                      "{" +
                          "\"value\":\"test@realine.net\"," +
                          "\"objectCode\":\"Email\"," +
                          "\"vmd.Domain\":" +
                              "{" +
                                  "\"vvx.Name\": \"realine.net\"," +
                                  "\"objectCode\":\"Domain\"," +
                              "}," +
                      "}," +
                      "\"vx.Pasword\":" +
                      "{" +
                          "\"value\":\"111111\"," +
                          "\"objectCode\":\"Pasword\"," +
                      "}," +
                      "\"vx.Phone\":" +
                      "{" +
                          "\"value\":1231245125," +
                          "\"objectCode\":\"Phone\"," +
                      "}," +
                      "\"$operation\": \"initiate\"," +
                  "}" +
              "}"
              );

            var node = new EdmNode();
            edm.TraverseEx((obj, name, state) =>
            {
                if (name == null)
                {
                    return state;
                }

                var pivotData = ParserHelper.GetPivotData(name);
                if (pivotData != null)
                {
                    if (node.NodeValue == null && pivotData.PivotDefinition.Type == PivotType.Transaction)
                    {
                        var wrapper = new JObject();
                        var root = new JObject();
                        //foreach (var notIntent in GetNotIntents(obj))
                        //{
                        //    root.Add(notIntent.Name, notIntent.Value);
                        //}
                        wrapper.Add(name, root);
                        node.NodeValue = wrapper;
                        return node;
                    }
                    else if (pivotData.PivotDefinition.IsIntent)
                    {
                        var jObject = new JObject();
                        jObject.Add(name, obj);
                        //foreach (var notIntent in GetNotIntents(obj))
                        //{
                        //    jObject.Add(notIntent.Name, notIntent.Value);
                        //}
                        var edmNode = new EdmNode(jObject);
                        state.Children.Add(edmNode);
                        return edmNode;
                    }
                    else
                    {
                        state.NodeValue.Add(name, obj);
                    }

                }
                return state;
            }, node);


        }

        //private IEnumerable<JProperty> GetNotIntents(JObject jObject)
        //{
        //    foreach (var child in jObject.Children<JProperty>())
        //    {
        //        if(child.Type)
        //        var pivotData = ParserHelper.GetPivotData(child.Name);
        //        if (pivotData != null && !pivotData.PivotDefinition.IsIntent)
        //        {
        //            yield return child;
        //        }
        //    }
        //}


    }

    public class EdmNode
    {
        public EdmNode()
        {
            Children = new List<EdmNode>();
        }

        public EdmNode(JObject value) : this()
        {
            NodeValue = value;
        }

        public JObject NodeValue { get; set; }

        public IList<EdmNode> Children { get; set; }

        public override string ToString()
        {
            return NodeValue.ToString();
        }
    }

    public static class JTokenEx
    {
        public static void TraverseEx<TState>(this JToken node, Func<JToken, string, TState, TState> action, TState state)
        {
            JProperty parent;
            switch (node.Type)
            {
                case JTokenType.Object:
                    parent = node.Parent as JProperty;
                    var nextState = action(node, parent?.Name, state);

                    foreach (JProperty child in node.Children<JProperty>())
                    {
                        TraverseEx(child.Value, action, nextState);
                    }
                    break;
                case JTokenType.Array:
                    foreach (JToken child in node.Children())
                    {
                        TraverseEx(child, action, state);
                    }
                    break;
                default:
                    parent = node.Parent as JProperty;
                    action(node, parent?.Name, state);
                    break;
            }
        }
    }
}
