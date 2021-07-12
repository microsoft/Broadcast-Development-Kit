// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Net;

namespace Application.Common.Config
{
    public class BotConfiguration
    {
        public string VirtualMachineName { get; set; } = "localhost";

        public string MainApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the DNS name for this service.
        /// </summary>
        public string ServiceDnsName { get; set; }

        public string ServiceCname { get; set; }

        /// <summary>
        /// Gets the base callback URL for this instance.  To ensure that all requests
        /// for a given call go to the same instance, this Url is unique to each
        /// instance by way of its instance input endpoint port.
        /// </summary>
        public Uri CallControlBaseUrl => new Uri(string.Format(
                "https://{0}/{1}/{2}",
                ServiceCname,
                HttpRouteConstants.CallSignalingRoutePrefix,
                HttpRouteConstants.OnIncomingRequestRoute));

        /// <summary>
        /// Gets or sets the remote endpoint that any outgoing call targets.
        /// </summary>
        public Uri PlaceCallEndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets the AadAppId generated at the time of registration of the bot.
        /// </summary>
        public string AadAppId { get; set; }

        /// <summary>
        /// Gets or sets the AadAppSecret generated at the time of registration of the bot.
        /// </summary>
        public string AadAppSecret { get; set; }

        public string ServiceFqdn { get; set; }

        public IPAddress InstancePublicIPAddress { get; set; } = IPAddress.Any;

        public int InstancePublicPort { get; set; }

        public int InstanceInternalPort { get; set; }

        public int NumberOfMultiviewSockets { get; set; }

        public string CertificateThumbprint { get; set; }

        public string CertificatePassword { get; set; }
    }
}