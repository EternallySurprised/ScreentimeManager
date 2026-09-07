using Microsoft.AspNetCore.Mvc;
using ScreentimeManagerCore.Models;
using ScreentimeManagerCore.Services;

namespace ScreentimeManagerApp.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : ControllerBase
    {
        #region Fields
        protected readonly ILogger<StatusController> _logger;
        protected readonly ScreentimeManagerStatemachine _statemachine;
        #endregion

        #region Constructors
        public StatusController(ILogger<StatusController> logger, ScreentimeManagerStatemachine statemachine)
        {
            _logger = logger;
            _statemachine = statemachine;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the current status of the host.
        /// </summary>
        /// <returns>Current host status as <see cref="HostStatus"/></returns>
        [HttpGet]
        public async Task<ActionResult<HostStatus>> GetStatus()
        {
            HostStatus status = _statemachine.CurrentHostStatus;

            if (status == null)
            {
                _logger.LogError("Current host status could not be retrieved.");
                return NotFound();
            }
            else
            {
                // Ignore milliseconds for API responses
                status.ScreentimeLeft = new TimeSpan(status.ScreentimeLeft.Hours, status.ScreentimeLeft.Minutes, status.ScreentimeLeft.Seconds);
                status.ScreentimeLimit = new TimeSpan(status.ScreentimeLimit.Hours, status.ScreentimeLimit.Minutes, status.ScreentimeLimit.Seconds);
                return Ok(status);
            }
        } 
        #endregion
    }
}
