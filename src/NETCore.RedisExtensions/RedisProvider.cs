using System;
using System.Threading;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace NETCore.RedisExtensions
{
    public class RedisProvider : IRedisProvider
    {
        private IConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _database;
        private readonly RedisOptions _options;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisProvider(IOptions<RedisOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
        }

        public IDatabase Db
        {
            get
            {
                if (_database == null)
                {
                    Connection();
                }

                return _database;
            }
        }

        public void Dispose()
        {
            if (_connectionMultiplexer != null)
            {
                _connectionMultiplexer.Close();
            }
        }

        public IDatabase GetDatabase(int index)
        {
            if (index < 0 || index > 15)
            {
                throw new IndexOutOfRangeException("数据库索引号超出范围");
            }

            if (_connectionMultiplexer == null)
            {
                Connection();
            }

            return _connectionMultiplexer.GetDatabase(index);
        }

        private void Connection()
        {
            if (_database != null)
            {
                return;
            }

            // 可用资源-1
            // 当可用资源<=0时，再次调用wait方法将阻塞
            _connectionLock.Wait();
            try
            {
                if (_database == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connectionMultiplexer = ConnectionMultiplexer.Connect(_options.ConfigurationOptions);
                    }
                    else
                    {
                        _connectionMultiplexer = ConnectionMultiplexer.Connect(_options.Configuration);
                    }

                    _database = _connectionMultiplexer.GetDatabase(_options.Index);
                }
            }
            finally
            {
                // 释放连接，可用资源+1
                _connectionLock.Release();
            }
        }
    }
}
