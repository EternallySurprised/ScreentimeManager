using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Models
{
    /// <summary>
    /// Represents the status of a host system.
    /// </summary>
    public class HostStatus
    {
        #region Properties
        /// <summary>
        /// Gets or sets the hostname of the server or endpoint.
        /// </summary>
        public virtual string Hostname { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the host is currently online.
        /// </summary>
        public virtual bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the remaining screentime for the host.
        /// </summary>
        public virtual TimeSpan ScreentimeLeft { get; set; }

        /// <summary>
        /// Gets or sets the total screentime limit for the host.
        /// </summary>
        public virtual TimeSpan ScreentimeLimit { get; set; }

        /// <summary>
        /// Gets the percentage of screentime used.
        /// </summary>
        public virtual double ScreentimeUsedPercent
        {
            get
            {
                double ret = (1 - ScreentimeLeft.Divide(ScreentimeLimit)) * 100.0;
                return double.IsNaN(ret) || double.IsNegative(ret) ? 0.0 : ret;
            }
        } 
        #endregion
    }
}
