namespace Application.Common.Config
{
    public class CloudConfigSettings
    {
        public string StorageAccountName { get; set; }

        public string BlobContainerName { get; set; }

        public string AppSettingsFileName { get; set; }

        public string CertificateFileName { get; set; }

        public string SasToken { get; set; }
    }
}
