using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Http.Cache.Headers.MessageHandler
{
    public class HttpCacheHeadersDelegatingHandler: System.Net.Http.DelegatingHandler
    {
        private readonly ILogger _logger;
        
        private readonly IDistributedCache _distributedCache;
        
        private readonly HttpCacheHeadersDelegatingHandlerOptions _options;

        private readonly string _cacheKeyFormat;

        public HttpCacheHeadersDelegatingHandler(
            ILoggerFactory loggerFactory,
            IDistributedCache distributedCache,
            HttpCacheHeadersDelegatingHandlerOptions options)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache), $"{nameof(IDistributedCache)} dependency is required to store cache responses");
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory.CreateLogger<HttpCacheHeadersDelegatingHandler>();
            _cacheKeyFormat = $"{options.CacheEntryPrefix}:{{0}}";
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsoluteUri  == null)
            {
                _logger.LogInformation("Ignoring caching for request with null RequestUri");
                return await base.SendAsync(request, cancellationToken);
            }

            if (_options.IgnoreNonGetRequest && request.Method != HttpMethod.Get)
            {
                _logger.LogInformation("HttpCacheHeadersDelegatingHandlerOptions.IgnoreNonGetRequest = true, ignoring caching for {Method} {Path}", request.Method.Method, request.RequestUri);
                return await base.SendAsync(request, cancellationToken);
            }

            var key = string.Format(_cacheKeyFormat, request.GetRequestKey(_options.CacheKeyHeaderNameExclusions));

            var cachedEntry = await _distributedCache.GetCacheEntryAsync(key.ToSha256(), cancellationToken);
            
            if (cachedEntry?.Etag is { } etag)
            {
                _logger.LogInformation("Setting \"If-Match\" header from cached entry");
                request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));
               // request.Headers.IfModifiedSince = cachedEntry.ToHttpContent().Headers.LastModified;
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            //return from cache if status code is NotModified
            if (response.SetFromCacheIfNotModified(cachedEntry))
            {
                //set headers
                
                _logger.LogInformation("Server status code was NotModified, cached entry will be used");
                return response;
            }
            
            //else set new cache entry
            if (await response.GetCacheEntryAsync(cancellationToken) is not { } modifiedEntry)
            {
                _logger.LogDebug("Could not cache response with headers: {Etag}", response.Headers);
                return response;
            }

            //read from the response headers
            var cacheOptions = response.GetCacheOptions();
            _logger.LogDebug("Setting cache key: {cacheKey}", key);
            await _distributedCache.SetCacheEntryAsync(key.ToSha256(), modifiedEntry, cacheOptions, cancellationToken);
            
            return response;
        }
    }
}