using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Http.Cache.Headers.MessageHandler.Sample.Tests.Extensions;
using Http.Cache.Headers.MessageHandler.Sample.Tests.Setup;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Http.Cache.Headers.MessageHandler.Sample.Tests
{
    public class CacheHeadersDelegatingHandlerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CacheHeadersDelegatingHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Can_Fetch_And_Add_To_Cache()
        {

            using var factory = new CustomWebApplicationFactory<Startup>();
            var client = factory.CreateClientWithDelegatingHandler();
            var distributedCache = factory.Services.GetRequiredService<IDistributedCache>();
            var response = await client.GetAllAsync();
            
            //we expect the cache to contain this http content
            var expected = Assert.IsType<CachedHttpContent>(await response.GetHttpContent());
            
            //fetch cache key
            var key = response.GetCacheKey();
            
            //fetch the http content from cache
            var actual = Assert.IsType<CachedHttpContent>(await distributedCache.GetHttpContent(key));
            
            //asserts
            Assert.Equal(expected.Etag, actual.Etag, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(expected.Headers, actual.Headers);
            Assert.Equal(expected.StatusCode, actual.StatusCode);
            Assert.Equal(expected.Entry, actual.Entry);
            
            //clea
        }
        
        [Fact]
        public async Task Fetching_From_Cache_Is_Super_Fast()
        {
            
            using var factory = new CustomWebApplicationFactory<Startup>();
            var client = factory.CreateClientWithDelegatingHandler();
            var distributedCache = factory.Services.GetRequiredService<IDistributedCache>();
            //cache is empty
            var preCacheTimer = Stopwatch.StartNew();
            var responseA = await client.GetAllAsync();
            var contentA = await responseA.GetHttpContent();
            preCacheTimer.Stop();
            _testOutputHelper.WriteLine("Pre Cache: {0} (ms)", preCacheTimer.ElapsedMilliseconds);
            
            //fetch cache key
            var key = responseA.GetCacheKey();
            
            //assert that cache is not empty
            Assert.NotNull(await distributedCache.GetHttpContent(key));
            
            var postCacheTimer = Stopwatch.StartNew();
            var responseB = await client.GetAllAsync();
            var contentB = await responseB.GetHttpContent();
            postCacheTimer.Stop();
            _testOutputHelper.WriteLine("Post Cache: {0} (ms)", postCacheTimer.ElapsedMilliseconds);

            
            Assert.Equal(responseA.Content.Headers.LastModified, responseB.Content.Headers.LastModified);
            
            Assert.Equal(contentA.Entry, contentB.Entry);
            Assert.Equal(contentA.Etag, contentB.Etag);
            Assert.Equal(contentA.StatusCode, contentB.StatusCode);
            
            Assert.True(preCacheTimer.ElapsedMilliseconds > postCacheTimer.ElapsedMilliseconds);
            
            _testOutputHelper.WriteLine("Diff: {0} (ms)",preCacheTimer.ElapsedMilliseconds / postCacheTimer.ElapsedMilliseconds);
        }


        [Fact]
        public async Task Etag_Is_Invalidated_On_Not_Get_Routes()
        {
            
            using var factory = new CustomWebApplicationFactory<Startup>();
            var client = factory.CreateClientWithDelegatingHandler();
            var distributedCache = factory.Services.GetRequiredService<IDistributedCache>();
            //cache is empty
            var responseA = await client.GetAllAsync();
            
            //fetch cache key
            var key = responseA.GetCacheKey();
            
            //assert that cache is not empty
            Assert.NotNull(await distributedCache.GetHttpContent(key));
            Assert.NotNull(responseA.Headers.CacheControl);
            var responseB = await client.GetAllAsync();
            Assert.Null(responseB.Headers.CacheControl);
            
            //do something to cause must-revalidate etag
            await client.CreateAsync("some Value");
            
            
            var responseC = await client.GetAllAsync();
            Assert.NotNull(responseC.Headers.CacheControl);
            

        }
    }
}