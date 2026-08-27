using Microsoft.AspNetCore.Mvc;
using ScreentimeManagerCore.Models;
using ScreentimeManagerCore.Services;

namespace ScreentimeManagerApp.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class ScreentimeManagerController : ControllerBase
    {
        protected readonly ILogger<ScreentimeManagerController> _logger;
        protected readonly ScreentimeManagerStatemachine _statemachine;

        public ScreentimeManagerController(ILogger<ScreentimeManagerController> logger, ScreentimeManagerStatemachine statemachine)
        {
            _logger = logger;
            _statemachine = statemachine;
        }

        // GET: api/status
        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            HostStatus status = _statemachine.CurrentHostStatus;

            if (status != null) return Ok(status);

            else return NotFound();
        }
    }
}
