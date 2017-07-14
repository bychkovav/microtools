namespace Platform.Utils.Nhibernate
{
    using System;
    using Platform.Utils.Nhibernate.Conventions;

    public class DeletedEntitiesHistory: IDisposable
    {
        private readonly PlatformDataProvider _dataProvider;

        public DeletedEntitiesHistory(PlatformDataProvider dataProvider)
        {
            _dataProvider = dataProvider;

            if (_dataProvider.SafeDeleteMode)
            {
                _dataProvider.Session.DisableFilter(SafeDeleteConvention.FilterName);
            }
        }

        public void Dispose()
        {
            if (_dataProvider.SafeDeleteMode)
            {
                _dataProvider.Session.EnableFilter(SafeDeleteConvention.FilterName);
            }
        }
    }
}