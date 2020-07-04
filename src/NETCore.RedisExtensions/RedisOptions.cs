using System;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace NETCore.RedisExtensions
{
    public class RedisOptions : IOptions<RedisOptions>
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// This is preferred over Configuration.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// 数据库索引号
        /// </summary>
        public int Index { set; get; } = 0;

        /// <summary>
        /// 
        /// </summary>
        RedisOptions IOptions<RedisOptions>.Value
        {
            get { return this; }
        }
    }
}
