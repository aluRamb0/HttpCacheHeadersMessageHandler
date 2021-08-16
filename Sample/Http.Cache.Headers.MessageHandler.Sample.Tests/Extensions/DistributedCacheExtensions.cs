using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;

namespace Http.Cache.Headers.MessageHandler.Sample.Tests.Extensions
{
    internal static class DistributedCacheExtensions
    {
        internal static async Task<CachedHttpContent> GetHttpContent(this IDistributedCache cache, string key)
        {
            var entry = await cache.GetStringAsync(key);

            return JToken.Parse(entry).ToObject<CachedHttpContent>();
        }
    }
}