using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dnHomeDashboard.Controllers
{
    public class BoilerController : Controller 
    {
        private readonly ILogger<BoilerController> _logger;

        public BoilerController(ILogger<BoilerController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() {
            ViewBag.HasSettings = true;
            return View();
        }
    }
}