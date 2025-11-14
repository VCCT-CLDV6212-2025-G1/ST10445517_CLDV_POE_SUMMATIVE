using ClassLibrary.Models;
using ClassLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly AzureTableStorageService _azureTableStorageService;

        //---------------------------------------------------------------------------------------------------------------------
        public ProductController(BlobService blobService, AzureTableStorageService azureTableStorageService)
        {
            _blobService = blobService;
            _azureTableStorageService = azureTableStorageService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var products = await _azureTableStorageService.GetAllProductsAsync(); 
            return View(products);
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var contentType = file.ContentType;
                var imageUrl = await _blobService.UploadsAsyncFunction(stream, file.FileName, contentType);
                product.ImageUrl = imageUrl;
            }

            if (ModelState.IsValid) 
            {
                product.PartitionKey = "ProductPartition";
                product.RowKey = Guid.NewGuid().ToString();
                await _azureTableStorageService.AddProductAsync(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult Cart()
        {
            // Ensure you have the SessionExtensions.cs file from the previous step!
            var cart = HttpContext.Session.Get<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();

            // We pass the list of CartItem models directly to the view.
            return View(cart);
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public IActionResult AddToCart(string productId, string name, double price, int quantity)
        {
            // get cart from the session or create a new one
            var cart = HttpContext.Session.Get<List<CartItem>>("ShoppingCart") ?? new List<CartItem>();

            // check if the product is already in the cart
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // if product is already in the cart, then the quantity of the that item in the cart will be increased
                existingItem.Quantity += quantity;
            }
            else
            {
                // if the item is new to the cart then a new CartItem will be created and added to the cart
                var newItem = new CartItem
                {
                    ProductId = productId,
                    ProductName = name,
                    Price = price,
                    Quantity = quantity
                };
                cart.Add(newItem);
            }

            // saves the updated cart for the Session
            HttpContext.Session.Set("ShoppingCart", cart);

            TempData["SuccessMessage"] = $"{quantity} x {name} added to cart successfully!";

            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public IActionResult RemoveCartItem(string productId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("ShoppingCart");

            if (cart != null)
            {
                // find the item to remove
                var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                    HttpContext.Session.Set("ShoppingCart", cart);
                    TempData["SuccessMessage"] = $"{itemToRemove.ProductName} removed from cart.";
                }
            }

            return RedirectToAction("Cart");
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public IActionResult UpdateCartItem(string productId, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("ShoppingCart");

            if (cart != null)
            {
                var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

                if (existingItem != null)
                {
                    if (quantity > 0)
                    {
                        // updates the quantity if itnis valid
                        existingItem.Quantity = quantity;
                        HttpContext.Session.Set("ShoppingCart", cart);
                        TempData["SuccessMessage"] = $"Quantity for {existingItem.ProductName} updated to {quantity}.";
                    }
                    else
                    {
                        // if quantity is 0 or less then the item will be removed
                        cart.Remove(existingItem);
                        HttpContext.Session.Set("ShoppingCart", cart);
                        TempData["SuccessMessage"] = $"{existingItem.ProductName} removed from cart.";
                    }
                }
            }

            return RedirectToAction("Cart");
        }
        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            await _azureTableStorageService.DeleteProductAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------