using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ClassLibrary.Services;
using ClassLibrary.Models;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace ABCRetail_POE.Functions
{
    public class ProcessOrderQueueItem
    {
        private readonly ILogger<ProcessOrderQueueItem> _logger;
        private readonly AzureTableStorageService _azureTableStorageService;

        public ProcessOrderQueueItem(
            ILogger<ProcessOrderQueueItem> logger,
            AzureTableStorageService azureTableStorageService)
        {
            _logger = logger;
            _azureTableStorageService = azureTableStorageService;
        }

        [Function("ProcessOrderQueueItem")]
        public async Task Run(
            [QueueTrigger("order-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            _logger.LogInformation($"C# Queue trigger function started processing message: {myQueueItem}");

            try
            {
                var order = JsonSerializer.Deserialize<Order>(myQueueItem,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (order == null)
                {
                    _logger.LogWarning("Queue item was null or could not be deserialized into an Order object.");
                    return;
                }

                order.PartitionKey = order.CustomerID > 0 ? order.CustomerID.ToString() : "DefaultPartition";
                order.RowKey = Guid.NewGuid().ToString();

                // Set initial status
                if (string.IsNullOrEmpty(order.Status))
                {
                    order.Status = "New";
                }

                await _azureTableStorageService.AddOrderAsync(order);

                _logger.LogInformation($"Order {order.RowKey} for Customer {order.CustomerID} successfully written to Azure Table.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"JSON deserialization error in queue trigger. Message: {myQueueItem}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing queue item and writing to table: {ex.Message}");
                throw;
            }
        }
    }
}