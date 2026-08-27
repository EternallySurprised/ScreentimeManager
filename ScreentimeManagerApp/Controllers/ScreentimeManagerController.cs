using Microsoft.AspNetCore.Mvc;
using ScreentimeManagerCore.Models;

namespace ScreentimeManagerApp.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class ScreentimeManagerController : ControllerBase
    {
        private readonly ILogger<ScreentimeManagerController> _logger;

        public ScreentimeManagerController(ILogger<ScreentimeManagerController> logger)
        {
            _logger = logger;
        }

        // GET: api/status
        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            HostStatus status = new HostStatus();

            if (status != null) return Ok(status);

            else return NotFound();
        }
    }
}
