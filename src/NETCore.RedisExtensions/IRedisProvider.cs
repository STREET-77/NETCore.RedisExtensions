using System;
using StackExchange.Redis;

namespace NETCore.RedisExtensions
{
    public interface IRedisProvider : IDisposable
    {
        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="index">数据库索引号</param>
        /// <returns></returns>
        IDatabase GetDatabase(int index);

        /// <summary>
        /// 当前数据库
        /// </summary>
        IDatabase Db { get; }
    }
}
