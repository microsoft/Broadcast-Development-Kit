// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace BotService.Infrastructure.Common.Logging
{
    public class PipelineBusObserver : IObserver<BusEventPayload>, IDisposable
    {
        private static readonly int MaxLogCount = 5000;

        private readonly ILogger _logger;

        /// <summary>
        /// Lock for securing logs.
        /// </summary>
        private readonly object _lockLogs = new object();

        /// <summary>
        /// Linked list representing the logs.
        /// </summary>
        private LinkedList<string> _logs = new LinkedList<string>();

        public PipelineBusObserver(ILoggerFactory loggerFactory)
        {
            // TODO: add the dependencies needed in the constructor
            _logger = loggerFactory.CreateLogger<PipelineBusObserver>();
        }

        public void OnCompleted()
        {
            // Part of the IObserver interface. We don't need to do anything here.
        }

        public void OnError(Exception error)
        {
            // Part of the IObserver interface. We don't need to do anything here.
        }

        public void OnNext(BusEventPayload value)
        {
            // TODO: Handle the methods to log the event
            AddLog(value);
            AddLogToApplicationInsight(value);
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
            }
        }

        private static string GetFormattedLog(BusEventPayload busEvent)
        {
            // TODO: validate formatting the log for easy identification
            var formattedLog = new StringBuilder()
                .Append($"[{busEvent.DateTime:s}] [Injection Pipeline]").AppendLine()
                .Append($"\tCall Id: {busEvent.CallId}").AppendLine()
                .Append($"\tStream Id: {busEvent.StreamId}").AppendLine()
                .Append($"\tType of message: {busEvent.MessageType}").AppendLine()
                .Append($"\tMessage: {busEvent.Message}")
                .ToString();

            return formattedLog;
        }

        private void AddLogToApplicationInsight(BusEventPayload busEvent)
        {
            // TODO: Validate the format of the logging
            var formattedLog = "[Injection Pipeline] Call Id: {callId}, Stream Id: {streamId}, Type of message: {busMessageType}, {message}";

            if (busEvent.MessageType == BusMessageType.StateChanged
                || busEvent.MessageType == BusMessageType.Eos)
            {
                _logger.LogInformation(formattedLog, busEvent.CallId, busEvent.StreamId, busEvent.MessageType, busEvent.Message);
            }
            else if (busEvent.MessageType == BusMessageType.Qos
                     || busEvent.MessageType == BusMessageType.Unknown)
            {
                _logger.LogWarning(formattedLog, busEvent.CallId, busEvent.StreamId, busEvent.MessageType, busEvent.Message);
            }
            else
            {
                _logger.LogError(formattedLog, busEvent.CallId, busEvent.StreamId, busEvent.MessageType, busEvent.Message);
            }
        }

        private void AddLog(BusEventPayload busEvent)
        {
            // Only log the error messges
            if (busEvent.MessageType == BusMessageType.Error
                || busEvent.MessageType == BusMessageType.Qos
                || busEvent.MessageType == BusMessageType.Eos)
            {
                var formattedLog = GetFormattedLog(busEvent);

                lock (_lockLogs)
                {
                    _logs.AddFirst(formattedLog);
                    if (_logs.Count > MaxLogCount)
                    {
                        _logs.RemoveLast();
                    }
                }
            }
        }
    }

    // TODO: remove from here this entity, create in the corresponding path
    public class BusEventPayload
    {
        public string CallId { get; set; }

        public string StreamId { get; set; }

        public BusMessageType MessageType { get; set; }

        public string Message { get; set; }

        public DateTime DateTime { get; set; }
    }
}
