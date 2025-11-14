using Azure.Storage.Queues;
using System.Text.Json;

namespace TestQueue
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Your real Azure Storage connection string
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=abc1retailstorage;AccountKey=MdhoejTXw7i//1aEJAQNphoRQ4afRLQwsWTxCqyNVJtJX0Md0kwANSO2ZRnXtbQm5PB6/0ZptSUV+ASt+HHuCw==;EndpointSuffix=core.windows.net";

            // Queue name must match the one your function listens to
            var queueClient = new QueueClient(
            connectionString,
            "orders",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 }); //ensure its base64

            // Create queue if it doesn't exist
            await queueClient.CreateIfNotExistsAsync();
            // Build test object
            var order = new { ProductID = "3", CustomerID= "5" };
            // Serialize object to JSON
            string json = JsonSerializer.Serialize(order);
            // Send as plain JSON string
            await queueClient.SendMessageAsync(json);
            Console.WriteLine($"Message sent: {json}");
        }
    }

}
