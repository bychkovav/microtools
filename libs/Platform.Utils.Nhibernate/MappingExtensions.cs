namespace Platform.Utils.Nhibernate
{
    using FluentNHibernate.Mapping;

    using Domain;

    public static class MappingExtensions
    {
        public static void MapBase<T>(this ClassMap<T> mapClass) where T : EntityBase
        {
            mapClass.Id(x => x.Id).GeneratedBy.GuidComb();
            mapClass.Map(x => x.CreateDate);
            mapClass.Map(x => x.UpdateDate);
            mapClass.Map(x => x.Deleted);
        }
    }
}
