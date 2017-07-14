namespace Platform.Utils.Events.Domain.Objects
{
    using System;
    using System.Collections.Generic;
    using Enums;

    public class ModelElementObjectBase
    {
        public ModelElementObjectBase()
        {
            AllowdChildrenType = new List<TreeNodeType>();
            Children = new List<ModelElementObjectBase>();
        }

        public ModelElementObjectBase(ModelElementObjectBase other)
        {
            Id = other.Id;
            ProductId = other.ProductId;
            JsonStructure = other.JsonStructure;
            CreateDate = other.CreateDate;
            ModelElementType = other.ModelElementType;
            Code = other.Code;
            ServiceId = other.ServiceId;
            GlobalModelId = other.GlobalModelId;
            ModelDefinitionId = other.ModelDefinitionId;
            AllowdChildrenType = other.AllowdChildrenType;
            Children = other.Children;
            ParentId = other.ParentId;
            Parent = other.Parent;
            LookupModelDefinitionId = other.LookupModelDefinitionId;
            LinkedModelDefinitionId = other.LinkedModelDefinitionId;
            LinkedModelDefinitionName = other.LinkedModelDefinitionName;
            LinkedViewModelId = other.LinkedViewModelId;
            DataType = other.DataType;
            DisplayText = other.DisplayText;
        }

        public Guid Id { get; set; }

        public Guid? ProductId { get; set; }

        public string JsonStructure { get; set; }

        public DateTime CreateDate { get; set; }

        public TreeNodeType ModelElementType { get; set; }

        public string Code { get; set; } //pending pickup

        public Guid? ServiceId { get; set; } //pending pickup

        public Guid? GlobalModelId { get; set; } //pending pickup

        public Guid ModelDefinitionId { get; set; } //pending pickup

        public IList<TreeNodeType> AllowdChildrenType { get; set; }

        public IList<ModelElementObjectBase> Children { get; set; }

        public Guid? ParentId { get; set; }

        public ModelElementObjectBase Parent { get; set; }

        public Guid? LookupModelDefinitionId { get; set; }

        public Guid? LinkedModelDefinitionId { get; set; }

        public string LinkedModelDefinitionName { get; set; }

        public Guid? LinkedViewModelId { get; set; }

        public DataType? DataType { get; set; }

        public string DisplayText { get; set; }
    }
}