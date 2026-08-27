using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Configuration
{
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
