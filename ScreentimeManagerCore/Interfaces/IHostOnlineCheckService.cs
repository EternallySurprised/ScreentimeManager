using Microsoft.Extensions.Hosting;
using ScreentimeManagerCore.Configuration;

namespace ScreentimeManagerCore.Interfaces
{
    /// <summary>
    /// An interface for a service that checks if a configured host is online and provides the current configuration settings for the online check process.
    /// </summary>
    public interface IHostOnlineCheckService : IHostedService
    {
        /// <summary>
        /// Gets the current configuration settings for the online check process.
        /// </summary>
        OnlineCheckConfiguration CurrentConfiguration { get; }

        /// <summary>
        /// Indicates if the configured host is currently online.
        /// </summary>
        bool HostIsOnline { get; }

        /// <summary>
        /// Called whenever the host status is updated.
        /// </summary>
        event Action<bool>? HostStatusUpdated;
    }
}