namespace Platform.Utils.Events.Domain.Objects.Scripts
{
    using System.Collections.Generic;

    public class ListObject<TItem>
    {
        public IList<TItem> List { get; set; }
        public long TotalCount { get; set; }
    }
}