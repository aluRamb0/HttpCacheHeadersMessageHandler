using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Http.Cache.Headers.MessageHandler
{
    internal static class HttpResponseMessageExtensions
    {
        internal static async Task<CachedHttpContent> GetCacheEntryAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
        {
            //we depend on etag header to get this working
            if (response.Headers.ETag == null)
            {
                return null;
            }
            await response.Content.LoadIntoBufferAsync();
            
            var buffer = await response.Content.ReadAsByteArrayAsync();
            
            var headers = response.Content.Headers.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            
            return new CachedHttpContent(response.Headers.ETag.ToString(), headers, response.StatusCode, buffer);
        }
        
        internal static bool SetFromCacheIfNotModified(this HttpResponseMessage response, CachedHttpContent cachedHttpContent)
        {
            //Ensure that the conditions to use cache value are met by checking for NotModified, ETag header, and a cachedHttpContent
            if (cachedHttpContent == null || response.StatusCode != HttpStatusCode.NotModified || response.Headers.ETag is not { } entityTagHeaderValue)
            {
                return false;
            }

            //compare the tags
            if (cachedHttpContent.Etag.Equals(entityTagHeaderValue.Tag, StringComparison.InvariantCultureIgnoreCase) != true)
            {
                return false;
            }
            
            response.Content = cachedHttpContent.ToHttpContent();
            
            //override the NoModified status code
            response.StatusCode = cachedHttpContent.StatusCode;
            return true;
        }
        
        internal static DistributedCacheEntryOptions GetCacheOptions(this HttpResponseMessage response)
        {  
            var entryOptions = new DistributedCacheEntryOptions();
            
            if (response.Headers.CacheControl is { } cacheControl)
            {
                entryOptions.AbsoluteExpiration = DateTimeOffset.Now.Add(cacheControl.MaxAge.GetValueOrDefault());
            }
            return entryOptions;
        }
    }
}