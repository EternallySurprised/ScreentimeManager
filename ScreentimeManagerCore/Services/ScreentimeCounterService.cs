using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreentimeManagerCore.Configuration;
using ScreentimeManagerCore.Interfaces;
using ScreentimeManagerCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Services
{
    public class ScreentimeCounterService : BackgroundService
    {
        #region Fields
        protected const int FALLBACK_SCREENTIME_LIMIT = 180;
        protected readonly ILogger<ScreentimeCounterService> _logger;
        protected readonly IOptions<ScreentimeCounterConfiguration> _config;
        protected readonly IHostOnlineCheckService _onlineCheckService;
        protected readonly int _interval;
        protected DateTime _currentDay;
        protected DateTime _lastCheckTime;
        #endregion

        #region Properties
        public TimeSpan RemainingScreentime { get; protected set; }

        /// <summary>
        /// The configured screentime limit in minutes. If the configuration is invalid, the fallback value of <see cref="FALLBACK_SCREENTIME_LIMIT"/> is used.
        /// </summary>
        public TimeSpan ScreentimeLimit
        {
            get
            {
                return new TimeSpan(0, _config.Value.ScreentimeLimitMinutes >= 0 ? _config.Value.ScreentimeLimitMinutes : FALLBACK_SCREENTIME_LIMIT, 0);
            }
        }

        /// <summary>
        /// Indicates that the allowed screentime has been exceeded.
        /// </summary>
        public bool ScreentimeExceeded
        {
            get
            {
                return RemainingScreentime <= TimeSpan.Zero;
            }
        }
        #endregion

        #region Constructors
        public ScreentimeCounterService(ILogger<ScreentimeCounterService> logger, IOptions<ScreentimeCounterConfiguration> config, IHostOnlineCheckService onlineCheckService)
        {
            _logger = logger;
            _config = config;
            _onlineCheckService = onlineCheckService;

            _interval = _config.Value.CheckInterval > 0 ? _config.Value.CheckInterval : 1000;
            RemainingScreentime = _config.Value.ScreentimeLimitMinutes >= 0 ? new TimeSpan(0, _config.Value.ScreentimeLimitMinutes, 0) : new TimeSpan(0, 180, 0);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds the specified number of minutes to the remaining screentime. This can be used to manually increase the allowed screentime.
        /// </summary>
        /// <param name="minutes"></param>
        public void AddScreentime(int minutes)
        {
            if (minutes > 0)
            {
                RemainingScreentime = RemainingScreentime.Add(new TimeSpan(0, minutes, 0));
                _logger.LogInformation($"Added {minutes} minutes to screentime. New remaining screentime: {RemainingScreentime.ToString()}");
            }
            else
            {
                _logger.LogWarning($"Attempted to add non-positive screentime: {minutes} minutes. No changes made.");
            }
        }

        /// <summary>
        /// Subtracts the specified number of minutes from the remaining screentime. This can be used to manually reduce the allowed screentime.
        /// </summary>
        /// <param name="minutes">Amount of minutes to subtract from remaining screentime</param>
        public void SubtractScreentime(int minutes)
        {
            if (minutes > 0)
            {
                RemainingScreentime = RemainingScreentime.Subtract(new TimeSpan(0, minutes, 0));
                _logger.LogInformation($"Subtracted {minutes} minutes from screentime. New remaining screentime: {RemainingScreentime.ToString()}");
            }
            else
            {
                _logger.LogWarning($"Attempted to subtract non-positive screentime: {minutes} minutes. No changes made.");
            }
        }

        /// <summary>
        /// Sets the remaining screentime to zero, effectively ending the allowed screentime. This can be used to manually end the screentime countdown.
        /// </summary>
        public void EndScreentime()
        {
            RemainingScreentime = TimeSpan.Zero;
            _logger.LogInformation($"Remaining screentime manually set to zero.");
        }

        /// <summary>
        /// Resets the remaining screentime to the configured screentime limit. This can be used to manually reset the screentime counter, for example at the start of a new day or after a specific event.
        /// </summary>
        public void ResetScreentime()
        {
            RemainingScreentime = ScreentimeLimit;
            _logger.LogInformation($"Remaining screentime reset to {ScreentimeLimit}.");
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executed when the background service is started. This method runs in a loop, checking the remaining screentime and updating it based on the host's online status. 
        /// If the host is online, the remaining screentime is decremented based on the elapsed time since the last check. 
        /// If the host is offline, the countdown is paused. The remaining screentime is reset at the start of a new day.
        /// </summary>
        /// <param name="stoppingToken"><see cref="CancellationToken"/> for graceful task cancellation.</param>
        /// <returns><see cref="Task"/> reference.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime today = DateTime.Today.Date;

                    // Reset on new day
                    if (_currentDay != today)
                    {
                        RemainingScreentime = ScreentimeLimit;
                        _logger.LogInformation($"Today is {today.ToString("d")}. Screentime reset.");
                        _currentDay = today;
                    }
                    else if (_onlineCheckService.HostIsOnline)
                    {
                        TimeSpan passedTime = DateTime.Now - _lastCheckTime;
                        RemainingScreentime = RemainingScreentime.Subtract(passedTime);
                        _logger.LogDebug($"Remaining Screentime: {RemainingScreentime.ToString()}");
                    }
                    else
                    {
                        _logger.LogDebug($"Host is offline. Screentime countdown stopped.");
                    }

                    // Save last check time
                    _lastCheckTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while updating remaining screentime. {ex.Message}");
                }
                finally
                {
                    // Wait for a specified interval before checking again
                    await Task.Delay(TimeSpan.FromMilliseconds(_interval));
                }
            }

        } 
        #endregion
    }
}
