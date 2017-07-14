namespace Platform.Utils.Nhibernate
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Conventions;
    using Domain;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Conventions;
    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Context;
    using NHibernate.Criterion;
    using NHibernate.Engine;
    using NHibernate.Linq;
    using NHibernate.Type;
    using NHibernate.Util;

    using NLog;

    using Environment = NHibernate.Cfg.Environment;

    /// <summary>
    ///     The platform data provider.
    /// </summary>
    public class PlatformDataProvider
    {
        public readonly bool SafeDeleteMode;

        /// <summary>
        ///     The factory.
        /// </summary>
        private readonly ISessionFactory factory;
        //TODO: NOTE: 'COre' for serviceConfiguration
        private const string MappingsProjectTemplate = "Platform..*.(Data|Domain|Core|Mappings)";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformDataProvider"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        /// <param name="additionalAssemblyNames"></param>
        /// <param name="currentSessionContextType"></param>
        /// <param name="safeDeleteMode"></param>
        public PlatformDataProvider(string connectionString,
            IEnumerable<AssemblyName> additionalAssemblyNames = null,
            Type currentSessionContextType = null,
            bool safeDeleteMode = true)
        {
            this.SafeDeleteMode = safeDeleteMode;
            var conventionsList = new List<IConvention>() { new TableNameConvention(), new ColumnNameConvention(), new FkNameConvention() };

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetName())
                .Where(a => new Regex(MappingsProjectTemplate).IsMatch(a.Name))
                .ToList();

            if (additionalAssemblyNames != null)
            {
                assemblies = assemblies.Union(additionalAssemblyNames).ToList();
            }

            assemblies.ForEach(x => Trace.WriteLine(x.FullName));

            if (currentSessionContextType == null)
            {
                currentSessionContextType = typeof(PlatformNhSessionContext);
            }

            var configuration = new Configuration()
                .SetProperty(Environment.ConnectionString, connectionString)
                .SetInterceptor(new TrackChangesInterceptor());

            if (safeDeleteMode)
            {
                var filterDef = new FilterDefinition(
                    SafeDeleteConvention.FilterName,
                    "Deleted <> 1",
                    new Dictionary<string, IType>(),
                    false);

                configuration.AddFilterDefinition(filterDef);

                conventionsList.Add(new SafeDeleteConvention());
            }

            this.factory = Fluently.Configure(configuration.Configure())
                .Mappings(v => assemblies.ForEach(a => v.FluentMappings.AddFromAssembly(Assembly.Load(a))
                    .Conventions.Add(conventionsList.ToArray())))
                .CurrentSessionContext(currentSessionContextType.AssemblyQualifiedName)
                .BuildSessionFactory();
        }

        /// <summary>
        ///     Gets the session.
        /// </summary>
        public ISession Session
        {
            get { return this.OpenSession(); }
        }

        /// <summary>
        ///     The open session.
        /// </summary>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession OpenSession()
        {
            if (CurrentSessionContext.HasBind(this.factory))
            {
                return this.factory.GetCurrentSession();
            }

            ISession session = this.factory.OpenSession();
            session.FlushMode = FlushMode.Never;
            if (SafeDeleteMode)
            {
                session.EnableFilter(SafeDeleteConvention.FilterName);
            }
            CurrentSessionContext.Bind(session);

            return session;
        }

        /// <summary>
        ///     The close session.
        /// </summary>
        public void CloseSession()
        {
            ISession session = CurrentSessionContext.Unbind(this.factory);
            if (session != null && session.IsOpen)
            {
                try
                {
                    if (session.Transaction != null && session.Transaction.IsActive)
                    {
                        session.Transaction.Rollback();
                        //throw new Exception("Rolling back uncommited NHibernate transaction.");
                    }
                    //                    session.Flush();
                }
                finally
                {
                    session.Close();
                    session.Dispose();
                }
            }
        }

        /// <summary>
        ///     The begin transaction.
        /// </summary>
        /// <param name="isolationLevel">
        ///     The isolation level.
        /// </param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            this.Session.BeginTransaction(isolationLevel);
        }

        /// <summary>
        ///     The begin transaction.
        /// </summary>
        public void BeginTransaction()
        {
            this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        ///     The commit transaction.
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                if (this.Session != null && this.Session.Transaction.IsActive)
                {
                    this.Session.Transaction.Commit();
                }
            }
            catch
            {
                this.RollbackTransaction();
                throw;
            }
        }

        /// <summary>
        ///     Rollbacks NHibernate transaction and close session.
        /// </summary>
        public void RollbackTransaction()
        {
            try
            {
                this.Session.Transaction.Rollback();
            }
            finally
            {
                this.CloseSession();
            }
        }

        /// <summary>
        ///     Completely clears the session. Evict all loaded instances and cancel pending saves, updates and deletes.
        /// </summary>
        public void Clear()
        {
            this.Session.Clear();
        }

        /// <summary>
        ///     Creates root criteria for current session.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="ICriteria" />.
        /// </returns>
        public ICriteria CreateCriteria<T>()
            where T : EntityBase
        {
            return this.Session.CreateCriteria(typeof(T));
        }

        /// <summary>
        ///     Saves entity to database.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The <see cref="object" />.
        /// </returns>
        public object Save<T>(T entity)
            where T : EntityBase
        {
            object id = this.Session.Save(entity);

            this.Session.Flush();
            return id;
        }

        /// <summary>
        ///     Saves entity to database.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        public void Delete<T>(T entity)
            where T : EntityBase
        {
            this.Session.Delete(entity);
            this.Session.Flush();
        }

        /// <summary>
        ///     The query over.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IQueryOver" />.
        /// </returns>
        public IQueryOver<T, T> QueryOver<T>() where T : EntityBase
        {
            var query = this.Session.QueryOver<T>();
            return query;
        }

        public IQueryable<T> Query<T>() where T : EntityBase
        {
            return this.Session.Query<T>();
        }

        public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : EntityBase
        {
            var query = this.Session.QueryOver(alias);

            return query;
        }

        public IQueryOver<T, T> QueryOver<T>(QueryOver<T> detachedQuery) where T : EntityBase
        {
            var query = detachedQuery.GetExecutableQueryOver(Session);

            return query;
        }

        public void Unproxy<T>(T enity) where T : EntityBase
        {
            this.Session.GetSessionImplementation().PersistenceContext.Unproxy(enity);
        }

        /// <summary>
        ///     Return the persistent instance of the <see cref="T" /> entity with the given identifier, or null if there is no
        ///     such persistent instance. (If the instance, or a proxy for the instance, is already associated with the session,
        ///     return that instance or proxy.)
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="id">an identifier</param>
        /// <returns>a persistent instance or null</returns>
        public T Get<T>(object id)
            where T : EntityBase
        {
            return this.Session.Get<T>(id);
        }

        /// <summary>
        /// 
        ///     Gets new stateless session. Can be used only in specific cases.
        /// </summary>
        /// <returns></returns>
        public IStatelessSession OpenStatelessSession()
        {
            return this.factory.OpenStatelessSession();
        }

        /// <summary>
        ///     Execute raw sql query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ISQLQuery CreateSqlQuery(string query)
        {
            return this.Session.CreateSQLQuery(query);
        }


        ///     <summary>
        ///         Saves or updates entity to database.
        ///     </summary>
        ///     <typeparam name="T">
        ///     </typeparam>
        ///     <param name="entity">
        ///         The entity.
        ///     </param>
        ///     <returns>
        ///         The <see cref="object" />.
        ///     </returns>
        public void SaveOrUpdate<T>(T entity)
            where T : EntityBase
        {
            this.Session.SaveOrUpdate(entity);
            this.Session.Flush();
        }

        public void Update<T>(T entity)
            where T : EntityBase
        {
            this.Session.Update(entity);
            this.Session.Flush();
        }

        public void Merge<T>(T entity)
            where T : EntityBase
        {
            this.Session.Merge(entity);
            this.Session.Flush();
        }
    }
}