using Microsoft.AspNetCore.Mvc;

namespace ABCRetail_POE.Controllers
{
    public class AdminDashboardController : Controller
    {
        //---------------------------------------------------------------------------------------------------------------------
        public IActionResult Index()
        {
            // check to if Admin is logs in
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        //---------------------------------------------------------------------------------------------------------------------
    }
}
//---------------------------------------------------END OF FILE---------------------------------------------------------------