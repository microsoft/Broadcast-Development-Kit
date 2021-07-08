// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
namespace Application.Common.Config
{
    public static class HttpRouteConstants
    {
        /// <summary>
        /// Route prefix for all incoming requests.
        /// </summary>
        public const string CallSignalingRoutePrefix = "api/bot";

        /// <summary>
        /// Route for incoming call requests.
        /// </summary>
        public const string OnIncomingRequestRoute = "calling";

        /// <summary>
        /// Route for incoming notification requests.
        /// </summary>
        public const string OnNotificationRequestRoute = "notification";

        public const string JoinCall = "JoinCall";
        public const string Logs = "logs/";
    }
}