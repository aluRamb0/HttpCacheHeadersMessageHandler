using Cache.Headers.Delegating.Handler.Sample.Tests.Setup;
using Cache.Headers.DelegatingHandler;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cache.Headers.Delegating.Handler.Sample.Tests.Extensions
{
    public static class WebApplicationFactoryExtensions
    {
        public static StoreManipulationClient CreateClientWithDelegatingHandler(this WebApplicationFactory<Startup> factory)
        {
            var httpClient =
                factory.CreateDefaultClient(factory.Services.GetRequiredService<HttpCacheHeadersDelegatingHandler>());
            return new StoreManipulationClient(httpClient);
        }
    }
}