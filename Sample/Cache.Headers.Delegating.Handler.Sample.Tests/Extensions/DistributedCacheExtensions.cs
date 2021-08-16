using System.Threading.Tasks;
using Cache.Headers.DelegatingHandler;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;

namespace Cache.Headers.Delegating.Handler.Sample.Tests.Extensions
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