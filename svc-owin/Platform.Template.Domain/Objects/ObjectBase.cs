namespace Platform.Template.Domain.Objects
{
    using System;

    public class ObjectBase
    {
        public Guid Id { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
