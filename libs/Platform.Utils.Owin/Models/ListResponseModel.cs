namespace Platform.Utils.Owin.Models
{
    using System.Collections.Generic;

    public class ListResponseModel<TItem> : ServiceResponseModel<IList<TItem>>
    {
        public long RowsCount { get; set; }
    }
}