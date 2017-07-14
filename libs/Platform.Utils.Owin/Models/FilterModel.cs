namespace Platform.Utils.Owin.Models
{
    using Domain;

    public class FilterModel 
    {
        #region [Constants]

        private const int TakeDefault = 25;

        #endregion

        public string SearchString { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public string OrderBy { get; set; }

        public OrderDirection OrderDirection { get; set; }

        public FilterModel()
        {
            Take = TakeDefault;
        }

        public FilterBase Unbind(FilterBase filter)
        {
            filter.Skip = Skip;
            filter.Take = Take;
            filter.OrderBy = OrderBy;
            filter.OrderDirection = OrderDirection;

            return filter;
        }
    }
}
