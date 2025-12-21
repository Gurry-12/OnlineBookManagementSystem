using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineBookManagementSystem.Controllers
{
    [Authorize]
    public class SuperAdminController : BaseController
    {

        public SuperAdminController() { }

        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
