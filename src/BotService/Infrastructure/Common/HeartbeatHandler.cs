using System;
using System.Threading.Tasks;
using System.Timers;
using BotService.Infrastructure.Extensions;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Communications.Common.Telemetry;

namespace BotService.Infrastructure.Common
{
    public abstract class HeartbeatHandler : ObjectRootDisposable
    {
        private readonly Timer _heartbeatTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartbeatHandler"/> class.
        /// </summary>
        /// <param name="frequency">The frequency of the heartbeat.</param>
        /// <param name="logger">The graph logger.</param>
        protected HeartbeatHandler(TimeSpan frequency, IGraphLogger logger)
            : base(logger)
        {
            // initialize the timer
            var timer = new Timer(frequency.TotalMilliseconds)
            {
                Enabled = true,
                AutoReset = true,
            };

            timer.Elapsed += HeartbeatDetected;
            _heartbeatTimer = timer;
        }

        /// <summary>
        /// This function is called whenever the heartbeat frequency has ellapsed.
        /// </summary>
        /// <param name="args">The elapsed event args.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected abstract Task HeartbeatAsync(ElapsedEventArgs args);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _heartbeatTimer.Elapsed -= HeartbeatDetected;
            _heartbeatTimer.Stop();
            _heartbeatTimer.Dispose();
        }

        /// <summary>
        /// The heartbeat function.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The elapsed event args.</param>
        private void HeartbeatDetected(object sender, ElapsedEventArgs args)
        {
            var task = $"{GetType().FullName}.{nameof(HeartbeatAsync)}(args)";
            GraphLogger.Verbose($"Starting running task: " + task);
            _ = Task.Run(() => HeartbeatAsync(args)).ForgetAndLogExceptionAsync(GraphLogger, task);
        }
    }
}