using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Controllers
{
    public class HomeController : BaseController
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
