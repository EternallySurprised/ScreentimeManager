using ScreentimeManagerCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Interfaces
{
    public interface IWebhookNotifier
    {
        Task NotifyTimeUpAsync(HostStatus status);

        Task NotifyTurnedOnWithoutTimeLeftAsync(HostStatus status);

        Task NotifyHostHasShutdownAsync(HostStatus status);
    }
}
