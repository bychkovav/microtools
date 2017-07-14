namespace Platform.Utils.Events.Data
{
    using Domain;
    using Nhibernate;

    public class RecieveEventRepository : RepositoryBase<RecieveEventEntity>
    {
        public RecieveEventRepository(PlatformDataProvider dataProvider) : base(dataProvider)
        {
        }
    }
}
