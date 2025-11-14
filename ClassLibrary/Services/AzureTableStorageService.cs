using Azure.Data.Tables;
using Azure;
using System.Drawing;
using ClassLibrary.Models;

namespace ClassLibrary.Services
{
    public class AzureTableStorageService
    {
        public readonly TableClient _customerTableClient; // For customer table
        public readonly TableClient _productTableClient; // For product table
        public readonly TableClient _orderTableClient; // For order table

        //---------------------------------------------------------------------------------------------------------------------
        public AzureTableStorageService(string connectionString)
        {
            _customerTableClient = new TableClient(connectionString, "Customer");
            _productTableClient = new TableClient(connectionString, "Product");
            _orderTableClient = new TableClient(connectionString, "orders");
            
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<List<Customer>> GetAllCustomerAsync()
        {
            var customers = new List<Customer>();

            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _customerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table storage", ex);
            }

        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task AddCustomerAsyncFunction(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            // Validate required fields before making the API call
            if (customer.CustomerID == 0)
                throw new ArgumentException("CustomerID must be greater than 0");
            if (string.IsNullOrEmpty(customer.CustomerName))
                throw new ArgumentException("CustomerName cannot be empty");

            // Base URL for the Azure Function (can come from configuration)
            string baseUrl = Environment.GetEnvironmentVariable("FunctionAppUrl");
                             

            using var httpClient = new HttpClient();

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(customer);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Azure Function call failed: {response.StatusCode}, Details: {errorDetails}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Error calling Azure Function endpoint", ex);
            }

        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to Table storage", ex);
            }

        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

       public async Task AddOrderAsync(Order order)
        {
            if(string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }
            try
            {
                await _orderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order to storage", ex);
            }

        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            await foreach(var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }

            return orders;
        }

        //--------------------------------------------------------------------------------------------------------------------
        public async Task UpdateOrderStatusAsync(string partitionKey, string rowKey, string newStatus)
        {
            // GetS the existing order entity
            Response<Order> response = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);
            Order orderToUpdate = response.Value;

            // ModifIES the status
            orderToUpdate.Status = newStatus;

            // updates the entity in Azure Table Storage
            // ensures the entity is overwritten
            await _orderTableClient.UpdateEntityAsync(orderToUpdate, orderToUpdate.ETag, TableUpdateMode.Replace);
        }

        //--------------------------------------------------------------------------------------------------------------------
        public async Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            string filter = $"PartitionKey eq '{customerId}'";

            var queryResults = _orderTableClient.QueryAsync<Order>(filter: filter);

            var orders = new List<Order>();
            await foreach (var entity in queryResults)
            {
                orders.Add(entity);
             }

            return orders;
        }
        //--------------------------------------------------------------------------------------------------------------------
    }
}

//--------------------------------------------------------------END OF FILE----------------------------------------------------------