namespace Platform.Utils.Events.QueryGenerator.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Platform.Utils.Events.Domain.Enums;
    using Platform.Utils.Events.Domain.Extensions;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Helpers;
    using Platform.Utils.Events.QueryParser.Helpers;

    public static class ModelElementObjectBaseExtensions {
        public static ModelElementObjectBase FindByPath(this ModelElementObjectBase element, string path, bool useDots)
        {
            return element.FindByPathInternal(path, "", useDots);
        }

        private static ModelElementObjectBase FindByPathInternal(this ModelElementObjectBase element, string path, string parentPath, bool useDots)
        {
            if (path == "" && parentPath == "")
            {
                return element;
            }

            if (parentPath != "")
            {
                parentPath += ".";
            }

            foreach (var child in element.Children)
            {
                var displayText = useDots ? child.DisplayText : child.GetDisplayTextExcludeDots();

                if (path == parentPath + displayText)
                {
                    return child;
                }

                var res = child.FindByPathInternal(path, parentPath, useDots);

                if (res != null)
                    return res;
            }

            return null;
        }

        public static ModelElementObjectBase FindByElementId(this ModelElementObjectBase element, string elementId)
        {
            var elementIdGuid = new Guid(elementId);
            foreach (var child in element.Children)
            {
                if (elementIdGuid == child.Id)
                {
                    return child;
                }

                var res = child.FindByElementId(elementId);

                if (res != null)
                    return res;
            }

            return null;
        }

        public static string GetDisplayTextExcludeDots(this ModelElementObjectBase node)
        {
            var preffix = (node.ParentId.HasValue && node.ModelElementType != TreeNodeType.Bp) ? $"{node.ModelElementType.ToString().ToLowerInvariant()}" : string.Empty;
            var code = preffix != "" ? node.Code.ToUpperCamelCaseName() : node.Code;
            return $"{preffix}{code}";
        }

        public static bool IsIzvratProperty(this ModelElementObjectBase node)
        {
            return
                DynamicApiHelper.IzvratPropertiesPivots.Contains(ParserHelper.TreeNodeTypeToPivot[node.ModelElementType]);
        }

        public static ModelElementObjectBase FindByElementId(this IList<ModelElementObjectBase> elementDescriptions, string elementId)
        {
            var elId = new Guid(elementId);

            ModelElementObjectBase result = elementDescriptions.FirstOrDefault(x => x.Id == elId);

            if (result != null)
                return result;

            var prop = elementDescriptions.First().FindByElementId(elementId);

            if (prop != null)
            {
                result = elementDescriptions.FirstOrDefault(x => x.ModelDefinitionId == prop.LinkedModelDefinitionId);

                if (result != null)
                {
                    result = new ModelElementObjectBase(result);

                    result.ModelElementType = prop.ModelElementType;

                    result.Children = result.Children.Union(prop.Children).ToList();

                    return result;
                }

                return prop;
            }

            return null;
        }
    }
}