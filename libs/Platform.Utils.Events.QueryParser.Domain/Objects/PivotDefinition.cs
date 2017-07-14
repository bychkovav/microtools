namespace Platform.Utils.Events.QueryParser.Domain.Objects
{
    using System;
    using System.Collections.Generic;
    using Enums;

    [Flags]
    public enum PivotTypeForm : int
    {
        Property = 0x1,
        Collection = 0x2
    }

    [Flags]
    public enum PivotStorageFeatures : int
    {
        /// <summary> Use pivot name as storage node name (e.g. bp: {...})</summary>
        StoreByPivotName = 0x1,

        /// <summary> Use pivot main value as storage node name (e.g. ae.objectCode: {...})</summary>
        StoreByMainPivotValue = 0x2,

        /// <summary> Use pivot second value as storage node name (e.g. vmd.objectCode.capacity: {...})</summary>
        StoreBySecondPivotValue = 0x4,

        /// <summary> Add pivot main value as filteration subject </summary>
        FilterByMainPivotValue = 0x8,
    }

    [Serializable]
    public class PivotDefinition
    {
        public PivotType Type { get; set; }

        public PivotTypeForm Form { get; set; } = 0;

        public PivotStorageFeatures StorageFeatures { get; set; } = 0;

        public string MainProperty { get; set; }

        public string SecondaryProperty { get; set; }

        public List<string> CommonProperties { get; set; } = new List<string>();

//        public bool ShouldBeFilteredByMainPivotValue { get; set; } = false;

        public bool IsCollection => Form.HasFlag(PivotTypeForm.Collection);

        public bool IsIntent { get; set; } = false;

        public List<PivotType> AllowedNestedPivotTypes { get; set; } = new List<PivotType>();
    }
}