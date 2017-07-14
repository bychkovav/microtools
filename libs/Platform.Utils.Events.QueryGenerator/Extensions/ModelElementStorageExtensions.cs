namespace Platform.Utils.Events.QueryGenerator.Extensions
{
    using System.Linq;
    using Platform.Utils.Events.Domain.Objects;
    using Platform.Utils.Events.QueryGenerator.Interfaces;

    public static class ModelElementStorageExtensions
    {
        public static ModelElementObjectBase FindByElement(this IModelElementStorage modelElementStorage, ModelElementObjectBase property)
        {
            var originalProperty = modelElementStorage.GetModelElementTree(property.ModelDefinitionId).FindByElementId(property.Id.ToString());

            var result = new ModelElementObjectBase(originalProperty);

            if (originalProperty.LinkedModelDefinitionId.HasValue)
            {
                var linkedModel = modelElementStorage.GetModelElementTree(originalProperty.LinkedModelDefinitionId.Value);

                result.Children = result.Children.Union(linkedModel.Children).ToList();
            }

            return result;
        }
    }
}