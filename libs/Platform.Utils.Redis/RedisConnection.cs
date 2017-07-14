// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedisConnection.cs" company="">
//   
// </copyright>
// <summary>
//   The redis connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Platform.Utils.Redis
{
    using System;
    using System.Net;

    using StackExchange.Redis;

    /// <summary>
    /// The redis connection.
    /// </summary>
    public class RedisConnection
    {
        /// <summary>
        /// The connection multiplexer.
        /// </summary>
        private readonly ConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnection"/> class.
        /// </summary>
        /// <param name="initialConfig">
        /// The initial config.
        /// </param>
        public RedisConnection(string initialConfig)
        {
            ConfigurationOptions configuration = ConfigurationOptions.Parse(initialConfig);
            configuration.SyncTimeout = 20000;
            configuration.ConnectTimeout = 20000;
            //Change options here if necessary
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
        }

        /// <summary>
        /// The get database.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <returns>
        /// The <see cref="IDatabase"/>.
        /// </returns>
        public IDatabase GetDatabase(int db)
        {
            return this.connectionMultiplexer.GetDatabase(db);
        }

        /// <summary>
        /// The flush database.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public void FlushDatabase(int db)
        {
            EndPoint[] endPoints = this.connectionMultiplexer.GetEndPoints();
            foreach (EndPoint endPoint in endPoints)
            {
                this.connectionMultiplexer.GetServer(endPoint).FlushDatabase(db);
            }
        }

        /// <summary>
        /// The subscribe.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> callback)
        {
            this.connectionMultiplexer.GetSubscriber().Subscribe(channel, callback);
        }
    }
}
