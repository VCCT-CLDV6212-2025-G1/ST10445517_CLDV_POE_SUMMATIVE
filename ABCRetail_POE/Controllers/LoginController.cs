using ABCRetail_POE.ViewModels;
using ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlLoginService _loginService;
        
        //---------------------------------------------------------------------------------------------------------------------
        public LoginController(SqlLoginService loginService)
        {
            _loginService = loginService;
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Displays the login form
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _loginService.AuthenticateUser(model.Email, model.Password);

                if (user != null)
                {
                    // Setting up session for authentication
                    HttpContext.Session.SetString("UserEmail", user.CustomerEmail!);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetInt32("UserID", user.CustomerID);

                    // Redirect user based on  their role ("Admin" or "Customer")
                    if (user.Role == "Admin")
                        return RedirectToAction("Index", "AdminDashboard");

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Displays the registration form
        }

        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // register the user with a default "Customer" role
                    //REGISTRATIONS ARE PROHIBITED FOR ADMIN.
                    //Please find the admin login detials in the documentation provided for the CLDV Summative POE
                    await _loginService.RegisterUser(model.Name, model.Email, model.Password, "Customer");

                    // Redirect to login page after successful registration
                    TempData["SuccessMessage"] = "Registration successful! Please log in.";
                    return RedirectToAction("Index", "Login");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Email", ex.Message); // Email already registered
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");
                }
            }
            return View(model);
        }

        //---------------------------------------------------------------------------------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------