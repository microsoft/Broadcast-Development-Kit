# Storage Account

## Getting Started

This document shows how to create and configure the Atorage Account for the solution core components. This Storage Account will be used to store the environment settings in **JSON** format, the wildcard SSL certificate in **PFX** format, and the queues that are consumed by the Azure Functions. 

To create a Storage Account in Azure, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

Create this storage account with the following settings:
- ***Name***: a meaningfull name.
- ***Region***: same region as the rest of the resources.
- ***Performance***: Standard.
- ***Redundancy***: Locally-redundant storage (LRS).

Leave the rest of the settings as-is. Once this Storage Account is created, create a new container in this storage account with the following settings: 
- ***Name***: config.
- ***Public access level***: private.

Once the config container is created, upload the Bot Service settings and the wildcard SSL certificate files to it.

### Environment ***.json*** file settings example:
Below there is a json file template with placeholders values you need to complete and upload to storage account before using the bot for the first time.

```json
{
 "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
	"HttpServer":{
          "Endpoints":{
              "Http":{
                  "Host": "localhost",
                  "Port": 80,
                  "Scheme": "http"
              },
              "Https":{
                  "Host": "localhost",
                  "Port": 443,
                  "Scheme": "https"
              }
          }
    },
  "Settings": {
    "GraphClientConfiguration": {
      "TenantId": "{{tenantIdBotChannelsAppRegistration}}",
      "ClientId": "{{clientIdBotChannelsAppRegistration}}",
      "ClientSecret": "{{clientSecretBotChannelsAppRegistration}}"
    },
    "CosmosDbConfiguration": {
      "EndpointUrl": "{{cosmosDbEndpointUrl}}",
      "PrimaryKey": "{{cosmosDbPrimareyKey}}",
      "DatabaseName": "{{cosmosDbDatabaseName}}",
    },
    "BotConfiguration": {
      "ServiceDnsName": "{{virtualMachineDnsCname}}",
      "ServiceCname": "{{virtualMachineDnsCname}}",
      "PlaceCallEndpointUrl": "https://graph.microsoft.com/beta",
      "AadAppId": "{{clientIdBotChannelsAppRegistration}}",
      "AadAppSecret": "{{clientSecretBotChannelsAppRegistration}}",
      "NumberOfMultiviewSockets": 3,
      "InstanceInternalPort": 8445,
      "InstancePublicPort": 8445,
      "ServiceFqdn": "{{virtualMachineDnsCname}}",
      "CertificatePassword": "{{pfxCertificatePassword}}",
      "CertificateThumbprint":"{{pfxCertificateThumbprint}}",
      "SRTPort": 8888,
      "MainApiUrl":"{{managementApiURl}}",
      "VirtualMachineDnsCname": "{{virtualMachineUrl}}"
    }
  },
  "APPINSIGHTS_INSTRUMENTATIONKEY": "{{appInsigtsKey}}"
}
```
### Placeholder specification table

| Placeholder                            | Description                                                                                                                                                                                                                                                               |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| TenantIdBotChannelsAppRegistration     | Tenant Id of bot channels registration.                                                                                                                                                                                                                                   |
| ClientIdBotChannelsAppRegistration     | Client Id of the bot channels app registration.                                                                                                                                                                                                                           |
| ClientSecretBotChannelsAppRegistration | Client secret of the bot channels app registration.                                                                                                                                                                                                                       |
| CosmosDbEndpointUrl                    | Endpoint URL of the cosmos db created.                                                                                                                                                                                                        |
| CosmosDbPrimareyKey                    | Primary key of the cosmos db created.                                                                                                                                                                                                         |
| CosmosDbDatabaseName                   | Database name of the cosmos db created.                                                                                                                                                                                                       |
| VirtualMachineDnsCname                 | Full domain name assigned to the virtual machine where the bot service is hosted. E.g.: If your wildcard certificate is for *.teamstx.co and you added the cname botservicevm to the IP address of the virtual machine, the domain name will be botservicevm.teamstx.co. |
| PfxCertificatePassword                 | Password of the wildcard certificate uploaded to the storage account.                                                                                                                                                                                                     |
| PfxCertificateThumbprint               | Thumbprint of the wildcard certificate uploaded to the storage account.                                                                                                                                                                                                   |
| ManagementApiURl                       | Url of the management API (without https:// prefix).                                                                                                                                                                                                                      |
| AppInsigtsKey                          | Application Insights key of the application insights resource.                                                                                                                                                                        |

[← Back to How to Running the solution in Azure](README.md#running-the-solution-in-azure)
