using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreentimeManagerCore.Configuration;
using ScreentimeManagerCore.Interfaces;
using System.Net.NetworkInformation;

namespace ScreentimeManagerCore.Services
{
    /// <summary>
    /// Background service that regularly checks, if a host on the network is online.
    /// </summary>
    public class PingHostOnlineCheckService : BackgroundService, IHostOnlineCheckService
    {
        protected const int FALLBACK_TIMEOUT = 200;
        protected const int FALLBACK_INTERVAL = 30000;
        protected const string FALLBACK_HOSTNAME = "localhost";
        protected readonly ILogger<PingHostOnlineCheckService> _logger;
        protected readonly IOptions<OnlineCheckConfiguration> _config;
        protected readonly string _hostToPing = "localhost";
        protected readonly int _pingTimeoutMs = 200;
        protected readonly int _pingIntervalMs = 30000;
        protected bool _hostIsOnline = false;
        protected bool _oldHostIsOnline = false;

        /// <summary>
        /// Indicates if the configured host is currently online.
        /// </summary>
        public virtual bool HostIsOnline { get => _hostIsOnline; protected set => _hostIsOnline = value; }

        /// <summary>
        /// Gets the current configuration settings for the online check process.
        /// </summary>
        public virtual OnlineCheckConfiguration CurrentConfiguration { get => _config.Value; }

        /// <summary>
        /// Called whenever the host status is updated.
        /// </summary>
        public event Action<bool>? HostStatusUpdated;

        /// <summary>
        /// Creates a new instance of the <see cref="PingHostOnlineCheckService"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public PingHostOnlineCheckService(ILogger<PingHostOnlineCheckService> logger, IOptions<OnlineCheckConfiguration> config)
        {
            _logger = logger;
            _config = config;
            _hostToPing = _config.Value.Host ?? FALLBACK_HOSTNAME;
            _pingTimeoutMs = _config.Value.Timeout > 0 ? _config.Value.Timeout : FALLBACK_TIMEOUT;
            _pingIntervalMs = _config.Value.CheckInterval > 0 ? _config.Value.CheckInterval : FALLBACK_INTERVAL;

            _logger.LogDebug($"HostOnlineCheckService instantiated.\r\nHostname: {_hostToPing}\r\nTimeout: {_pingTimeoutMs}\r\nInterval: {_pingIntervalMs}");
        }

        /// <summary>
        /// Is called for every host status update and invokes <see cref="HostStatusUpdated"/>
        /// </summary>
        /// <param name="isOnline">Current online status.</param>
        protected virtual void OnHostStatusUpdated(bool isOnline)
        {
            HostStatusUpdated?.Invoke(isOnline);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (Ping pingSender = new Ping())
            {
                PingOptions _pingOptions = new PingOptions() { DontFragment = true };

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Use ping to check if host is online
                        HostIsOnline = (await pingSender.SendPingAsync(_hostToPing, _pingTimeoutMs)).Status == IPStatus.Success;

                        // Log the status change only if it has changed
                        if (HostIsOnline != _oldHostIsOnline)
                        {
                            _logger.LogInformation(HostIsOnline ? $"{_hostToPing} is online." : $"{_hostToPing} is offline.");
                            _oldHostIsOnline = HostIsOnline;
                        }

                        // Notify subscribers
                        OnHostStatusUpdated(HostIsOnline);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while checking host status.");
                    }
                    finally
                    {
                        // Wait for a specified interval before checking again
                        await Task.Delay(TimeSpan.FromMilliseconds(_pingIntervalMs));
                    }
                }
            }
        }
    }
}
