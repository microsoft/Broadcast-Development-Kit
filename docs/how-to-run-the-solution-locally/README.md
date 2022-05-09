# How to run the solution locally

Before configuring the solution to run it locally you must have the following prerequisites:

- A **domain name** - This domain name will be used to host the solution's core components.
- An **SSL wildcard certificate** - A valid wildcard certificate for the domain mentioned in the previous point. This is required to establish a TLS/TCP control channel between the bot's media platform and the calling clouds. The certificate must be in .pem and .pfx formats.
- An **Office 365 tenant with Microsoft Teams** enabled - If the organization is already using Microsoft Teams for their meetings, then you already have an Office 365 tenant configured.
  - Note that you need the create an app registration in this tenant with permissions to join meetings and send and receive audio and video in those meetings. These permissions will need to be approved by your Office 365 tenant administrator.
  - If your organization doesn't want to provide the necessary permissions for the solution to connect to the meetings, or you just want to test this solution in an isolated tenant, you can obtain a new testing tenant using Office 365's [Developer Program](https://developer.microsoft.com/en-us/microsoft-365/dev-program).

## Azure Bot

To run the solution locally and add calling capabilities to the bot, you must create an Azure Bot resource. To do so, before creating the Azure Bot, you must create a resource group for it ([How to create a resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups)) and then create the following resources:

- [How to create/configure Azure Bot app registration](../common/azure-bot-app-registration.md#azure-bot-app-registration)
- [How to create/configure Azure Bot](../common/azure-bot.md#azure-bot)

## Tools

### Microsoft Azure Storage Emulator

Download and Install [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator#get-the-storage-emulator)

### Visual Studio 2022

Download and Install [Visual Studio Community 2022](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community)

- [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) (should be already included with VS)
- [Microsoft Visual C++ 2015-2019 Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) (x64) (should be already included with VS)

### Cosmos DB Emulator

Download [Azure Cosmos DB emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) and install.

Open the Azure Cosmos DB emulator, it will open a new browser tab and will show the `Quickstart` view.

|![Cosmos DB Data Explorer](images/cosmos_db_data_explorer.png)|
|:--:|
|*Cosmos DB Emulator Data Explorer*|

> Copy the value of the primary key, you will use it later.

### GStreamer

To support the processing of the Teams media (the content injection and content extraction), it is necessary to install and configure [GStreamer](gstreamer.md).

### NGINX

To support RTMP pull mode extractions and RTMP injection features, it is necessary to install and configure [NGINX](../common/install-and-configure-nginx-with-rtmp-module-on-windows.md).

### Ngrok

To run the solution locally and expose localhost so Microsoft Graph API can send notifications to the bot, it is necessary to install and configure [ngrok](ngrok.md) so we can use it to create the tunnels to localhost.

### Domain Certificate

You will need to install the pfx certificate mentioned in the prerequisites in your local environment. You can find instructions on how to do this in document [Manual installation of your domain certificate](../common/install-domain-certificate.md).

## Configure the Backend Solution to run locally

The solution is composed by 3 main projects:

- **BotOrchestrator:** The Azure Functions used to execute some of the features in the solution.
- **ManagementApi:** The project containing the management API that is used to interact with the solution.
- **BotService:** A self-hosted API/application-hosted media bot that hosts the media session. As part of its core components, it includes  the GStreamer pipelines that are in charge of processing the media and deliver it in SRT/RTMP.

Each of the three projects have configuration files that must be updated separately in order to run the solution locally. To do so, run Visual Studio 2022 as administrator, right click on the Visual Studio icon and click the `Run as administrator` option. It is very important to open it as administrator otherwise the solution will not run correctly.

In the Visual Studio menu select `"File" > "Open" > "Project / Solution"` and select the `Broadcaster.sln` file in the src folder of the solution.

### Configure Bot Orchestrator

In the solution explorer, go to the `BotOrchestrator` project (under `src/applications`) and create a new configuration file with the name `local.settings.json`

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "BROADCASTER_ENVIRONMENT": "local",
    "APPINSIGHTS_INSTRUMENTATIONKEY": "{{appInsightInstrumentationKey}}",
    "BuildVersion": "local-Api",
    "CosmosDbConfiguration:EndpointUrl": "https://localhost:8081",
    "CosmosDbConfiguration:DatabaseName": "{{cosmosDbDatabaseName}}",
    "CosmosDbConfiguration:PrimaryKey": "{{cosmosDbPrimaryKey}}",
    "AzServicePrincipalConfiguration:ApplicationClientId": "",
    "AzServicePrincipalConfiguration:ApplicationClientSecret": "",
    "AzServicePrincipalConfiguration:SubscriptionId": "",
    "AzServicePrincipalConfiguration:TenantId": "",
    "AzureAdConfiguration:Instance": "https://login.microsoftonline.com/",
    "AzureAdConfiguration:TenantId": "",
    "BotServiceAuthenticationConfiguration:BotServiceApiClientId": "",
    "BotServiceAuthenticationConfiguration:ClientId": "",
    "BotServiceAuthenticationConfiguration:ClientSecret": ""
  }
}
```

Placeholder | Description
---------|----------
appInsightInstrumentationKey | ***`Optional:`*** by default leave it empty. If you want to store the logs and have an instance of Application Insights you can enter its instrumentation key.
cosmosDbPrimaryKey | [Azure Cosmos DB Emulator primary key, can be found in the data explorer of the emulator](#cosmos-db-emulator).
cosmosDbDatabaseName | Name of the database that the solution will create in Cosmos DB Emulator. E.g.: `BroadcastDevelopmentKitDb`

### Configure Management Api

In the solution explorer, go to the `ManagementApi` project (under `src/applications`), open the `Properties` folder and replace the `launchSettings.json` with the following configuration.

```json
{
  "profiles": {
    "ManagementApi": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "local",
      },
      "applicationUrl": "https://localhost:8442;http://localhost:8441"
    }
  }
}
```

Then, in the root of the project, create a new configuration file with the name `appsettings.local.json` and copy the following configuration into it.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "APPINSIGHTS_INSTRUMENTATIONKEY": "{{appInsightInstrumentationKey}}",
  "Settings": {
    "BuildVersion": "local-Api",
    "GraphClientConfiguration": {
      "TenantId": "{{tenantIdAzureBotAppRegistration}}",
      "ClientId": "{{clientIdAzureBotAppRegistration}}",
      "ClientSecret": "{{clientSecretAzureBotAppRegistration}}"
    },
    "CosmosDbConfiguration": {
      "EndpointUrl": "https://localhost:8081",
      "PrimaryKey": "{{cosmosDbPrimaryKey}}",
      "DatabaseName": "{{cosmosDbDatabaseName}}"
    },
    "StorageConfiguration": {
      "ConnectionString": "UseDevelopmentStorage=true",
    }
  }
}
```

Placeholder | Description
---------|----------
appInsightInstrumentationKey | ***`Optional:`*** by default leave it empty or if you have an instance of Application Insights you can store the log messages by entering an instrumentation key.
tenantIdAzureBotAppRegistration | Tenant Id of Azure Bot [app registration](#azure-bot).
clientIdAzureBotAppRegistration | Client Id of Azure Bot [app registration](#azure-bot).
clientSecretAzureBotAppRegistration | Client secret of Azure bot [app registration](#azure-bot).
cosmosDbPrimaryKey | [Azure Cosmos DB Emulator primary key, can be found in the data explorer of the emulator](#cosmos-db-emulator).
cosmosDbDatabaseName | Name of the database that the solution will create in Cosmos DB Emulator.  E.g.: `BroadcastDevelopmentKitDb`

### Configure BotService

In the solution explorer, go to the `BotService` project (under `src/applications`), open the `Properties` folder and replace the `launchSettings.json` with the following configuration.

```json
{
  "profiles": {
    "BotService": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "local",
      },
      "applicationUrl": "https://localhost:9442;http://localhost:9441"
    }
  }
}
```

Then, in the root of the project, create a new configuration file with the name `appsettings.{{Configuration}}.json` where ``{{Configuration}}`` has to be replaced with the configuration mode that you want to use, e.g: `Debug` or `Release`. At build time, Visual Studio will get the corresponding file to the selected configuration mode and generate the `appSettings.json`.

Copy the following configuration into it.

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
  "HttpServer": {
    "Endpoints": {
      "Http": {
        "Host": "localhost",
        "Port": 9441,
        "Scheme": "http"
      },
      "Https": {
        "Host": "localhost",
        "Port": 9442,
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
      "EndpointUrl": "https://localhost:8081",
      "PrimaryKey": "{{cosmosDbPrimaryKey}}",
      "DatabaseName": "{{cosmosDbDatabaseName}}"
    },
    "BotConfiguration": {
      "ServiceDnsName": "{{ngrokUrl}}",
      "ServiceCname": "{{ngrokUrl}}",
      "PlaceCallEndpointUrl": "https://graph.microsoft.com/beta",
      "AadAppId": "{{clientIdAzureBotAppRegistration}}",
      "AadAppSecret": "{{clientSecretAzureBotAppRegistration}}",
      "NumberOfMultiviewSockets": 3,
      "InstanceInternalPort": 8445,
      "InstancePublicPort": "{{instancePublicPort}}",
      "ServiceFqdn": "{{serviceFqdn}}",
      "CertificatePassword": "",
      "CertificateThumbprint": "{{certificateThumbprint}}",
      "MainApiUrl": "localhost:8442",
      "SecondsWithoutParticipantsBeforeRemove": {{secondsBeforeRemove}}
    }
  },
  "APPINSIGHTS_INSTRUMENTATIONKEY": "{{appInsightInstrumentationKey}}"
}
```

Placeholder | Description
|-|-|
botServiceHttpsPort | BotService Https port configured in botService launchsettings
botServiceHttpPort | BotService Http port configured in botService launchsettings
tenantIdAzureBotAppRegistration | Tenant Id of the [app registration](#azure-bot).
clientIdAzureBotAppRegistration | Client Id of the [app registration](#azure-bot).
clientSecretAzureBotAppRegistration | Client secret of the [app registration](#azure-bot).
cosmosDbPrimaryKey | [Azure Cosmos DB Emulator primary key, can be found in the data explorer of the emulator](#cosmos-db-emulator).
cosmosDbDatabaseName | Name of the database that the solution will create in Cosmos DB Emulator.  E.g.: `BroadcastDevelopmentKitDb`
certificateThumbprint | Thumbprint of the installed certificate. If you don't know your certificate thumbprint, follow the instructions [here](../common/install-domain-certificate.md#obtaining-the-certificate-thumbprint) to obtain it.
secondsBeforeRemove | Seconds before removing the bot from a meeting with no participants.
appInsightInstrumentationKey | ***`Optional:`*** by default leave it empty or if you have an instance of Application Insights you can store the log messages by entering an instrumentation key.

The settings listed below depends on ngrok static tunnels. Because we suggest to use a free ngrok account and free accounts do not provide static tunnels, tunnels change every time a tunnel is created. So, the following settings must be updated every time that you run ngrok and the solution.

| ![Ngrok Configuration](images/ngrok_configuration.png)|
|:--:|
|*Where to take the values from Ngrok*|

Placeholder | Description
|-|-|
ngrokUrl | Complete with the red marked value (1) from the ngrok console (e.g: `66c0b316671d.ngrok.io`)
instancePublicPort |  Complete with the yellow marked value (2) in the ngrok console (e.g: `16186`)
serviceFqdn | Complete with the green marked value (3) in the ngrok console and the domain name. (e.g: `4.domain.co`) It is necessary to  add a DNS record pointing to ngrok's TCP Url. (e.g: 4.domain.co -> 4.tcp.ngrok.io).

### Configure solution

In the standard Visual Studio toolbar, click on the down arrow next to the start button and select `Set Startup Projects...`.

|![Set Startup Projects](images/visual_set_startup_projects.png)|
|:--:|
|*Select "Set Startup Projects..."*|

A new window will open, select `Multiple startup projects:` option and change the action of the following projects from `None` to `Start`:

- ManagementApi
- BotOrchestrator
- BotService

|![Update Startup Actions](images/startup_projects.png)|
|:--:|
|*Update Actions properties to Start*|

In the solution explorer, right click on the `Botservice` project inside `src\applications` and click on the properties option. A New window will open and in the left menu, select `debug` and verify that the `Launch` configuration is set as `Project`. Verify also the `Launch` settings of the `ManagementApi` project

Make sure that [Cosmos DB](#cosmos-db-emulator) is running and press the `Start` Button in the Visual Studio Standard Toolbar.

|![Start](images/start.png)|
|:--:|
|*Start button on Standard Toolbar*|

This first start-up will create the database with the name specified in the configuration files and the necessary containers. The start of the solution will fail with an error message displayed in the BotService console saying that the service is not found, so you have to proceed and add this record in the database.

|![BotService Err](images/botservice_fails.png)|
|:--:|
|*BotService Error Message*|

Open Cosmos DB Data Explorer and in the left menu select explorer and then select the database created to run the solution.

|![Add new Item to Service](images/cosmos_add_service.png)|
|:--:|
|*Select in the Service Container and then add a new Item*|

Select the `Service` container and then click on the `New Item` button and copy the following settings and click on `Save` button

```json
{
    "CallId": null,
    "Name": "Local Service",
    "State": 1,
    "CreatedAt": "2021-06-09T11:05:37.2778107-03:00",
    "Infrastructure": {
        "VirtualMachineName": "localhost",
        "ResourceGroup": "",
        "SubscriptionId": "",
        "Id": "localhost",
        "PowerState": "PowerState/run",
        "IpAddress": "localhost:9442",
        "Dns": "localhost:9442",
        "ProvisioningDetails": {
            "Message": "",
            "State": {
                "Id": 1,
                "Name": "Provisioned"
            }
        }
    },
    "id": "00000000-0000-0000-0000-000000000000"
}
```

Press the `Start` button again and wait until all projects start. If the Solution does not start successfully, review the initial settings.

## Run the solution

Each time you are going to run the solution you need to perform the following steps:

- ***Run Azure cosmos DB emulator***: you can check that cosmos DB is run by verifying that the cosmos db logo is in the windows taskbar.
- ***Run Ngrok:*** It is necessary to have an instance of ngrok run in order to execute the solution. Once initialized, it is necessary to update the `appsettings.Debug.json` file of the `BotService` with the new values of the ngrok instance.
- ***Run NGINX*** *`Optional`*: If you want to perform locally RTMP extractions in pull mode or RTMP injection into Microsoft Teams Meeting, you need to run NGINX before running the solution.

Finally, run Visual Studio as administrator, open the solution and click the `Start` button on the standard Visual Studio toolbar and wait for all projects to start.

>Both the Management API and the BotService API should open a console windows and also browser windows (with no content in them). Check the port numbers in the browser windows as they should match with the ports configured in previous steps. If not, the incorrect profile might be selected in one of the projects.

## Testing the application

To verify that the solution is properly running we suggest to use [Postman](https://identity.getpostman.com/signup?continue=https%3A%2F%2Fgo.postman.co%2Fbuild) to join the bot to a Microsoft Teams meeting.

[Create](https://support.microsoft.com/en-us/office/schedule-a-meeting-in-teams-943507a9-8583-4c58-b5d2-8ec8265e04e5) a new Microsoft Teams meeting and join it.

|![Microsoft Teams Invite Link](images/invite_link.png)|
|:--:|
|*Steps to copy the invite Link from Microsoft Teams*|

Once you have joined the meeting, copy the invitation link from the meeting.

Open postman and create a new POST request pointing to the following address: `https://localhost:8442/api/call/initialize-call` (is the same domain you have configured in the launchSettings of the managementApi)

In the header tab, add (if it does not exist) a new key `Content-Type` with the value `application/json`.

|![Postman Header](images/postman_header.png)|
|:--:|
|*Postman content-Type header*|

In the body tab select raw and complete by copying the following

```json
{
    "MeetingUrl": "{{microsoftTeamsInviteLink}}"
}
```

Placeholder | Description
---------|----------
 microsoftTeamsInviteLink | Microsoft Teams invite link

 Click on the `Send` button and the request will be sent to the solution, you should receive a response with status `200 Ok` and after a few seconds the bot should join the Microsoft Teams meeting.

|![Test with Postman](images/test_with_postman.png)|
|:--:|
|*Test the solution with postman*|
