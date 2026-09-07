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
    /// <summary>
    /// Available states for the screentime manager state machine.
    /// </summary>
    public enum States
    {
        HostIsOffline,
        HostIsOnline,
        HostIsBeingShutdown
    }

    /// <summary>
    /// Available events for the screentime manager state machine.
    /// </summary>
    public enum Events
    {
        Offline,
        Online,
        ShutdownError
    }

    /// <summary>
    /// Statemachine implementation for the screentime manager. This class is responsible for managing the state of the host based on its online status and screentime usage.
    /// </summary>
    public class ScreentimeManagerStatemachine : IHostedService
    {
        #region Fields
        protected bool _prevHostOnline = false;
        protected readonly ILogger<ScreentimeManagerStatemachine> _logger;
        protected readonly IHostOnlineCheckService _hostOnlineCheckService;
        protected readonly IRemoteShutdownService _remoteShutdownService;
        protected readonly ScreentimeCounterService _counterService;
        protected readonly IWebhookNotifier _webhookNotifier;
        protected readonly PassiveStateMachine<States, Events> _stateMachine;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current status of the host.
        /// </summary>
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
        #endregion

        #region Constructors
        public ScreentimeManagerStatemachine(
           ILogger<ScreentimeManagerStatemachine> logger,
           IHostOnlineCheckService hostOnlineCheckService,
           IRemoteShutdownService remoteShutdownService,
           IWebhookNotifier webhookNotifier,
           ScreentimeCounterService counterService)
        {
            _logger = logger;
            _hostOnlineCheckService = hostOnlineCheckService;
            _hostOnlineCheckService.HostStatusUpdated += OnHostStatusUpdated;
            _remoteShutdownService = remoteShutdownService;
            _webhookNotifier = webhookNotifier;
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
        #endregion

        #region Methods
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

        /// <summary>
        /// Executes a host shutdown.
        /// </summary>
        protected void ExecuteShutdown()
        {
            try
            {
                _logger.LogInformation("Shutdown is being executed...");
                _remoteShutdownService.ShutdownHost("Screentime is exceeded.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Shutdown failed. {ex.Message}");
                _stateMachine.Fire(Events.ShutdownError);
            }
        }

        /// <summary>
        /// Send notification when screentime is used up.
        /// </summary>
        protected void NotifyTimeUp()
        {
            _logger.LogInformation("Screentime for host is used up.");
            _webhookNotifier.NotifyTimeUpAsync(CurrentHostStatus);
        }

        /// <summary>
        /// Send notification when the host was turned on again after a shutdown due to exceeded screentime.
        /// </summary>
        protected void NotifyTurnedOnWithoutTimeLeft()
        {
            _logger.LogInformation("Host turned on without time left.");
            _webhookNotifier.NotifyTurnedOnWithoutTimeLeftAsync(CurrentHostStatus);
        }

        /// <summary>
        /// Send notification when the host was successfully shut down.
        /// </summary>
        protected void NotifyHostHasShutdown()
        {
            _logger.LogInformation("Host has shutdown.");
            _webhookNotifier.NotifyHostHasShutdownAsync(CurrentHostStatus);
        }
        #endregion

        #region IHostedService Implementation
        /// <summary>
        /// Starts the statemachine to bring it into operational state.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to allow graceful cancellation.</param>
        /// <returns><see cref="Task"/> reference.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _stateMachine.Start();
            _logger.LogInformation("Statemachine started.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the statemachine.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to allow graceful cancellation.</param>
        /// <returns><see cref="Task"/> reference.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stateMachine.Stop();
            _logger.LogInformation("Statemachine stopped.");

            return Task.CompletedTask;
        } 
        #endregion
    }
}
