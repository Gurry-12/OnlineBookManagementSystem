using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }
    }
}
