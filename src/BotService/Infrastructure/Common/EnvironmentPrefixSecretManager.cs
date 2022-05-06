// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace BotService.Infrastructure.Common
{
    public class EnvironmentPrefixSecretManager : KeyVaultSecretManager
    {
        private readonly string _prefix;

        public EnvironmentPrefixSecretManager(string prefix)
            => _prefix = $"{prefix}-";

        public override bool Load(SecretProperties properties)
            => properties.Name.StartsWith(_prefix);

        public override string GetKey(KeyVaultSecret secret)
        {
            var secretName = secret.Name.Remove(0, _prefix.Length).Replace("--", ConfigurationPath.KeyDelimiter);
            return secretName;
        }
    }
}
