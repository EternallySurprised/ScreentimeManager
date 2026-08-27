using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Configuration
{
    public class WebhookNotifierConfiguration
    {
        /// <summary>
        /// Webhook URL to use
        /// </summary>
        [ConfigurationKeyName("WEBHOOK_URL")]
        public virtual string WebhookUrl { get; set; } = "https://localhost";

        /// <summary>
        /// Culture to use for formatting
        /// </summary>
        [ConfigurationKeyName("CULTURE")]
        public virtual string Culture { get; set; } = "de-De";
    }
}
