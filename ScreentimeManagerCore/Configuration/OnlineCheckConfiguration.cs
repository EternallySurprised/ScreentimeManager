using Microsoft.Extensions.Configuration;

namespace ScreentimeManagerCore.Configuration
{
    /// <summary>
    /// Represents the configuration for an Online-Check Service.
    /// </summary>
    public class OnlineCheckConfiguration
    {
        /// <summary>
        /// Hostname of the host to ping
        /// </summary>
        [ConfigurationKeyName("PING_HOST")]
        public virtual string Host { get; set; } = "localhost";

        /// <summary>
        /// Timeout for Online-Check in Milliseconds.
        /// </summary>
        [ConfigurationKeyName("PING_TIMEOUT_MS")]
        public virtual int Timeout { get; set; } = 200;

        /// <summary>
        /// Interval to repeat Online-Check in Milliseconds
        /// </summary>
        [ConfigurationKeyName("PING_INTERVAL_MS")]
        public virtual int CheckInterval { get; set; } = 30000;
    }
}
