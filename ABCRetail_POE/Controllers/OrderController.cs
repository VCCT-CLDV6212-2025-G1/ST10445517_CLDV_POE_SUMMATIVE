using Microsoft.AspNetCore.Mvc;
using ClassLibrary.Services;
using ClassLibrary.Models;

namespace ABCRetail_POE.Controllers
{
    public class OrderController : Controller
    {
        private readonly AzureTableStorageService _azureTableStorageService;
        private readonly QueueService _queueService;

        //---------------------------------------------------------------------------------------------------------------------
        public OrderController(AzureTableStorageService azureTableStorageService, QueueService queueService)
        {
            _azureTableStorageService = azureTableStorageService;
            _queueService = queueService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        // edited Index Action for Managing Admin Order Management
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var orders = await _azureTableStorageService.GetAllOrdersAsync();
            return View(orders);
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string partitionKey, string rowKey)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                await _azureTableStorageService.UpdateOrderStatusAsync(partitionKey, rowKey, "PROCESSED");
                TempData["SuccessMessage"] = $"Order {rowKey.Substring(0, 8)} status updated to PROCESSED.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to process order: {ex.Message}";
            }

            return RedirectToAction("Index"); // Redirect back to the Admin Order list
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> CreateOrder()
        {
            var customers = await _azureTableStorageService.GetAllCustomerAsync();
            var products = await _azureTableStorageService.GetAllProductsAsync();

            if(customers == null || customers.Count == 0)
            {
                ModelState.AddModelError("", "No Customers found. Please add one");
            }
            if (products == null || products.Count == 0)
            {
                ModelState.AddModelError("", "No products found.Please Add products");
            }

            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View(new Order());
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult>CreateOrder(Order order)
        {
            if(ModelState.IsValid)
            {
                order.PartitionKey = "OrderPartition";
                order.RowKey = Guid.NewGuid().ToString();
                order.Timestamp = DateTime.Now;
                await _azureTableStorageService.AddOrderAsync(order);

                string message = $"New order for {order.CustomerID}"+ $"for {order.ProductID}";
                await _queueService.SendMessage(message);
                return RedirectToAction("Index");
            }

            var customers = await _azureTableStorageService.GetAllCustomerAsync();
            var products = await _azureTableStorageService.GetAllProductsAsync();
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View(order);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------