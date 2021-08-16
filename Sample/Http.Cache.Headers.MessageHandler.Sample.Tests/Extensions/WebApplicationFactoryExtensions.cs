using Http.Cache.Headers.MessageHandler.Sample.Tests.Setup;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Http.Cache.Headers.MessageHandler.Sample.Tests.Extensions
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