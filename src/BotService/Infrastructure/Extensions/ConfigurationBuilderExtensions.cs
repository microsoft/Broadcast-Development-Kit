// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BotService.Infrastructure.Common;
using Microsoft.Extensions.Configuration;

namespace BotService.Infrastructure.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        private const string KeyVaultNameKey = "Settings:KeyVaultName";
        private const string KeyVaultEnvKey = "Settings:KeyVaultEnv";

        public static IConfigurationRoot GetConfiguration(string[] args)
        {
            var builder = new ConfigurationBuilder()
              .AddCommandLine(args)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .AddKeyVaultConfiguration();

            var configuration = builder.Build();

            return configuration;
        }

        public static IConfigurationBuilder AddKeyVaultConfiguration(this IConfigurationBuilder builder)
        {
            var appSettings = builder.Build();
            var keyVaultName = appSettings[KeyVaultNameKey];

            if (string.IsNullOrEmpty(keyVaultName))
            {
                Console.WriteLine("Keyvault name was not specified.");
                return builder;
            }

            var envPrefix = appSettings[KeyVaultEnvKey];
            var secretManager = string.IsNullOrEmpty(envPrefix) ? new KeyVaultSecretManager() : new EnvironmentPrefixSecretManager(envPrefix);

            try
            {
                var secretClient = new SecretClient(
                    new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());
                builder.AddAzureKeyVault(secretClient, secretManager);
                Console.WriteLine($"Added Azure Key Vault: {keyVaultName}");

                return builder;
            }
            catch (Exception ex)
            {
                throw new System.Configuration.ConfigurationErrorsException($"Error adding Azure Key Vault configuration: {ex.Message}", ex);
            }
        }
    }
}
