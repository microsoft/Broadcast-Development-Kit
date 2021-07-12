// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Application.Interfaces.Common;

namespace Infrastructure.Core.Common
{
    public class HostEnvironment : IHostEnvironment
    {
        private const string Development = "development";
        private const string Production = "production";
        private const string Local = "local";
        private const string BroadcasterEnvironmentVariableKey = "BROADCASTER_ENVIRONMENT";
        private const string AspNetCoreEnvironmentVariableKey = "ASPNETCORE_ENVIRONMENT";
        private const string AzureFunctionEnvironmentVariableKey = "AZURE_FUNCTIONS_ENVIRONMENT";

        public HostEnvironment(bool isAzureFunction = false)
        {
            var broadcasterEnvironment = Environment.GetEnvironmentVariable(BroadcasterEnvironmentVariableKey);

            if (string.IsNullOrEmpty(broadcasterEnvironment))
            {
                var environment = isAzureFunction ?
                    Environment.GetEnvironmentVariable(AzureFunctionEnvironmentVariableKey) :
                    Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariableKey);

                if (!string.IsNullOrEmpty(environment))
                {
                    EnvironmentName = environment;
                }
            }
            else
            {
                EnvironmentName = broadcasterEnvironment;
            }
        }

        public string EnvironmentName { get; private set; } = Development;

        public bool IsDevelopment()
        {
            var result = EnvironmentName.ToLowerInvariant() == Development;
            return result;
        }

        public bool IsProduction()
        {
            var result = EnvironmentName.ToLowerInvariant() == Production;
            return result;
        }

        public bool IsLocal()
        {
            var result = EnvironmentName.ToLowerInvariant() == Local;
            return result;
        }
    }
}
