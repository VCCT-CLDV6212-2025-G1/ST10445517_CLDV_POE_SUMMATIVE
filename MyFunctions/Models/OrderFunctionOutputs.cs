// Inside your Azure Function project (e.g., in a Models folder or at the top of the file)
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class OrderFunctionOutputs
{
    // 1. The HTTP response (The primary output)
    public HttpResponseData HttpResponse { get; set; }

    // 2. The Queue message (The Output Binding)
    // The QueueOutput attribute goes here.
    [QueueOutput("order-queue", Connection = "AzureWebJobsStorage")]
    public string OrderMessage { get; set; }
}