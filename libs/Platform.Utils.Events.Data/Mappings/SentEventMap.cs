namespace Platform.Utils.Events.Data.Mappings
{
    using Domain;
    using FluentNHibernate.Mapping;
    using Nhibernate;

    public class SentEventMap : ClassMap<SentEventEntity>
    {
        public SentEventMap()
        {
            this.MapBase();

            Map(x => x.Content).Length(int.MaxValue).CustomSqlType("nvarchar(max)");
            Map(x => x.CorrelationId);
            Map(x => x.EventUniqueId);
            Map(x => x.EventCode);
            Map(x => x.ContentType);
            Map(x => x.TargetContextIdentity);
        }
    }
}
