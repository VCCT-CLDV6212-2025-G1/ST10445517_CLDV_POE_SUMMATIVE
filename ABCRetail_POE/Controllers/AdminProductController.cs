using ClassLibrary.Services;
using ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly AzureTableStorageService _azureTableStorageService;

        //---------------------------------------------------------------------------------------------------------------------
        public AdminProductController(BlobService blobService, AzureTableStorageService azureTableStorageService)
        {
            _blobService = blobService;
            _azureTableStorageService = azureTableStorageService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        // helps authenticaticate the admin login
        private IActionResult AdminCheck()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            return Ok();
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Admin product list
        public async Task<IActionResult> Index()
        {
            var check = AdminCheck();
            if (check is RedirectToActionResult) 
            { 
                return check; 
            }

            var products = await _azureTableStorageService.GetAllProductsAsync();
            return View(products);
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Add product form (GET)
        [HttpGet]
        public IActionResult AddProduct()
        {
            var check = AdminCheck();
            if (check is RedirectToActionResult)
            {
                return check;
            }
            return View();
        }

        //---------------------------------------------------------------------------------------------------------------------
        // handles adding products (POST)
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            var check = AdminCheck();
            if (check is RedirectToActionResult)
            { 
                return check; 
            }

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
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        //---------------------------------------------------------------------------------------------------------------------
        // Delete Product (POST)
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            var check = AdminCheck();
            if (check is RedirectToActionResult) 
            { 
                return check; 
            }

            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            await _azureTableStorageService.DeleteProductAsync(partitionKey, rowKey);
            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------