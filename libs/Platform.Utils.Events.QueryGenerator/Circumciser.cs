namespace Platform.Utils.Events.QueryGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Json;
    using Newtonsoft.Json.Linq;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Interfaces;
    using QueryParser.Extensions;
    using QueryParser.Helpers;

    public class Circumciser
    {
        private readonly IModelElementStorage modelElementStorage;

        public Circumciser(IModelElementStorage modelElementStorage)
        {
            this.modelElementStorage = modelElementStorage;
        }

        public JObject Circumcise(JObject obj, ModelElementObjectBase classDescription, string path, 
            IDictionary<string, IList<ModelElementObjectBase>> allViewModels,
            bool excludeDots)
        {
            if (obj == null)
                return null;

            var result = new JObject();

            var pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[classDescription.ModelElementType]);

            foreach (var property in pivot.GetAllCommonProperties())
            {
                var token = obj[property];

                if (token != null)
                {
                    if ((property == ObjectHelper.CreatedInfoPropName || property == ObjectHelper.UpdatedInfoPropName) 
                        && token.Type == JTokenType.String)
                    {
                        token = JToken.Parse(token.Value<string>());
                    }

                    result.Add(property, token);
                }
            }

            var viewModel = allViewModels.Where(kv => path.StartsWith(kv.Key))
                .Select(kv => kv.Value)
                .FirstOrDefault();

            foreach (var property in classDescription.Children)
            {
                if (obj[property.DisplayText] != null)
                {
                    var resultPropertyDisplayText = excludeDots ? property.GetDisplayTextExcludeDots() : property.DisplayText;

                    if (viewModel != null && viewModel.Any(x => x.Id == property.Id) == false)
                    {
                        continue;
                    }

                    ModelElementObjectBase childClassDescription = null;

                    if (property.LinkedModelDefinitionId.HasValue)
                    {
                        childClassDescription = this.modelElementStorage.GetModelElementTree(property.LinkedModelDefinitionId.Value);
                    }

                    if (childClassDescription != null || property.Children.Any())
                    {
                        var childToken = obj[property.DisplayText];

                        if (childClassDescription != null)
                        {
                            childClassDescription = new ModelElementObjectBase(childClassDescription);
                            childClassDescription.Children =
                                childClassDescription.Children.Union(property.Children).ToList();
                            childClassDescription.ModelElementType = property.ModelElementType;
                        }
                        else if (property.Children.Any())
                        {
                            childClassDescription = property;
                        }

                        if (childToken.Type == JTokenType.Array)
                        {
                            var childArray = childToken as JArray;
                            var childArrayRes = new JArray();

                            foreach (var childArrayItem in childArray.Children())
                            {
                                var childObject = childArrayItem as JObject;
                                var childProperty = this.Circumcise(childObject, childClassDescription, path + "." + property.DisplayText, allViewModels, excludeDots);
                                childArrayRes.Add(childProperty);
                            }

                            result.Add(resultPropertyDisplayText, childArrayRes);
                        }
                        else
                        {
                            var childObject = childToken as JObject;

                            var childProperty = this.Circumcise(childObject, childClassDescription, path + "." + property.DisplayText, allViewModels, excludeDots);

                            result.Add(resultPropertyDisplayText, childProperty);
                        }
                    }
                    else
                    {
                        result.Add(resultPropertyDisplayText, obj[property.DisplayText]);
                    }
                }
            }

            var ac = obj["ac"] as JArray;
            if (ac != null)
            {
                var resAc = new JArray();

                foreach (var acItem in ac)
                {
                    if (acItem[ObjectHelper.ObjectCodePropName] != null)
                    {
                        var code = acItem[ObjectHelper.ObjectCodePropName].Value<string>();
                        var acClassDescription =
                            classDescription.Children.FirstOrDefault(
                                x => x.ModelElementType == TreeNodeType.Ac && x.Code == code);

                        if (acClassDescription != null)
                        {
                            var resAcItem = this.Circumcise(acItem as JObject, acClassDescription,
                                path + "." + acClassDescription.DisplayText, allViewModels,
                                excludeDots);

                            resAc.Add(resAcItem);
                        }
                    }
                }

                result.Add("ac", resAc);
            }

            return result;
        }

        public JObject CamelCaseToDots(JObject obj, ModelElementObjectBase classDescription, string path)
        {
            if (obj == null)
                return null;

            var result = new JObject();

            var pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[classDescription.ModelElementType]);

            foreach (var property in pivot.GetAllCommonProperties())
            {
                var token = obj[property];

                if (token != null)
                {
                    if ((property == ObjectHelper.CreatedInfoPropName || property == ObjectHelper.UpdatedInfoPropName)
                        && token.Type == JTokenType.String)
                    {
                        token = JToken.Parse(token.Value<string>());
                    }

                    result.Add(property, token);
                }
            }
            
            foreach (var property in classDescription.Children)
            {
                var propertyNameMayBe = property.GetDisplayTextExcludeDots();

                var izvratOrNormalPropertyNames = new List<string> { propertyNameMayBe };

                if (property.IsIzvratProperty())
                {
                    izvratOrNormalPropertyNames = obj.Properties()
                        .Where(x => x.Name.StartsWith(propertyNameMayBe))
                        .Select(x => x.Name.Substring(propertyNameMayBe.Length))
                        .Where(x => x.StartsWith("."))
                        .Select(x => x.Substring(1).Trim())
                        .Where(x => string.IsNullOrEmpty(x) == false)
                        .Select(x => $"{propertyNameMayBe}.{x}" )
                        .ToList();
                }

                foreach (var propertyName in izvratOrNormalPropertyNames)
                {
                    if (obj[propertyName] != null)
                    {
                        ModelElementObjectBase childClassDescription = null;

                        if (property.LinkedModelDefinitionId != null)
                            childClassDescription = this.modelElementStorage.GetModelElementTree(property.LinkedModelDefinitionId.Value);

                        if (childClassDescription != null || property.Children.Any())
                        {
                            var childToken = obj[propertyName];

                            if (property.Children.Any() && childClassDescription != null)
                            {
                                childClassDescription = new ModelElementObjectBase(childClassDescription);
                                childClassDescription.Children =
                                    childClassDescription.Children.Union(property.Children).ToList();
                            }
                            else if (childClassDescription == null && property.Children.Any())
                            {
                                childClassDescription = property;
                            }

                            if (childToken.Type == JTokenType.Array)
                            {
                                var childArray = childToken as JArray;
                                var childArrayRes = new JArray();

                                foreach (var childArrayItem in childArray.Children())
                                {
                                    var childObject = childArrayItem as JObject;
                                    var childProperty = this.CamelCaseToDots(childObject, childClassDescription, path + "." + property.DisplayText);
                                    childArrayRes.Add(childProperty);
                                }

                                result.Add(property.DisplayText, childArrayRes);
                            }
                            else
                            {
                                var childObject = childToken as JObject;

                                var childProperty = this.CamelCaseToDots(childObject, childClassDescription, path + "." + property.DisplayText);

                                result.Add(property.DisplayText, childProperty);
                            }
                        }
                        else
                        {
                            result.Add(property.DisplayText, obj[propertyName]);
                        }
                    }
                }
            }

            return result;
        }

        public JToken CamelCaseToDotsInFilterInternal(JToken obj, ModelElementObjectBase currentClassDescription)
        {
            if (obj == null)
                return null;

            switch (obj.Type)
            {
                case JTokenType.Array:
                {
                    var arr = (JArray) obj;
                    var result = new JArray();
                    foreach (var token in arr.Children())
                    {
                        result.Add(CamelCaseToDotsInFilterInternal(token, currentClassDescription));
                    }

                    return result;
                }
                case JTokenType.Object:
                {
                    var typedObj = (JObject) obj;
                    var res = new JObject();
                    var pivot = ParserHelper.GetPivot(ParserHelper.TreeNodeTypeToPivot[currentClassDescription.ModelElementType]);
                    var pivotCommonProperties = pivot.GetAllCommonProperties();

                    foreach (var child in typedObj.Properties())
                    {
                        if (child.Name.StartsWith("$"))
                        {
                            res.Add(child.Name, child.Value);
                        }
                        else
                        {
                            var prop =
                                currentClassDescription.Children.FirstOrDefault(
                                    x => x.GetDisplayTextExcludeDots() == child.Name);

                            if (prop != null)
                            {
                                var propRes = CamelCaseToDotsInFilterInternal(child.Value,
                                    this.modelElementStorage.FindByElement(prop));

                                res.Add(prop.DisplayText, propRes);
                            }
                            else if (pivotCommonProperties.Contains(child.Name))
                            {
                                res.Add(child.Name, child.Value);
                            }
                        }
                    }

                    return res;
                }
                default:
                    return obj;
            }
        }

        public JObject CamelCaseToDotsInFilter(JObject obj)
        {
            var rootModel = this.modelElementStorage.GetModelElementTree(obj.Properties().First().Name);

            var className = rootModel.DisplayText;

            var result = CamelCaseToDotsInFilterInternal(obj, rootModel) as JObject;

            if(result == null)
                return result;

            result.Add(className,
                CamelCaseToDotsInFilterInternal(obj[className], rootModel));

            //todo: orderBy

            return result;
        }
    }
}