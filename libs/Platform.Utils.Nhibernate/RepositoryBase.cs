using System.Collections.Generic;

namespace Platform.Utils.Nhibernate
{
    using System;
    using System.Linq;
    using Domain;
    using NHibernate;
    using NHibernate.Criterion;
    using NHibernate.Engine;

    public abstract class RepositoryBase<TEntity> where TEntity : EntityBase
    {
        protected readonly PlatformDataProvider DataProvider;

        protected RepositoryBase(PlatformDataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public IQueryable<TEntity> GetQueryableByIdList(IEnumerable<Guid> idList, bool skipDeleted = true)
        {
            var result = GetQueryable(skipDeleted);
            var idArray = idList.ToArray();

            result = result.Where(x => idArray.Contains(x.Id));

            return result;
        }

        public IQueryable<TEntity> GetQueryableById(Guid id, bool skipDeleted = true)
        {
            var result = GetQueryableByIdList(new[] {id}, skipDeleted);

            return result;
        }

        public IQueryable<TEntity> GetQueryable(bool skipDeleted = true)
        {
            var result = DataProvider.Query<TEntity>();

            if (skipDeleted)
                result = result.Where(x => !x.Deleted);

            return result;
        } 

        public virtual void Save(TEntity entity)
        {
            if (entity.IsNew)
                DataProvider.Save(entity);
            else
                DataProvider.Update(entity);
        }

        public virtual TEntity GetEntityById(Guid id)
        {
            return DataProvider.Get<TEntity>(id);
        }

        public virtual void Delete(Guid id)
        {
            TEntity entity = GetEntityById(id);
            if (entity == null)
            {
                return;
            }
            entity.Deleted = true;
            Save(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entity.Deleted = true;
            Save(entity);
        }

        /// <summary>
        ///     Adds <see cref="paging"/> restrictions to <see cref="query"/>.
        /// </summary>
        protected void AddPaging<T>(IQueryOver<T, T> query, FilterBase paging, bool distinct = false) where T : EntityBase
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (paging == null)
            {
                return;
            }
            if (paging.Take.HasValue)
            {
                query.Take(paging.Take.Value);
            }
            if (paging.Skip.HasValue)
            {
                query.Skip(paging.Skip.Value);
            }

            var rowQuery = distinct ? ToDistinctRowCount(query) : query.ToRowCountQuery();
            paging.TotalCount = rowQuery.FutureValue<int>().Value;
        }

        protected void AddPaging<T>(IQueryable<T> query, FilterBase paging, bool distinct = false) where T : EntityBase
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            if (paging == null)
            {
                return;
            }
            if (paging.Take.HasValue)
            {
                query = query.Take(paging.Take.Value);
            }
            if (paging.Skip.HasValue)
            {
                query = query.Skip(paging.Skip.Value);
            }
        }

        /// <summary>
        /// Creates row count query (with unique identifier)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IQueryOver<T> ToDistinctRowCount<T>(IQueryOver<T, T> query) where T : EntityBase
        {
            return query.Clone()
                .Select(Projections.CountDistinct<T>(x => x.Id))
                .ClearOrders()
                .Skip(0)
                .Take(RowSelection.NoValue);
        }

        //public virtual IQueryOver<T, T> Get(FilterBase filter)
        //{
        //    IQueryOver<T, T> query = DataProvider.QueryOver<T>();

        //    if (!string.IsNullOrEmpty(filter.OrderBy))
        //        query.RootCriteria.AddOrder(new Order(filter.OrderBy,
        //                                              filter.OrderDirection == OrderDirection.Asc));
        //    if (filter.Skip.HasValue)
        //        query.Skip(filter.Skip.Value);

        //    if (filter.Take.HasValue)
        //        query.Take(filter.Take.Value);

        //    return query;
        //}
    }
}