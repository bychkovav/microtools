namespace Platform.Utils.Events.Data.Mappings
{
    using Domain;
    using FluentNHibernate.Mapping;
    using Nhibernate;

    public class RecieveEventMap : ClassMap<RecieveEventEntity>
    {
        public RecieveEventMap()
        {
            this.MapBase();

            Map(x => x.Content).Length(int.MaxValue).CustomSqlType("nvarchar(max)");
            Map(x => x.CorrelationId);
            Map(x => x.EventUniqueId);
            Map(x => x.EventCode);
            Map(x => x.ContentType);
            Map(x => x.TargetContextIdentity);

            Map(x => x.SenderServiceIdentity);
            Map(x => x.SenderContextIdentity);
            Map(x => x.Completed);
        }
    }
}
