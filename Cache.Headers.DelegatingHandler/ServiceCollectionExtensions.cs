using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cache.Headers.DelegatingHandler
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="HttpCacheHeadersDelegatingHandler"/> as a transient dependency.
        /// </summary>
        /// <remarks>This requires a <see cref="IDistributedCache"/></remarks>
        public static IServiceCollection AddHttpCacheHeadersMessageHandler(this IServiceCollection services, Action<HttpCacheHeadersDelegatingHandlerOptions> optionsAction = null)
        {
            var options = new HttpCacheHeadersDelegatingHandlerOptions();

            optionsAction?.Invoke(options);
            
            services.AddTransient(provider => 
                new HttpCacheHeadersDelegatingHandler(
                    provider.GetRequiredService<ILoggerFactory>(),
                    provider.GetRequiredService<IDistributedCache>(), options));

            return services;
        }
    }
}