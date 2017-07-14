namespace Platform.Utils.Events.Manager.Helpers
{
    using System;
    using System.Linq;
    using Json;
    using Newtonsoft.Json.Linq;
    using QueryParser.Domain.Enums;
    using QueryParser.Domain.Objects;
    using QueryParser.Extensions;
    using QueryParser.Helpers;

    public class ModelsHelper
    {
        private readonly StorageProvider storageProvider;

        public ModelsHelper(StorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        public JObject Init(string rootCode, PivotData initData)
        {
            var code = initData.SecondaryValues.FirstOrDefault();

            JObject obj = null;
            if (initData.PivotDefinition.Type != PivotType.Activity)
            {
                var model = this.storageProvider.GetRootModel(initData.MainValue);
                if (string.IsNullOrEmpty(model?.JsonStructure))
                {
                    throw new Exception($"No model with name {initData.MainValue}");
                }

                obj = JObject.Parse(model.JsonStructure);
            }
            else
            {
                obj = GetActivity(rootCode, initData);
            }

            JObject result = new JObject();

            var nested = new JObject { };
            var id = Guid.NewGuid();
            nested[ObjectHelper.IdPropName] = id;

            if (initData.PivotDefinition.Type == PivotType.Transaction)
            {
                nested[ObjectHelper.MasterIdPropName] = id;
                nested[ParserHelper.GetPivotName(PivotType.BusinessProcess)] = new JObject()
                {
                    [ParserHelper.GetPivotName(PivotType.Activity)] = new JArray(),
                    [ObjectHelper.ActivityDependencyPropName] = new JArray(),
                    [ObjectHelper.ActivitySequencePropName] = new JArray()
                };

                result[initData.MainValue] = nested;
            }
            else if (initData.PivotDefinition.Type == PivotType.Attributes)
            {
                result[$"{ParserHelper.GetPivotName(PivotType.Attributes)}.{initData.MainValue}"] = nested;
            }
            else if (initData.PivotDefinition.Type == PivotType.Activity)
            {
                result[$"{ParserHelper.GetPivotName(PivotType.Activity)}"] = nested;
            }


            var roots = obj.Properties().ToList();
            if (roots.Count() != 1)
            {
                throw new Exception("There should be just 1 root property!");
            }

            var root = roots.First();
            nested[ObjectHelper.ObjectCodePropName] = root.Name;
            nested[ObjectHelper.CodePropName] = code;

            ParseJObjectLvl(root.Value, nested);

            return result;
        }

        private JObject GetActivity(string rootCode, PivotData initData)
        {
            var rootModel = this.storageProvider.GetRootModel(rootCode);
            if (string.IsNullOrEmpty(rootModel?.JsonStructure))
            {
                throw new Exception($"No root model with name {rootModel}");
            }

            var tree = ObjectHelper.GetInnerObject(JObject.Parse(rootModel.JsonStructure));
            var bpNode = tree[ParserHelper.GetPivotName(PivotType.BusinessProcess)];
            if (bpNode == null)
            {
                throw new Exception("No BP in your model");
            }

            var ac = bpNode[initData.GetJTokenName()];
            if (!(ac is JObject))
            {
                throw new Exception($"No ac with objectCode {initData.MainValue}");
            }

            var res = new JObject { [initData.MainValue] = ac };
            return res;
        }

        private void ParseJObjectLvl(JToken val, JToken mapTo)
        {
            if (val == null)
            {
                return;
            }

            foreach (var prop in val.Values<JProperty>())
            {
                var propName = prop.Name;
                var propVal = prop.Value;
                if (propVal == null)
                {
                    continue;
                }

                if (propName.Contains($"{ParserHelper.GetPivotName(PivotType.Attributes)}.") || propName.Contains($"{ParserHelper.GetPivotName(PivotType.MasterData)}."))
                {
                    mapTo[propName] = new JArray();
                }
                else if (propName.Contains($"{ParserHelper.GetPivotName(PivotType.Task)}."))
                {
                    var taskCode = propName.Split('.').Last();

                    var id = Guid.NewGuid();
                    var nestedTsk = new JObject
                    {
                        [ObjectHelper.IdPropName] = id,
                        [ObjectHelper.TaskCodePropName] = taskCode,
                    };
                    mapTo[propName] = nestedTsk;
                    ParseJObjectLvl(propVal, nestedTsk);
                }
            }
        }
    }
}
