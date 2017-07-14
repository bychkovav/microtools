namespace Platform.Template.Core.Services
{
    using System;
    using Newtonsoft.Json.Linq;
    using Utils.Events.Domain.Objects;
    using Utils.Json;

    public class InfraService
    {
        public JToken GetModifyByObj(EventContext exct)
        {
            dynamic authorObj = new JObject();
            authorObj.firstName = "Lucas";
            authorObj.lastName = "Kafarski";

            JObject modifyBy = new JObject
            {
                [ObjectHelper.AuthorPropName] = authorObj,
                [ObjectHelper.DatePropName] = DateTime.UtcNow,
                [ObjectHelper.SvcIdPropName] = exct.Event.ProducerId
            };

            return modifyBy;
        }

        public void AddModifyByObj(JToken source, EventContext exct)
        {
            var nodes = source.GetWayFromLeaf();
            var modified = GetModifyByObj(exct);
            while (nodes.Count > 0)
            {
                var currentNode = nodes.Pop() as JProperty;
                if (currentNode != null && !(currentNode.Value is JArray))
                {
                    currentNode.Value[ObjectHelper.UpdatedInfoPropName] = modified;
                }
            }

            source[ObjectHelper.UpdatedInfoPropName] = modified;
        }
    }
}
