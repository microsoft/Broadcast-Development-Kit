function Update-AppSettings {
    param (
    $publishOutputSrc,
    $dnsRecordName,
    $dnsZoneName,
    $azureBotTenantId,
    $azureBotClientId,
    $managementUri,
    $vmName,
    $botServiceAppRegistrationTenantId,
    $botServiceApiClientId,
    $keyvaultName,
    $appinsightsinstrumentatioKey,
    $cosmosEndpoint,
    $cosmosDatabase
)

$pathToAppSettingsFile = $publishOutputSrc + "\\appSettings.json"


$certificateName = "Certificate"
$botDns = "${dnsRecordName}.${dnsZoneName}" 

$appSettingsObject = Get-Content "appSettings.json" | Out-String | ConvertFrom-Json

$appSettingsObject.HttpServer.Endpoints.Https.Host = $botDNS

$appSettingsObject.Settings.GraphClientConfiguration.TenantId = $azureBotTenantId
$appSettingsObject.Settings.GraphClientConfiguration.ClientId = $azureBotClientId

$appSettingsObject.Settings.CosmosDbConfiguration.EndpointUrl = $cosmosEndpoint
$appSettingsObject.Settings.CosmosDbConfiguration.DatabaseName = $cosmosDatabase

$appSettingsObject.Settings.BotConfiguration.ServiceDnsName = $botDNS
$appSettingsObject.Settings.BotConfiguration.ServiceCname = $botDNS
$appSettingsObject.Settings.BotConfiguration.AadAppId = $azureBotClientId
$appSettingsObject.Settings.BotConfiguration.ServiceFqdn = $botDNS
$appSettingsObject.Settings.BotConfiguration.CertificateName = $certificateName
$appSettingsObject.Settings.BotConfiguration.MainApiUrl = $managementUri
$appSettingsObject.Settings.BotConfiguration.VirtualMachineName = $vmName

$appSettingsObject.Settings.AzureAdConfiguration.TenantId = $botServiceAppRegistrationTenantId

$appSettingsObject.Settings.BotServiceAuthenticationConfiguration.BotServiceApiClientId = $botServiceApiClientId

$appSettingsObject.Settings.KeyVaultName = $keyvaultName
$appSettingsObject.Settings.KeyVaultEnv = ""

$appSettingsObject.APPINSIGHTS_INSTRUMENTATIONKEY = $appinsightsinstrumentatioKey

$appSettingsObject | ConvertTo-Json -Depth 100 | Out-File $pathToAppSettingsFile
}
