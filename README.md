# Http Cache Headers Message Handler for Http Client
Delegating message handler for Http Client that caches responses using a distributed cache. This handler will cache responses in the following way:

1. Cheks the response headers for `ETag`, and creates a cache entry for that request path
2. The next time the same path is requested, it checks if there is a cache entry and appends the `Etag` to the request headers
3. If the server responds with `NotModified` status code, the response content is replaced with the cache entry

# Usage
First, register the services with ASP.NET Core's dependency injection container (in the ConfigureServices method on the Startup class)

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

Add the messahe handler to your http client as follows

```
services.AddHttpClient<MyHttpClient>()
    .AddHttpMessageHandler<HttpCacheHeadersDelegatingHandler>();
```

Then you are good to go!
