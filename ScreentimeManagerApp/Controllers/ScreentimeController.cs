using Microsoft.AspNetCore.Mvc;
using ScreentimeManagerCore.Models;
using ScreentimeManagerCore.Services;

namespace ScreentimeManagerApp.Controllers
{
    [ApiController]
    [Route("api/screentime")]
    public class ScreentimeController : ControllerBase
    {
        protected readonly ILogger<StatusController> _logger;
        protected readonly ScreentimeCounterService _counterService;

        public ScreentimeController(ILogger<StatusController> logger, ScreentimeCounterService counterService)
        {
            _logger = logger;
            _counterService = counterService;
        }

        // GET: api/screentime/add/{minutes}
        [HttpPut("add/{minutes}")]
        public async Task<IActionResult> AddScreentime(int minutes)
        {
            try
            {
                _counterService.AddScreentime(minutes);
                return Ok(new TimeSpan(_counterService.RemainingScreentime.Hours, _counterService.RemainingScreentime.Minutes, _counterService.RemainingScreentime.Seconds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding screentime: {ex.Message}");
            }
            
        }
    }
}
