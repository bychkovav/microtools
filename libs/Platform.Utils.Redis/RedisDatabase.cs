// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisDatabase.cs" company="">
//   
// </copyright>
// <summary>
//   The redis database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Platform.Utils.Redis
{
    using StackExchange.Redis;

    /// <summary>
    /// The redis database.
    /// </summary>
    public class RedisDatabase
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly RedisConnection connection;

        /// <summary>
        /// The db.
        /// </summary>
        private readonly int db;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDatabase"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="db">
        /// The db.
        /// </param>
        public RedisDatabase(RedisConnection connection, int db)
        {
            this.connection = connection;
            this.db = db;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <returns>
        /// The <see cref="IDatabase"/>.
        /// </returns>
        public IDatabase Get()
        {
            return this.connection.GetDatabase(this.db);
        }

        /// <summary>
        /// The flush.
        /// </summary>
        public void Flush()
        {
            this.connection.FlushDatabase(this.db);
        }
    }
}
