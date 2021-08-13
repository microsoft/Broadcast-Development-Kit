// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Gst;

namespace BotService.Infrastructure.Pipelines
{
    public class GstreamerCustomClock : SystemClock
    {
        private readonly ulong _baseTimestamp;

        public GstreamerCustomClock()
        {
            // Initial clock time, the system clock does not start from zero
            var initialTime = Time;

            // Calculate the current time sourced from 1900-01-01 00:00:00.0
            var currentTime = (ulong)((System.DateTime.UtcNow - new System.DateTime(1900, 1, 1)).Ticks * 100);
            _baseTimestamp = currentTime - initialTime;
        }

        protected override ulong OnGetInternalTime()
        {
            // It returns current time sourced from 1900-01-01 00:00:00.0
            return _baseTimestamp + base.OnGetInternalTime();
        }
    }
}
