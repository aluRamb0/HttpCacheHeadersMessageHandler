using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Cache.Headers.DelegatingHandler
{
    internal static class DistributedCacheExtensions
    {
        internal static async Task<CachedHttpContent> GetCacheEntryAsync(this IDistributedCache distributedCache, string key, CancellationToken cancellationToken)
        {
            var entry = await distributedCache.GetStringAsync(key, cancellationToken);
            
            return string.IsNullOrWhiteSpace(entry)? null: JsonConvert.DeserializeObject<CachedHttpContent>(entry, new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }
        internal static async Task SetCacheEntryAsync(this IDistributedCache distributedCache, string key, CachedHttpContent cachedHttpContent, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        {
            await distributedCache.SetStringAsync(key, cachedHttpContent.ToString(),options, cancellationToken);
        }
    }
}