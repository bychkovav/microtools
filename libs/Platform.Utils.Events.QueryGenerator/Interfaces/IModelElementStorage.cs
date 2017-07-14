namespace Platform.Utils.Events.QueryGenerator.Interfaces
{
    using System;
    using Platform.Utils.Events.Domain.Objects;

    public interface IModelElementStorage
    {
        ModelElementObjectBase GetModelElementTree(Guid modelDefinitionId);
        ModelElementObjectBase GetModelElementTree(string modelName);
    }
}