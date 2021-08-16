using System.IO;
using System.Net.Http;

namespace Http.Cache.Headers.MessageHandler
{
    internal static class CacheEntryExtensions
    {
        internal static HttpContent ToHttpContent(this CachedHttpContent cachedHttpContent)
        {
            //we create a stream content, the stored response headers will ensure that it is converted to the original http content type.
            var content = new StreamContent(new MemoryStream(cachedHttpContent.Entry) {Position = 0});
            
            foreach (var (key, value) in cachedHttpContent.Headers)
            {
                content.Headers.Add(key, value);
            }
            
            return content;
        }
    }
}