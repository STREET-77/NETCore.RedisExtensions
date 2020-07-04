using System;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace NETCore.RedisExtensions.Filter
{
    public static class BloomFilter
    {
        /// <summary>
        /// 创建一个空的布隆过滤器，具有给定的所需错误率和初始容量
        /// </summary>
        /// <param name="key">过滤器键名</param>
        /// <param name="error_rate">误报的期望概率，这应该是介于0到1之间的十进制值</param>
        /// <param name="capacity">打算添加到过滤器中的条目数</param>
        /// <returns></returns>
        public static async Task<bool> ReserveAsync(this IDatabase database, string key, double error_rate, int capacity)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (bool)await database.ExecuteAsync("BF.RESERVE", key, error_rate, capacity);
        }

        /// <summary>
        /// 将项目添加到布隆过滤器中，如果该过滤器尚不存在，则创建该过滤器
        /// </summary>
        /// <param name="key">过滤器的名称</param>
        /// <param name="item">要添加的项目</param>
        /// <returns>如果是新插入的项目，则返回true；如果已存在，则返回false</returns>
        public static async Task<bool> AddAsync(this IDatabase database, string key, string item)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return (bool)await database.ExecuteAsync("BF.ADD", key, item);
        }

        /// <summary>
        /// 将一个或多个项目添加到“布隆过滤器”中，并创建一个尚不存在的过滤器。
        /// </summary>
        /// <param name="key">过滤器的名称</param>
        /// <param name="items">一个或多个要添加的项目</param>
        /// <returns>布尔值数组，每个元素是true还是false取决于相应的输入元素是新添加到过滤器还是先前已经存在</returns>
        public static async Task<bool[]> MAddAsycn(this IDatabase database, string key, string[] items)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // #Remark#
            // ExecuteAsync方法执行BF.MADD/BF.MEXISTS命令超时，未找到解决方法
            // 改用Lua脚本执行

            var argv = new StringBuilder();
            for (var i = 0; i < items.Length; i++)
            {
                argv.Append($",ARGV[{i + 1}]");
            }
            string script = $"return redis.call('BF.MADD', KEYS[1], {argv.Remove(0, 1).ToString()})";

            return (bool[])await database.ScriptEvaluateAsync(script, new RedisKey[] { key }, items.ToRedisValueArray());
        }

        /// <summary>
        /// 确定项目是否在布隆过滤器中存在
        /// </summary>
        /// <param name="key">过滤器的名称</param>
        /// <param name="item">要检查的项目</param>
        /// <returns>如果该项目不存在，则为false;如果该项目存在，则为true</returns>
        public static async Task<bool> ExistsAsync(this IDatabase database, string key, string item)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (string.IsNullOrEmpty(item))
            {
                throw new ArgumentNullException(nameof(item));
            }

            return (bool)await database.ExecuteAsync("BF.EXISTS", key, item);
        }


        /// <summary>
        /// 确定过滤器中是否存在一项或多项
        /// </summary>
        /// <param name="key">过滤器名称</param>
        /// <param name="items">一个或多个要检查的项目</param>
        /// <returns>布尔值数组，true表示相应的项目可能存在于过滤器中，而false表示不存在</returns>
        public static async Task<bool[]> MExistsAsync(this IDatabase database, string key, string[] items)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var argv = new StringBuilder();
            for (var i = 0; i < items.Length; i++)
            {
                argv.Append($",ARGV[{i + 1}]");
            }
            string script = $"return redis.call('BF.MEXISTS', KEYS[1], {argv.Remove(0, 1).ToString()})";

            return (bool[])await database.ScriptEvaluateAsync(script, new RedisKey[] { key }, items.ToRedisValueArray());
        }

        /// <summary>
        /// 将一个或多个项目添加到布隆过滤器中，如果尚不存在，则默认情况下将其创建
        /// </summary>
        /// <param name="key">过滤器的名称</param>
        /// <param name="error_rate">容错率，如果过滤器尚不存在，但未提供此值，则使用默认的容错率</param>
        /// <param name="capacity">如果过滤器已存在，则忽略此参数。如果过滤器不存在，但不存在此参数，则使用默认容量</param>
        /// <param name="items">指示要添加到过滤器中的项目</param>
        /// <returns>布尔值数组，每个元素是true还是false取决于相应的输入元素是新添加到过滤器还是先前已经存在</returns>
        public static async Task<bool[]> InsertAsync(this IDatabase database, string key, string[] items, double error_rate = 0.001, int capacity = 10000)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var argv = new StringBuilder();
            for (var i = 0; i < items.Length; i++)
            {
                argv.Append($",ARGV[{i + 1}]");
            }
            string script = $"return redis.call('BF.INSERT', KEYS[1], 'CAPACITY', KEYS[2], 'ERROR', KEYS[3], 'ITEMS', {argv.Remove(0, 1).ToString()})";

            return (bool[])await database.ScriptEvaluateAsync(script, new RedisKey[] { key, capacity.ToString(), error_rate.ToString() }, items.ToRedisValueArray());
        }
    }
}
