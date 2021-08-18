# Http Cache Headers Message Handler for Http Client
Delegating message handler for Http Client that caches responses using a distributed cache. This handler will cache responses in the following way:

1. Cheks the response for `Cache-Control` headers, and creates a cache entry for that request path.
2. The next time the same path is requested, it checks if there is a cache entry and appends the `Cache-Control` headers to the request.
3. If the server responds with `NotModified` status code, the response content is replaced with the cached entry.

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

Then you are good to go!

*Note: the back-end needs to already support http cache headers for this handler to work*
