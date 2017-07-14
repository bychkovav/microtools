namespace Platform.Utils.Events.QueryParser.Domain.Objects
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class PivotData
    {
        public PivotDefinition PivotDefinition { get; set; }

        public string MainValue { get; set; }

        public List<string> SecondaryValues { get; set; } = new List<string>();
    }
}