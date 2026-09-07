using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScreentimeManagerCore.Services;

namespace ScreentimeManagerCore.Configuration
{
    /// <summary>
    /// Configuration class for the <see cref="ScreentimeCounterService">. This class holds the settings related to screentime limits and check intervals.
    /// </summary>
    public class ScreentimeCounterConfiguration
    {
        /// <summary>
        /// Daily screentime limit in minutes
        /// </summary>
        [ConfigurationKeyName("SCREENTIME_LIMIT_MINUTES")]
        public virtual int ScreentimeLimitMinutes { get; set; } = 180;

        /// <summary>
        /// Interval to repeat screentime check in Milliseconds
        /// </summary>
        [ConfigurationKeyName("SCREENTIME_COUNT_INTERVAL_MS")]
        public virtual int CheckInterval { get; set; } = 1000;
    }
}
