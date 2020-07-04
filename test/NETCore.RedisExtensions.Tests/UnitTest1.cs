using System;
using Xunit;

namespace NETCore.RedisExtensions.Tests
{
    public class UnitTest1
    {
        private readonly IRedisProvider redisProvider;

        public UnitTest1()
        {
            redisProvider = new RedisProvider(new RedisOptions
            {
                Configuration = "127.0.0.1:6379"
            });
        }

        [Fact]
        public void Test1()
        {
            var result = redisProvider.Db.StringSet("test_key", "helloworld");
            Assert.True(result);
        }
    }
}
