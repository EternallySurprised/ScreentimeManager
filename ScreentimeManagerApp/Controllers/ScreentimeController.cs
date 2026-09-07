using Microsoft.AspNetCore.Mvc;
using ScreentimeManagerCore.Models;
using ScreentimeManagerCore.Services;

namespace ScreentimeManagerApp.Controllers
{
    [ApiController]
    [Route("api/screentime")]
    public class ScreentimeController : ControllerBase
    {
        #region Fields
        protected readonly ILogger<StatusController> _logger;
        protected readonly ScreentimeCounterService _counterService;
        #endregion

        #region Constructors
        public ScreentimeController(ILogger<StatusController> logger, ScreentimeCounterService counterService)
        {
            _logger = logger;
            _counterService = counterService;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Endpoint to add screentime in minutes to the current remaining screentime. Returns the updated remaining screentime as a TimeSpan.
        /// </summary>
        /// <param name="minutes">Number of minutes to add to remaining screentime</param>
        /// <returns></returns>
        [HttpPut("add/{minutes}")]
        [Produces("application/json")]
        public async Task<ActionResult<TimeSpan>> AddScreentime(int minutes)
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

        /// <summary>
        /// Subtracts the specified number of minutes from the remaining screentime.
        /// </summary>
        /// <param name="minutes">Number of minutes to subtract from remaining screentime</param>
        /// <returns>The updated remaining screentime as a <see cref="TimeSpan"></returns>
        [HttpPut("subtract/{minutes}")]
        [Produces("application/json")]
        public async Task<ActionResult<TimeSpan>> SubtractScreentime(int minutes)
        {
            try
            {
                _counterService.SubtractScreentime(minutes);
                return Ok(new TimeSpan(_counterService.RemainingScreentime.Hours, _counterService.RemainingScreentime.Minutes, _counterService.RemainingScreentime.Seconds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while subtracting screentime: {ex.Message}");
            }

        }

        /// <summary>
        /// Sets the remaining screentime to zero. Returns the updated remaining screentime as a TimeSpan.
        /// </summary>
        /// <returns>>The updated remaining screentime as a <see cref="TimeSpan"></returns>
        [HttpPut("end")]
        [Produces("application/json")]
        public async Task<ActionResult<TimeSpan>> EndScreentime()
        {
            try
            {
                _counterService.EndScreentime();
                return Ok(new TimeSpan(_counterService.RemainingScreentime.Hours, _counterService.RemainingScreentime.Minutes, _counterService.RemainingScreentime.Seconds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while setting screentime to zero: {ex.Message}");
            }

        }

        /// <summary>
        /// Resets the remaining screentime to the configured maximum screentime limit.
        /// </summary>
        /// <returns>>The updated remaining screentime as a <see cref="TimeSpan"></returns>
        [HttpPut("reset")]
        [Produces("application/json")]
        public async Task<ActionResult<TimeSpan>> ResetScreentime()
        {
            try
            {
                _counterService.ResetScreentime();
                return Ok(new TimeSpan(_counterService.RemainingScreentime.Hours, _counterService.RemainingScreentime.Minutes, _counterService.RemainingScreentime.Seconds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while resetting screentime to configured maximum: {ex.Message}");
            }

        } 
        #endregion
    }
}
