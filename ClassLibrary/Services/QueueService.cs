using Azure.Storage.Queues;

namespace ClassLibrary.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        public QueueService(string connectionString, string queueName)
        {
            _queueClient =new QueueClient(connectionString, queueName);
        }

        public async Task SendMessage(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }
    }
}
