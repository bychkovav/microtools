// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackChangesInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   The track changes interceptor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Platform.Utils.Nhibernate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using NHibernate;
    using NHibernate.Mapping;
    using NHibernate.SqlCommand;
    using NHibernate.Type;
    using NLog;
    using Array = System.Array;

    /// <summary>
    /// The track changes interceptor.
    /// </summary>
    public class TrackChangesInterceptor : EmptyInterceptor
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region [Constants]

        /// <summary>
        /// The create date property.
        /// </summary>
        private const string CreateDateProperty = "CreateDate";

        /// <summary>
        /// The update date property.
        /// </summary>
        private const string UpdateDateProperty = "UpdateDate";

        #endregion

        /// <summary>
        /// The on prepare statement.
        /// </summary>
        /// <param name="sql">
        /// The sql.
        /// </param>
        /// <returns>
        /// The <see cref="SqlString"/>.
        /// </returns>
        public override SqlString OnPrepareStatement(SqlString sql)
        {
            this.logger.Trace(sql);
            
            return base.OnPrepareStatement(sql);
        }

        /// <summary>
        /// The on flush dirty.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="currentState">
        /// The current state.
        /// </param>
        /// <param name="previousState">
        /// The previous state.
        /// </param>
        /// <param name="propertyNames">
        /// The property names.
        /// </param>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool OnFlushDirty(
            object entity,
            object id,
            object[] currentState,
            object[] previousState,
            string[] propertyNames,
            IType[] types)
        {
            return SetDates(currentState, propertyNames);
        }

        /// <summary>
        /// The on save.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <param name="propertyNames">
        /// The property names.
        /// </param>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool OnSave(
            object entity,
            object id,
            object[] state,
            string[] propertyNames,
            IType[] types)
        {
            return SetDates(state, propertyNames);
        }

        private static bool SetDates(object[] state, string[] propertyNames)
        {
            DateTime dateTime = DateTime.UtcNow;
            int createIndex = Array.IndexOf(propertyNames, CreateDateProperty);
            if (state[createIndex].Equals(DateTime.MinValue))
            {
                state[createIndex] = dateTime;
            }
            int updateIndex = Array.IndexOf(propertyNames, UpdateDateProperty);
            state[updateIndex] = dateTime;

            return true;
        }
    }
}
