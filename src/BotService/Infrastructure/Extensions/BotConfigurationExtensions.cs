using Application.Common.Config;
using Microsoft.Skype.Bots.Media;
using System.Net;

namespace BotService.Infrastructure.Extensions
{
    public static class BotWorkerConfigurationExtensions
    {
        public static MediaPlatformSettings GetMediaPlatformSettings(this BotConfiguration configuration)
        {
            int mediaInstanceInternalPort = configuration.InstanceInternalPort;
            int mediaInstancePublicPort = configuration.InstancePublicPort;

            IPAddress publicInstanceIpAddress = configuration.InstancePublicIPAddress;
            string serviceFqdn = configuration.ServiceFqdn;

            var mediaPlatformSettings = new MediaPlatformSettings()
            {
                MediaPlatformInstanceSettings = new MediaPlatformInstanceSettings()
                {
                    CertificateThumbprint = configuration.CertificateThumbprint,
                    InstanceInternalPort = mediaInstanceInternalPort,
                    InstancePublicIPAddress = publicInstanceIpAddress,
                    InstancePublicPort = mediaInstancePublicPort,
                    ServiceFqdn = serviceFqdn,
                },

                ApplicationId = configuration.AadAppId,
            };

            return mediaPlatformSettings;
        }
    }
}
