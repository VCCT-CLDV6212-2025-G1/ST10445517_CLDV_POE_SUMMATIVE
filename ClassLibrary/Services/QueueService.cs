using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using ClassLibrary.Models;

namespace ClassLibrary.Services
{
    
    public class QueueService
    {
        
        private readonly HttpClient _httpClient;
        private readonly string _functionAppUrl;
        private readonly string _functionApiKey;
       
        public QueueService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {

            _httpClient = httpClientFactory.CreateClient();

            // Read URL and Key from Configuration
            _functionAppUrl = configuration["FunctionAppUrl"]
                              ?? throw new ArgumentNullException("FunctionAppUrl is not configured.");
            _functionApiKey = configuration["FunctionApiKey"]
                              ?? throw new ArgumentNullException("FunctionApiKey is not configured.");
        }

        public async Task SendOrderToFunction(Order order)
        {
            string requestUrl = $"{_functionAppUrl}orders/queue?code={_functionApiKey}";

            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, content);

            response.EnsureSuccessStatusCode();
        }

    }
}