namespace Platform.Template.Domain.Objects
{
    using System.Collections.Generic;

    public class ListObject<T>
    {
        public List<T> List { get; set; }
        public long TotalCount { get; set; }
    }
}