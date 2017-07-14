namespace Platform.Utils.Owin.Storage
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Redis;
    using StackExchange.Redis;

    public class RedisStorage<T> : IStorage<T> where T : class
    {
        public const string KeyFormat = "principal:{0}";
        private readonly BinaryFormatter binaryFormatter;
        private readonly RedisDatabase database;
        private readonly TimeSpan expireIn;

        public RedisStorage(RedisDatabase database, TimeSpan timeout)
        {
            this.database = database;
            this.expireIn = timeout;
            this.binaryFormatter = new BinaryFormatter();
        }

        public T this[object key]
        {
            get
            {
                IDatabase db = this.database.Get();
                string stringKey = GetStringKey(key);
                RedisValue redisValue = db.StringGet(stringKey);
                if (string.IsNullOrEmpty(redisValue))
                {
                    return default(T);
                }
                T retVal;
                using (var stream = new MemoryStream(Convert.FromBase64String(redisValue)))
                {
                    retVal = (T)this.binaryFormatter.Deserialize(stream);
                }
                db.KeyExpire(stringKey, this.expireIn);
                return retVal;
            }
            set
            {
                if (value == null)
                {
                    this.database.Get().KeyDelete(GetStringKey(key));
                }
                else
                {
                    using (var stream = new MemoryStream())
                    {
                        this.binaryFormatter.Serialize(stream, value);
                        string data = Convert.ToBase64String(stream.ToArray());
                        this.database.Get().StringSet(GetStringKey(key), data, this.expireIn);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets formatted key value with some preffix before plain key value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetStringKey(object key)
        {
            return string.Format(KeyFormat, key);
        }
    }
}
