# How to run the solution in Azure

## Prerequisites

Before configuring the solution to run it locally and/or in azure you must have the following prerequisites:

- A **domain name** - This domain name will be used to host the solution's core components.
- An **SSL wildcard certificate** - A valid wildcard certificate for the domain mentioned in the previous point. This is required to establish a TLS/TCP control channel between the bot's media platform and the calling clouds. The certificate must be in .pem and .pfx formats.
- An **Office 365 tenant with Microsoft Teams** enabled - If the organization is already using Microsoft Teams for their meetings, then you already have an Office 365 tenant configured.
  - Note that you need the create an app registration in this tenant with permissions to join meetings and send and receive audio and video in those meetings. These permissions will need to be approved by your Office 365 tenant administrator.
  - If your organization doesn't want to provide the necessary permissions for the solution to connect to the meetings, or you just want to test this solution in an isolated tenant, you can obtain a new testing tenant using Office 365's [Developer Program](https://developer.microsoft.com/en-us/microsoft-365/dev-program).
- If you plan to deploy this solution to the cloud then you will need an **Azure subscription** to create the required resources to host the solution. Also, Azure AD needs to be used to created several app registrations for authentication and authorization.

## Provision Azure resources

Before provisioning main Azure resources, you must create a resource group for them ([How to create a resource group](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups)) and then create the following resources under that resource group.

1. [Application Insights](application-insights.md#application-insights)
2. [App Service Plan](app-service-plan.md#app-service-plan)
3. [Web App](web-app.md#web-app)
4. [Function App](function-app.md#function-app)
5. [Virtual Machine](virtual-machine.md#virtual-machine)
6. [Azure Key Vault](azure-key-vault.md#azure-key-vault)
7. [Cosmos DB](cosmos-db.md#cosmos-db)
8. [Storage Account](storage-account.md#storage-account)

## Setup DNS for the Virtual Machine

After creating and configuring the virtual machine, you must add a record to your DNS provider/server with the static IP of the virtual machine, and take note of the record name, you will use it later.

If you don't have a DNS provider, you can use [Azure DNS](https://azure.microsoft.com/services/dns/). To create an Azure DNS Zone, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/dns/dns-getstarted-portal).

If you chose Azure DNS, you will have to visit your domain name registrar to replace the name server records with the Azure DNS name servers. More info, [host your domain in Azure DNS](https://docs.microsoft.com/en-us/azure/dns/dns-delegate-domain-azure-dns#delegate-the-domain).

## Configure authentication resources

To configure authentication in the solution, secure and connect several of the solution's resources, you to create app registrations for the corresponding components, and a security group.

### App Registrations
  
While creating/configuring the application registrations, you must create client secrets and use the Azure Key Vault created in previous steps to store them. We also recommend keeping track of the application IDs generated for each app registration to simplify the configuration of the applications during the project.

> **NOTE**: If you are using a O365 Developer tenant for Microsoft Teams and a separate Azure subscription tenant to create the Azure resources, the Azure SDK app registration must be created in the Azure subscription tenant, while the rest can be created in the developer tenant.

- [How to create/configure Bot Service API app registration](bot-service-api-app-registration.md#bot-service-api-app-registration)
- [How to create/configure Bot Service Client app registration](bot-service-client-app-registration.md#bot-service-client-app-registration)
- [How to create/configure Management API app registration](management-api-app-registration.md#management-api-app-registration)
- [How to create/configure Azure SDK app registration](azure-sdk-app-registration.md#azure-sdk-app-registration)

### Security Group

If you want to restrict the access to the solution to certain users, you must create a security group and assign the corresponding users to it. If don't, you can skip this step.

- [How to create/configure the security group](security-group.md#security-group)

## Azure Bot

In order to configure the solution and add calling capabilities to the bot, you must create an Azure Bot resource. To do so, before creating the Azure Bot, you need to create an app registration with the required permissions the Azure Bot will use to authenticate against the Microsoft Graph API and get access to the different resources.

- [How to create/configure Azure Bot app registration](../common/azure-bot-app-registration.md#azure-bot-app-registration)
- [How to create/configure Azure Bot](../common/azure-bot.md#azure-bot)

## Deployments

After provisioning and configuring the Azure resources, you are able to start deploying the different backend components. In this section will list the `How to` documents to deploy them.

- [How to deploy the Bot Service into the virtual machine](bot-service-deploy.md#bot-service-deploy)
- [How to deploy the Management API into the Azure App Service](management-api-deploy.md#management-api-deploy)
- [How to deploy the Function App into the Azure Function App Service](function-app-deploy.md#function-app-deploy.md)

## Configure Event Grid/Event Grid handler

To keep the state of the bot service virtual machine consistent is Cosmos DB, we we must configure an event grid subscription to execute an Azure Function that updates its register in Cosmos DB, every time that it is being started/stopped from an external event, e.g.: A user starts/stops the virtual machine from Azure Portal or has scheduled auto-shutdown. To do so, please review the following document:

- [How to configure event grid](event-grid.md#event-grid)

## Register the service in Cosmos DB

In order to start using the Azure environment once all the components have been deployed and configured, it is necessary to configure/register a service into the Cosmos DB.

- [How to register the service](register-service.md)

## Test the environment

- [Quick test the Management API](test-web-app.md#how-to-test-the-management-api)
- [Quick test the Azure Function](test-function-app.md#how-to-test-the-orchestrator-function)
