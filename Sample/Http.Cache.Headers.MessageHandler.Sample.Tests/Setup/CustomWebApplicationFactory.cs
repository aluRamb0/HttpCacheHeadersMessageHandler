using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Cache.Headers.MessageHandler.Sample.Tests.Setup
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup: class
    {

        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {

                services.AddDistributedMemoryCache();
                services.AddHttpCacheHeadersMessageHandler(options =>
                {
                    options.IgnoreNonGetRequest = true;
                    options.CacheEntryPrefix = Constants.CacheEntryPrefix;
                });
            });
            
        }
    }

    public class Constants
    {
        
        public const string CacheEntryPrefix = "EtagTesting";
    }
}