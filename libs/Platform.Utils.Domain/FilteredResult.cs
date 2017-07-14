using System.Collections.Generic;

namespace Platform.Utils.Domain
{
    public class FilteredResult<T> where T : class
    {
        public IList<T> Items { get; set; }

        public int Total { get; set; }
    }
}
