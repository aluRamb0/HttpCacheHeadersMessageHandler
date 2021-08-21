# Http Cache Headers Message Handler for Http Client
Delegating message handler for Http Client that caches responses using a distributed cache. This handler will cache responses in the following way:

1. Cheks the response for valid [Etag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag) and [Cache-Control](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control) headers, and caches the response.

3. The next time the same path is requested, it checks if there is a cache entry and appends [Conditional](https://developer.mozilla.org/en-US/docs/Web/HTTP/Conditional_requests) headers to the request. Read [here](#important)
4. If the server responds with `NotModified` status code, the response content is replaced with the cached entry.

This message handler is responsible for caching ONLY on the client side. Please check [HttpCacheHeaders](https://github.com/KevinDockx/HttpCacheHeaders), an easy to use middleware for supporting cache headers on the server side. The sample in this repo uses this middleware and the tests reference some of the code from there.

# Installation (NuGet)

```
Install-Package Http.Cache.Headers.MessageHandler
```

# Usage
First, register the services with your dependency injection container

```
services.AddHttpCacheHeadersMessageHandler(options =>
{
    options.IgnoreNonGetRequest = true;
    options.CacheEntryPrefix = "EtagCachingMessageHandler";
    options.CacheKeyHeaderNameExclusions = new[] { "some-auto-generated-header" };
})
```

You will also need to register a distrubuted cache

```
services.AddDistributedMemoryCache();
```

Add the message handler to your http client as follows

```
services.AddHttpClient<MyHttpClient>()
    .AddHttpMessageHandler<HttpCacheHeadersDelegatingHandler>();
```

*Note: the back-end needs to already support http cache headers for this handler to work. Also make sure to header the next section*

# Header Exclusions

<a name="important"></a>The cache key is generated using a combination of the request `path` plus the `headers`.

Headers with auto-generated values must be excluded when creating the cache key. If your header gets a new value for every request then it should be excluded

Not excluding these header means the handler will never find your cache entries, and will never append the [Conditional](https://developer.mozilla.org/en-US/docs/Web/HTTP/Conditional_requests) headers to the request.

You can exclude these types of headers when registering the services
    
`options.CacheKeyHeaderNameExclusions = new[] { "some-auto-generated-header" };`
