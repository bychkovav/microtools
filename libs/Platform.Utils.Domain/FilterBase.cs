namespace Platform.Utils.Domain
{
    public class FilterBase
    {
        public int? Skip { get; set; }

        public int? Take { get; set; }

        public int TotalCount { get; set; }

        public string OrderBy { get; set; }

        public OrderDirection OrderDirection { get; set; }
    }

}
