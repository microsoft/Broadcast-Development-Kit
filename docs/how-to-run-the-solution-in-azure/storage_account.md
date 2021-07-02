# Storage Account

## Getting Started

This document shows how to create and configure the Storage Account for the solution core components. This Storage Account will be used to store the environment settings in `json` format, the wildcard SSL certificate in `pfx` format, and the queues that are consumed by the Azure Functions. 

To create a Storage Account in Azure, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

Create this storage account with the following settings:
- ***Resource Group:*** Select the resource group created for the solution architecture.
- ***Storage Account Name***: A meaningful name.
- ***Region***: Same region as the rest of the resources.
- ***Performance***: Standard.
- ***Redundancy***: Locally-redundant storage (LRS).

Leave the rest of the settings as-is.

Once this Storage Account is created, create a new container with the following settings: 
- ***Name***: config.
- ***Public access level***: private.

To create a new Container, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal#create-a-container)

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
                  "Host": "{{virtualMachineDnsCname}}",
                  "Port": 80,
                  "Scheme": "http"
              },
              "Https":{
                  "Host": "{{virtualMachineDnsCname}}",
                  "Port": 443,
                  "Scheme": "https"
              }
          }
    },
  "Settings": {
    "GraphClientConfiguration": {
      "TenantId": "{{tenantIdAzureBotAppRegistration}}",
      "ClientId": "{{clientIdAzureBotAppRegistration}}",
      "ClientSecret": "{{clientSecretAzureBotAppRegistration}}"
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
      "AadAppId": "{{clientIdAzureBotAppRegistration}}",
      "AadAppSecret": "{{clientSecretAzureBotAppRegistration}}",
      "NumberOfMultiviewSockets": 3,
      "InstanceInternalPort": 8445,
      "InstancePublicPort": 8445,
      "ServiceFqdn": "{{virtualMachineDnsCname}}",
      "CertificatePassword": "{{pfxCertificatePassword}}",
      "CertificateThumbprint":"{{pfxCertificateThumbprint}}",
      "MainApiUrl":"{{managementApiURl}}",
      "VirtualMachineName": "{{virtualMachineName}}"
    },
    "AzureAdConfiguration": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "{{tenantIdAzureBotServiceApiClientId}}"
    },
    "BotServiceAuthenticationConfiguration": {
      "BotServiceApiClientId": "{{botServiceClientId}}",
    }
  },
  "APPINSIGHTS_INSTRUMENTATIONKEY": "{{appInsightsKey}}"
}
```
> **NOTE:** The Bot Service appsettings will be completed in later steps.

### Placeholder specification table

| Placeholder                            | Description                                                                                                                                                                                                                                                               |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| virtualMachineDnsCname                 | Full domain name assigned to the virtual machine where the bot service is hosted. E.g.: If your wildcard certificate is for *.domain.co and you added the cname botservicevm to the IP address of the virtual machine, the domain name will be botservicevm.domain.co. |
| tenantIdAzureBotAppRegistration        | Tenant Id of Azure Bot.                                                                                                                                                                                                                                   |
| clientIdAzureBotAppRegistration        | Client Id of the Azure Bot app registration.                                                                                                                                                                                                                           |
| clientIdAzureBotAppRegistration        | Client secret of the Azure Bot app registration.                                                                                                                                                                                                                       |
| cosmosDbEndpointUrl                    | Endpoint URL of the cosmos db created.                                                                                                                                                                                                        |
| cosmosDbPrimareyKey                    | Primary key of the cosmos db created.                                                                                                                                                                                                         |
| cosmosDbDatabaseName                   | Database name of the cosmos db created.                                                                                                                                                                                                       |
| pfxCertificatePassword                 | Password of the wildcard certificate uploaded to the storage account.                                                                                                                                                                                                     |
| pfxCertificateThumbprint               | Thumbprint of the wildcard certificate uploaded to the storage account.                                                                                                                                                                                                   |
| managementApiURl                       | Url of the management API (without https:// prefix).                                                                                                                                                                                                                      |
| tenantIdAzureBotServiceApiClientId     | Client Id of the app registration for the BotService API Client.                                                                                                                                         |
| botServiceClientId                     | Client Id of the app registration for BotService API Client.                                                                                                                                         |
| appInsigtsKey                          | Application Insights key of the application insights resource.                                                                                                                                                                        |

## Upload file to container
Here's explains how to upload the `json file` into the container that was created.

1. From the container created, click on **Upload**.

    ![Uplaoad container](images/container_upload.png)

1. You must select the `json file` that we created in previous step, and then to finish the upload, click on **Upload**.

    ![Select file](images/container_select_file.png)

## Generate SAS Token
 In order to get access to the container created from the Bot Service API we need to generate a Shared access signature, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/document-translation/create-sas-tokens?tabs=Containers)

Create this SAS tokens with the following settings:
- ***Signing method*** Account key.
- ***Signing key***: Key 1.
- ***Permissions***: Read.
- ***Start and expiry data/time***: Select the Time zone for the Start and Expiry date and time (default is Local).
- ***Redundancy***: Locally-redundant storage (LRS).
- ***Allowed protocols:*** HTTPS only.



[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)
