using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Cache.Headers.DelegatingHandler
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
            if (request.RequestUri?.AbsoluteUri is not { } path)
            {
                _logger.LogInformation("Ignoring caching for request with null RequestUri");
                return await base.SendAsync(request, cancellationToken);
            }

            if (_options.IgnoreNonGetRequest && request.Method != HttpMethod.Get)
            {
                _logger.LogInformation("HttpCacheHeadersDelegatingHandlerOptions.IgnoreNonGetRequest = true, ignoring caching for {Method} {Path}", request.Method.Method, path);
                return await base.SendAsync(request, cancellationToken);
            }

            var key = string.Format(_cacheKeyFormat, path);

            var cachedEntry = await _distributedCache.GetCacheEntryAsync(key, cancellationToken);
            
            if (cachedEntry?.Etag is { } etag)
            {
                _logger.LogInformation("Setting \"If-Match\" header from cached entry");
                request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(etag));
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            //return from cache if status code is NotModified
            if (response.SetFromCacheIfNotModified(cachedEntry))
            {
                _logger.LogInformation("Server status code was NotModified, cached entry will be used");
                return response;
            }
            
            //else set new cache entry
            if (await response.GetCacheEntryAsync(cancellationToken) is not { } modifiedEntry)
            {
                _logger.LogDebug("Could not cache response with ETag: {Etag}", response.Headers.ETag);
                return response;
            }

            //read from the response headers
            var cacheOptions = response.GetCacheOptions();
            
            await _distributedCache.SetCacheEntryAsync(key, modifiedEntry, cacheOptions, cancellationToken);
            
            return response;
        }
    }
}