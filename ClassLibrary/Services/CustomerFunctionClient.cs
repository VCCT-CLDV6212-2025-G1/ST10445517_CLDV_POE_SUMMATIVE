using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using ClassLibrary.Models;
using System.Threading.Tasks;

namespace ClassLibrary.Services
{
    public class CustomerFunctionClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionAppUrl;
        private readonly string _functionApiKey;

        public CustomerFunctionClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _functionAppUrl = configuration["FunctionAppUrl"] ?? throw new ArgumentNullException("FunctionAppUrl not configured.");
            _functionApiKey = configuration["FunctionApiKey"] ?? throw new ArgumentNullException("FunctionApiKey not configured.");
        }

        public async Task AddCustomerViaFunctionAsync(Customer customer)
        {
            string requestUrl = $"{_functionAppUrl}customers?code={_functionApiKey}";

            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }
}