using System;

namespace Platform.Utils.Nhibernate
{
    using NHibernate;

    public interface IQueryFilter
    {
        void ApplyFilter(IQueryOver query, Type rootEntityType);
    }
}
