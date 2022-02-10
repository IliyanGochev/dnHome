using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dnHomeDashboard.Controllers
{
    public class DHWController : Controller 
    {
        private readonly ILogger<DHWController> _logger;

        public DHWController(ILogger<DHWController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() {
            ViewBag.HasSettings = true;
            return View();
        }
    }
}