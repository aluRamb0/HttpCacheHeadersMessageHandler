# Http Cache Headers Message Handler for Http Client
Delegating message handler for Http Client that stores responses using a distributed cache

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
