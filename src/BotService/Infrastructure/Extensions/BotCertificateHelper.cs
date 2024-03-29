﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Security.Cryptography.X509Certificates;
using Application.Exceptions;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using BotService.Configuration;
using Serilog;

namespace BotService.Infrastructure.Extensions
{
    public static class BotCertificateHelper
    {
        public static X509Certificate2 GetCertificate(AppConfiguration appConfiguration)
        {
            try
            {
                Log.Information("Getting certificate from store.");
                return GetCertificateFromStore(appConfiguration.BotConfiguration.CertificateThumbprint);
            }
            catch (CertificateNotFoundException)
            {
                Log.Warning("No certificate was found in the machine. The service will attempt to download the certificate and install it in the machine.");
            }

            Log.Information("Getting certificate from key vault.");
            var keyVaultCertificate = GetCertificateFromKeyVault(appConfiguration.KeyVaultName, appConfiguration.BotConfiguration.CertificateName);

            Log.Information("Installing certificate.");
            return InstallCertificate(keyVaultCertificate, appConfiguration.BotConfiguration.CertificatePassword);
        }

        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: true);
                if (certs.Count != 1)
                {
                    throw new CertificateNotFoundException($"No certificate with thumbprint {thumbprint} was found in the machine store.");
                }

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }

        private static KeyVaultCertificateWithPolicy GetCertificateFromKeyVault(string keyVaultName, string certificateName)
        {
            try
            {
                var client = new CertificateClient(vaultUri: new Uri($"https://{keyVaultName}.vault.azure.net/"), credential: new DefaultAzureCredential());
                var certificate = client.GetCertificate(certificateName);

                return certificate.Value;
            }
            catch (Exception)
            {
                Log.Error($"An error has ocurred while trying to get the certificate {certificateName} from keyvault {keyVaultName}.");
                throw;
            }
        }

        private static X509Certificate2 InstallCertificate(KeyVaultCertificateWithPolicy keyVaultCertificate, string password)
        {
            var certBytes = keyVaultCertificate.Cer;
            var cert = new X509Certificate2(certBytes, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();

            return cert;
        }
    }
}
