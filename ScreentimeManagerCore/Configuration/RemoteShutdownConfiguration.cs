using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Configuration
{
    public class RemoteShutdownConfiguration
    {
        /// <summary>
        /// Hostname of the host to shut down.
        /// </summary>
        [ConfigurationKeyName("PING_HOST")]
        public virtual string Host { get; set; } = "localhost";

        /// <summary>
        /// Username of the user that has shutdown rights on the host.
        /// </summary>
        [ConfigurationKeyName("HOST_USER")]
        public virtual string Username { get; set; } = "Administrator";
    }
}
