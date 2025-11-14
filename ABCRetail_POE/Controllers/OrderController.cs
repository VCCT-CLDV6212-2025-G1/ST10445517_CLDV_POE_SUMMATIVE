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
        public async Task<IActionResult> UpdateStatus(string partitionKey, string rowKey, string newStatus)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {

                await _azureTableStorageService.UpdateOrderStatusAsync(partitionKey, rowKey, newStatus);

                TempData["SuccessMessage"] = $"Order {rowKey.Substring(0, 8)} status updated to {newStatus}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to update status: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            // authentication check
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to complete checkout.";
                return RedirectToAction("Index", "Login");
            }

            // retrieves cart and user ID
            var cart = HttpContext.Session.Get<List<CartItem>>("ShoppingCart");
            int customerId = HttpContext.Session.GetInt32("UserID").GetValueOrDefault();

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Your shopping cart is empty.";
                return RedirectToAction("Cart", "Product");
            }

            // processes cart items into Orders
            try
            {
                decimal grandTotal = 0;

                foreach (var item in cart)
                {
                    // creates new Order entity for each product in the cart
                    var newOrder = new Order
                    {
                        PartitionKey = customerId.ToString(),
                        RowKey = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.Now,

                        // Order details
                        CustomerID = customerId,
                        ProductID = GetProductIdAsInt(item.ProductId),
                        Status = "PENDING",
                        Quantity = item.Quantity,
                        TotalAmount = item.Subtotal
                    };

                    
                    await _queueService.SendOrderToFunction(newOrder);

                    grandTotal += item.Subtotal;
                }

                // clear the Session Cart
                HttpContext.Session.Remove("ShoppingCart");

                TempData["SuccessMessage"] = $"Your order (Total R {grandTotal:F2}) has been successfully submitted and queued for processing!";

                return RedirectToAction("CustomerOrders", "Order");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during checkout: {ex.Message}";
                return RedirectToAction("Cart", "Product");
            }
        }
        //---------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> CreateOrder()
        {
            var customers = await _azureTableStorageService.GetAllCustomerAsync();
            var products = await _azureTableStorageService.GetAllProductsAsync();

            if (customers == null || customers.Count == 0)
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
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                order.PartitionKey = "OrderPartition";
                order.RowKey = Guid.NewGuid().ToString();
                order.Timestamp = DateTime.Now;

                await _queueService.SendOrderToFunction(order);

                TempData["SuccessMessage"] = $"New order created and sent for processing!";
                return RedirectToAction("Index");
            }

            var customers = await _azureTableStorageService.GetAllCustomerAsync();
            var products = await _azureTableStorageService.GetAllProductsAsync();
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View(order);
        }

        private int GetProductIdAsInt(string? productIdString)
        {
            if (string.IsNullOrEmpty(productIdString)) return 0;
            if (int.TryParse(productIdString, out int productId))
            {
                return productId;
            }
            return 0;
        }

        public async Task<IActionResult> CustomerOrders()
        {
            var customerId = HttpContext.Session.GetInt32("UserID");
            if (customerId == null || customerId == 0)
            {
                TempData["ErrorMessage"] = "Please log in to view your orders.";
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var customerOrders = await _azureTableStorageService.GetOrdersByCustomerIdAsync(customerId.Value);
                return View(customerOrders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not retrieve your orders: {ex.Message}";
                return View(new List<ClassLibrary.Models.Order>());
            }
        }
    }
}