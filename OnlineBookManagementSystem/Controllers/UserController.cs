using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public UserController() { }

        [Authorize(Policy = "UserOrHigher")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
