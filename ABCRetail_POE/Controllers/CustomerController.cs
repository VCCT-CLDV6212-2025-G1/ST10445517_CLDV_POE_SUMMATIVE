using ClassLibrary.Models;
using ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureTableStorageService _azureTableStorageService;

        //---------------------------------------------------------------------------------------------------------------------
        public CustomerController(AzureTableStorageService azureTableStorageService)
        {
            _azureTableStorageService = azureTableStorageService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var customers = await _azureTableStorageService.GetAllCustomerAsync();
            return View(customers);
        }

        //---------------------------------------------------------------------------------------------------------------------
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _azureTableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult>AddCustomer(Customer customer)
        {
            customer.PartitionKey = "CustomersPartition";
            customer.RowKey = Guid.NewGuid().ToString();

            await _azureTableStorageService.AddCustomerAsync(customer);
            return RedirectToAction("Index");
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------