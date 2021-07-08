// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace BotService.Infrastructure.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph.Communications.Common.Telemetry;

    /// <summary>
    /// Memory logger for quick diagnostics.
    /// Note: Do not use in production code.
    /// </summary>
    public class SampleObserver : IObserver<LogEvent>, IDisposable
    {
        private static readonly int MaxLogCount = 5000;

        /// <summary>
        /// Lock for securing logs.
        /// </summary>
        private readonly object _lockLogs = new object();

        /// <summary>
        /// Observer subscription.
        /// </summary>
        private IDisposable _subscription;

        /// <summary>
        /// Linked list representing the logs.
        /// </summary>
        private LinkedList<string> _logs = new LinkedList<string>();

        /// <summary>
        /// The formatter.
        /// </summary>
        private ILogEventFormatter _formatter = new CommsLogEventFormatter();

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleObserver" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public SampleObserver(IGraphLogger logger)
        {
            // Log unhandled exceptions.
            AppDomain.CurrentDomain.UnhandledException += (_, e) => logger.Error(e.ExceptionObject as Exception, $"Unhandled exception");
            TaskScheduler.UnobservedTaskException += (_, e) => logger.Error(e.Exception, "Unobserved task exception");

            _subscription = logger.Subscribe(this);
        }

        /// <summary>
        /// Get the complete or portion of the logs.
        /// </summary>
        /// <param name="skip">Skip number of entries.</param>
        /// <param name="take">Pagination size.</param>
        /// <returns>Log entries.</returns>
        public string GetLogs(int skip = 0, int take = int.MaxValue)
        {
            lock (_lockLogs)
            {
                skip = skip < 0 ? Math.Max(0, _logs.Count + skip) : skip;
                var filteredLogs = _logs
                    .Skip(skip)
                    .Take(take);
                return string.Join(Environment.NewLine, filteredLogs);
            }
        }

        /// <summary>
        /// Get the complete or portion of the logs.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="skip">Skip number of entries.</param>
        /// <param name="take">Pagination size.</param>
        /// <returns>
        /// Log entries.
        /// </returns>
        public string GetLogs(string filter, int skip = 0, int take = int.MaxValue)
        {
            lock (_lockLogs)
            {
                skip = skip < 0 ? Math.Max(0, _logs.Count + skip) : skip;
                var filteredLogs = _logs
                    .Where(log => log.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Skip(skip)
                    .Take(take);
                return string.Join(Environment.NewLine, filteredLogs);
            }
        }

        /// <inheritdoc />
        public void OnNext(LogEvent logEvent)
        {
            // Do nothing for metrics for now.
            if (logEvent.EventType == LogEventType.Metric)
            {
                return;
            }

            // Log only http traces if enabled.
            if (logEvent.EventType != LogEventType.HttpTrace && logEvent.Level != TraceLevel.Error && logEvent.Level != TraceLevel.Warning)
            {
                return;
            }

            var logString = _formatter.Format(logEvent);
            lock (_lockLogs)
            {
                _logs.AddFirst(logString);
                if (_logs.Count > MaxLogCount)
                {
                    _logs.RemoveLast();
                }
            }
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
            // Part of the IObserver interface. We don't need to do anything here.
        }

        /// <inheritdoc />
        public void OnCompleted()
        {
            // Part of the IObserver interface. We don't need to do anything here.
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lockLogs)
                {
                    _logs?.Clear();
                    _logs = null;
                }

                _subscription?.Dispose();
                _subscription = null;
                _formatter = null;
            }
        }
    }
}
