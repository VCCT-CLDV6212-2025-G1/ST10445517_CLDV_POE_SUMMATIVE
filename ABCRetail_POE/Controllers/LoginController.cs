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