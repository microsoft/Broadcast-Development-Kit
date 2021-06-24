using System.Threading.Tasks;

namespace Application.Interfaces.Common
{
    public interface ICloudConfigurationService
    {
        System.IO.Stream GetCertificate();
        Task<System.IO.Stream> GetCertificateAsync();
        System.IO.Stream GetAppSettingsAsStream();
        Task<System.IO.Stream> GetAppSettingsAsStreamAsync();
    }
}
