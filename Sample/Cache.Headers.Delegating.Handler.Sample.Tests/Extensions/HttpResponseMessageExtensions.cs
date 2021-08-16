using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cache.Headers.Delegating.Handler.Sample.Tests.Setup;
using Cache.Headers.DelegatingHandler;

namespace Cache.Headers.Delegating.Handler.Sample.Tests.Extensions
{

    internal static class HttpResponseMessageExtensions
    {
        internal static string GetCacheKey(this HttpResponseMessage response) =>
            string.Format($"{Constants.CacheEntryPrefix}:{response.RequestMessage.RequestUri}");
        
        
        internal static async Task<CachedHttpContent> GetHttpContent(this HttpResponseMessage response)
        {
            await response.Content.LoadIntoBufferAsync();
            var expectedEtag = response.Headers.ETag?.ToString();
            var contentHeaders = response.Content.Headers.ToDictionary(x => x.Key, x => x.Value);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            return new CachedHttpContent(expectedEtag, contentHeaders, response.StatusCode, bytes);
        }
        
    }
}