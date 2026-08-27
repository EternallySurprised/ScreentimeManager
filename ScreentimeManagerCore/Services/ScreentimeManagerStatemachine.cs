using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreentimeManagerCore.Interfaces;
using ScreentimeManagerCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreentimeManagerCore.Services
{
    public enum States
    {
        HostIsOffline,
        HostIsOnline,
        HostIsBeingShutdown
    }

    public enum Events
    {
        Offline,
        Online,
        ShutdownError
    }

    public class ScreentimeManagerStatemachine : IHostedService
    {
        protected bool _prevHostOnline = false;
        protected readonly ILogger<ScreentimeManagerStatemachine> _logger;
        protected readonly IHostOnlineCheckService _hostOnlineCheckService;
        protected readonly IRemoteShutdownService _remoteShutdownService;
        protected readonly ScreentimeCounterService _counterService;
        protected readonly PassiveStateMachine<States, Events> _stateMachine;

        public HostStatus CurrentHostStatus
        {
            get
            {
                var result = new HostStatus()
                {
                    Hostname = _hostOnlineCheckService.CurrentConfiguration.Host,
                    ScreentimeLeft = _counterService.RemainingScreentime,
                    ScreentimeLimit = _counterService.ScreentimeLimit,
                    IsOnline = _hostOnlineCheckService.HostIsOnline
                };

                return result;
            }
        }

        public ScreentimeManagerStatemachine(ILogger<ScreentimeManagerStatemachine> logger, IHostOnlineCheckService hostOnlineCheckService, IRemoteShutdownService remoteShutdownService, ScreentimeCounterService counterService)
        {
            _logger = logger;
            _hostOnlineCheckService = hostOnlineCheckService;
            _hostOnlineCheckService.HostStatusUpdated += OnHostStatusUpdated;
            _remoteShutdownService = remoteShutdownService;
            _counterService = counterService;

            var builder = new StateMachineDefinitionBuilder<States, Events>();

            // Configure State Graph
            builder.In(States.HostIsOffline)
                .On(Events.Online)
                    .If(CheckTimeUp).Goto(States.HostIsBeingShutdown)
                        .Execute(this.NotifyTurnedOnWithoutTimeLeft)
                    .Otherwise().Goto(States.HostIsOnline);

            builder.In(States.HostIsOnline)
               .On(Events.Offline).Goto(States.HostIsOffline)
               .On(Events.Online)
                    .If(CheckTimeUp).Goto(States.HostIsBeingShutdown)
                        .Execute(this.NotifyTimeUp);

            builder.In(States.HostIsBeingShutdown)
                .ExecuteOnEntry(this.ExecuteShutdown)
                .On(Events.Offline).Goto(States.HostIsOffline)
                    .Execute(this.NotifyHostHasShutdown)
                .On(Events.ShutdownError).Goto(States.HostIsOnline);

            // Build State Graph
            builder.WithInitialState(States.HostIsOffline);
            var definition = builder.Build();

            // Create FSM
            _stateMachine = definition.CreatePassiveStateMachine("ScreentimeManager");
            _counterService = counterService;
        }

        /// <summary>
        /// Called whenever the host online status is updated
        /// </summary>
        /// <param name="isOnline">Latest online status</param>
        protected void OnHostStatusUpdated(bool isOnline)
        {
            if (isOnline)
            {
                _stateMachine.Fire(Events.Online);
            }
            else
            {
                _stateMachine.Fire(Events.Offline);
            }

            // Log host state change
            if (_prevHostOnline && !isOnline)
            {
                _logger.LogInformation("Host went offline.");
            }
            else if (!_prevHostOnline && isOnline)
            {
                _logger.LogInformation("Host went online.");
            }

            _prevHostOnline = isOnline;
        }

        /// <summary>
        /// Checks if the screentime is used up.
        /// </summary>
        /// <returns></returns>
        protected bool CheckTimeUp()
        {
            return _counterService.ScreentimeExceeded;
        }

        protected void ExecuteShutdown()
        {
            try
            {
                _logger.LogInformation("Shutdown is being executed...");
                _remoteShutdownService.ShutdownHost("Screentime is exceeded.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shutdown failed");
                _stateMachine.Fire(Events.ShutdownError);
            }
        }

        protected void NotifyTimeUp()
        {
            _logger.LogInformation("Screentime for host is used up.");
            // Implement the notification logic here
        }

        protected void NotifyTurnedOnWithoutTimeLeft()
        {
            _logger.LogInformation("Host turned on without time left.");
            // Implement the notification logic here
        }

        protected void NotifyHostHasShutdown()
        {
            _logger.LogInformation("Host has shutdown.");
            // Implement the notification logic here
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _stateMachine.Start();
            _logger.LogInformation("Statemachine started.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stateMachine.Stop();
            _logger.LogInformation("Statemachine stopped.");

            return Task.CompletedTask;
        }
    }
}
