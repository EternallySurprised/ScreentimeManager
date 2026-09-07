using ScreentimeManagerCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Interfaces
{
    /// <summary>
    /// An interface for sending notifications via webhooks, such as Discord or other services, regarding the status of the host machine in relation to screentime management.
    /// </summary>
    public interface IWebhookNotifier
    {
        /// <summary>
        /// Send notification for "Screentime used up"
        /// </summary>
        /// <param name="status">Status information for host</param>
        /// <returns>Task</returns>
        Task NotifyTimeUpAsync(HostStatus status);

        /// <summary>
        /// Send notification for "Host rebooted after screentime was used up"
        /// </summary>
        /// <param name="status">Status information for host</param>
        /// <returns>Task</returns>
        Task NotifyTurnedOnWithoutTimeLeftAsync(HostStatus status);

        /// <summary>
        /// Send notification for "Host was shutdown successfully"
        /// </summary>
        /// <param name="status">Status information for host</param>
        /// <returns>Task</returns>
        Task NotifyHostHasShutdownAsync(HostStatus status);
    }
}
