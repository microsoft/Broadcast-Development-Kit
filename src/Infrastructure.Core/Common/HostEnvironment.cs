using Application.Interfaces.Common;
using System;

namespace Infrastructure.Core.Common
{
    public class HostEnvironment : IHostEnvironment
    {
        private const string _development = "development";
        private const string _production = "production";
        private const string _local = "local";
        private const string _broadcasterEnvironmentVariableKey = "BROADCASTER_ENVIRONMENT";
        private const string _aspNetCoreEnvironmentVariableKey = "ASPNETCORE_ENVIRONMENT";
        private const string _azureFunctionEnvironmentVariableKey = "AZURE_FUNCTIONS_ENVIRONMENT";

        public string EnvironmentName { get; private set; } = _development;

        public HostEnvironment(bool isAzureFunction = false)
        {
           var broadcasterEnvironment = Environment.GetEnvironmentVariable(_broadcasterEnvironmentVariableKey);

            if (string.IsNullOrEmpty(broadcasterEnvironment))
            {
                var environment = isAzureFunction? 
                    Environment.GetEnvironmentVariable(_azureFunctionEnvironmentVariableKey): 
                    Environment.GetEnvironmentVariable(_aspNetCoreEnvironmentVariableKey);

                if (!string.IsNullOrEmpty(environment)){
                    EnvironmentName = environment;
                }
            }
            else
            {
                EnvironmentName = broadcasterEnvironment;
            }
        }

        public bool IsDevelopment()
        {
            var result = EnvironmentName.ToLowerInvariant() == _development;
            return result;
        }

        public bool IsProduction()
        {
            var result = EnvironmentName.ToLowerInvariant() == _production;
            return result;
        }
        public bool IsLocal()
        {
            var result = EnvironmentName.ToLowerInvariant() == _local;
            return result;
        }
    }
}
