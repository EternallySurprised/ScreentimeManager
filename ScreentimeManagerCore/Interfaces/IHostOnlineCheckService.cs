using ScreentimeManagerCore.Configuration;

namespace ScreentimeManagerCore.Interfaces
{
    public interface IHostOnlineCheckService
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