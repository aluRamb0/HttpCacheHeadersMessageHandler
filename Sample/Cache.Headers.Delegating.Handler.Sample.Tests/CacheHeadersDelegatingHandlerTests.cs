using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cache.Headers.Delegating.Handler.Sample.Tests.Extensions;
using Cache.Headers.Delegating.Handler.Sample.Tests.Setup;
using Cache.Headers.DelegatingHandler;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cache.Headers.Delegating.Handler.Sample.Tests
{
    public class CacheHeadersDelegatingHandlerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly StoreManipulationClient _client;
        private readonly IDistributedCache _distributedCache;

        public CacheHeadersDelegatingHandlerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.CreateClientWithDelegatingHandler();
            _distributedCache = factory.Services.GetRequiredService<IDistributedCache>();
        }

        [Fact]
        public async Task Can_Fetch_And_Add_To_Cache()
        {
            var response = await _client.GetAllAsync();
            
            //we expect the cache to contain this http content
            var expected = Assert.IsType<CachedHttpContent>(await response.GetHttpContent());
            
            //fetch cache key
            var key = response.GetCacheKey();
            
            //fetch the http content from cache
            var actual = Assert.IsType<CachedHttpContent>(await _distributedCache.GetHttpContent(key));
            
            //asserts
            Assert.Equal(expected.Etag, actual.Etag, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(expected.Headers, actual.Headers);
            Assert.Equal(expected.StatusCode, actual.StatusCode);
            Assert.Equal(expected.Entry, actual.Entry);
        }
        
        [Fact]
        public async Task Fetching_From_Cache_Is_Super_Fast()
        {
            //cache is empty
            var preCacheTimer = Stopwatch.StartNew();
            var responseA = await _client.GetAllAsync();
            var contentA = await responseA.GetHttpContent();
            preCacheTimer.Stop();
            
            //fetch cache key
            var key = responseA.GetCacheKey();
            
            //assert that cache is not empty
            Assert.NotNull(await _distributedCache.GetHttpContent(key));
            
            var postCacheTimer = Stopwatch.StartNew();
            var responseB = await _client.GetAllAsync();
            var contentB = await responseB.GetHttpContent();
            postCacheTimer.Stop();

            
            Assert.Equal(contentA.Entry, contentB.Entry);
            
            Assert.True(preCacheTimer.ElapsedMilliseconds > postCacheTimer.ElapsedMilliseconds);
            Assert.True(preCacheTimer.ElapsedMilliseconds / postCacheTimer.ElapsedMilliseconds > 2);
        }


        [Fact]
        public async Task Etag_Is_Invalidated_On_Not_Get_Routes()
        {
            var responseA = await _client.GetAllAsync();
            
            //response has been cache
            var contentA = await _distributedCache.GetHttpContent(responseA.GetCacheKey());
            
            
            //do something to invalidate etag
            await _client.CreateAsync("some Value");
            
            
            var responseB = await _client.GetAllAsync();
            var contentB = await _distributedCache.GetHttpContent(responseB.GetCacheKey());
            
            Assert.NotEqual(contentB.Etag, contentA.Etag);

        }
    }
}