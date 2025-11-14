using ClassLibrary.Models;
using ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureTableStorageService _azureTableStorageService;
        private readonly CustomerFunctionClient _customerFunctionClient; 
        //---------------------------------------------------------------------------------------------------------------------
        public CustomerController(AzureTableStorageService azureTableStorageService,
            CustomerFunctionClient customerFunctionClient)
        {
            _azureTableStorageService = azureTableStorageService;
            _customerFunctionClient = customerFunctionClient;

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
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Call the new client which talks to Azure Function 2 (HandleNewCustomer)
                    await _customerFunctionClient.AddCustomerViaFunctionAsync(customer);

                    // 2. Set the success message using TempData
                    TempData["SuccessMessage"] = "Customer added and contract filed via Azure Function!";

                    // 3. Redirect to the index page
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // If the function call fails (e.g., connection error, function returns 500)
                    TempData["ErrorMessage"] = $"Error communicating with the Azure Function: {ex.Message}";
                    return View(customer);
                }
            }

            // If model state is invalid, show errors
            TempData["ErrorMessage"] = "Customer data is invalid. Please check the form.";
            return View(customer);
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