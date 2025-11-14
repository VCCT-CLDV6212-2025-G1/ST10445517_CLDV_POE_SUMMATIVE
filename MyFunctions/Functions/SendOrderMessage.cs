using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ABCRetail_POE.Functions
{

    public static class SendOrderMessage
    {
        [Function("SendOrderMessage")]
        public static async Task<OrderFunctionOutputs> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/queue")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendOrderMessage");
            logger.LogInformation("HTTP trigger processed a request to send an order to the queue.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Order details required.");
                return new OrderFunctionOutputs { HttpResponse = badResponse, OrderMessage = null };
            }

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteStringAsync("Order message successfully queued.");

            return new OrderFunctionOutputs
            {
                HttpResponse = response,
                OrderMessage = requestBody
            };
        }
    }
}