using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("SuperAdmin")) return RedirectToAction("Dashboard", "SuperAdmin");
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("User")) return RedirectToAction("Dashboard", "User");
            }
            return RedirectToAction("PublicList", "Books");
        }

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
