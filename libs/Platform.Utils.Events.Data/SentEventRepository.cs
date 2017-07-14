namespace Platform.Utils.Events.Data
{
    using Domain;
    using Nhibernate;

    public class SentEventRepository : RepositoryBase<SentEventEntity>
    {
        public SentEventRepository(PlatformDataProvider dataProvider)
            : base(dataProvider)
        {
        }
    }
}
