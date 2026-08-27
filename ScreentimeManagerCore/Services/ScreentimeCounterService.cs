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
        protected const int FALLBACK_SCREENTIME_LIMIT = 180;
        protected readonly ILogger<ScreentimeCounterService> _logger;
        protected readonly IOptions<ScreentimeCounterConfiguration> _config;
        protected readonly IHostOnlineCheckService _onlineCheckService;
        protected readonly int _interval;
        protected DateTime _currentDay;
        protected DateTime _lastCheckTime;

        public TimeSpan RemainingScreentime { get; protected set; }

        public TimeSpan ScreentimeLimit
        {
            get
            {
                return new TimeSpan(0, _config.Value.ScreentimeLimitMinutes >= 0 ? _config.Value.ScreentimeLimitMinutes : FALLBACK_SCREENTIME_LIMIT, 0);
            }
        }

        public bool ScreentimeExceeded
        {
            get
            {
                return RemainingScreentime <= TimeSpan.Zero;
            }
        }

        public ScreentimeCounterService(ILogger<ScreentimeCounterService> logger, IOptions<ScreentimeCounterConfiguration> config, IHostOnlineCheckService onlineCheckService)
        {
            _logger = logger;
            _config = config;
            _onlineCheckService = onlineCheckService;

            _interval = _config.Value.CheckInterval > 0 ? _config.Value.CheckInterval : 1000;
            RemainingScreentime = _config.Value.ScreentimeLimitMinutes >= 0 ? new TimeSpan(0, _config.Value.ScreentimeLimitMinutes, 0) : new TimeSpan(0, 180, 0);
        }

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
                        _logger.LogInformation($"Remaining Screentime: {RemainingScreentime.ToString()}");
                    }
                    else
                    {
                        _logger.LogInformation($"Host is offline. Screentime countdown stopped.");
                    }

                    // Save last check time
                    _lastCheckTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking host status.");
                }
                finally
                {
                    // Wait for a specified interval before checking again
                    await Task.Delay(TimeSpan.FromMilliseconds(_interval));
                }
            }

        }
    }
}
