using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ClassLibrary.Models;
using ClassLibrary.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetail_POE.Functions
{
    public class CreateCustomerFunction
    {
        private readonly ILogger<CreateCustomerFunction> _logger;
        private readonly AzureTableStorageService _azureTableStorageService;

        public CreateCustomerFunction(
            ILogger<CreateCustomerFunction> logger,
            AzureTableStorageService azureTableStorageService)
        {
            _logger = logger;
            _azureTableStorageService = azureTableStorageService;
        }

        [Function("CreateCustomer")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
        {
            _logger.LogInformation("CreateCustomer function triggered.");

            try
            {
                // Read the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrEmpty(requestBody))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new
                    {
                        error = "Request body is empty",
                        message = "Please provide customer details in the request body"
                    });
                    return badResponse;
                }

                // Deserialize the customer object using System.Text.Json
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var customer = JsonSerializer.Deserialize<Customer>(requestBody, options);

                if (customer == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new
                    {
                        error = "Invalid customer data",
                        message = "Could not parse customer information from request body"
                    });
                    return badResponse;
                }

                // Validate required fields
                if (customer.CustomerID == 0)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new
                    {
                        error = "Validation failed",
                        message = "CustomerID is required and must be greater than 0"
                    });
                    return badResponse;
                }

                if (string.IsNullOrEmpty(customer.CustomerName))
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new
                    {
                        error = "Validation failed",
                        message = "CustomerName is required"
                    });
                    return badResponse;
                }

                // Set PartitionKey and RowKey for Azure Table Storage
                customer.PartitionKey = "CustomersPartition";
                customer.RowKey = Guid.NewGuid().ToString();

                // Add customer to Azure Table Storage
                await _azureTableStorageService.AddCustomerAsync(customer);

                _logger.LogInformation($"Customer {customer.CustomerName} created successfully with RowKey: {customer.RowKey}");

                // Return success response
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    message = "Customer created successfully",
                    customerId = customer.RowKey,
                    customerName = customer.CustomerName,
                    partitionKey = customer.PartitionKey
                });
                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Invalid JSON format",
                    message = ex.Message
                });
                return errorResponse;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Validation error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Validation error",
                    message = ex.Message
                });
                return errorResponse;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError($"Storage error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Storage operation failed",
                    message = "An error occurred while saving the customer"
                });
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new
                {
                    error = "Internal server error",
                    message = "An unexpected error occurred"
                });
                return errorResponse;
            }
        }
    }
}