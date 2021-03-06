using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Http.Cache.Headers.MessageHandler.Sample.Tests.Setup
{
    public class StoreManipulationClient
    {
        private readonly HttpClient _httpClient;

        public StoreManipulationClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetAsync("api/storemanipulation", cancellationToken);
        }
        
        public async Task CreateAsync(string value, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsync($"api/storemanipulation", JsonContent.Create(value), cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
    
}