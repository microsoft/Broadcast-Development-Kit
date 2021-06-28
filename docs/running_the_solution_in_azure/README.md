# Running the solution in Azure

## Introduction

This document describes the resources that must be created and configured to running the solution in Azure. This includes:
- App Registrations for authentication in different components in Azure AD tenant:
    - App Registrations:
        - Bot Channel App.
        - Bot Service API.
        - Bot Service Client.
        - Azure VM Management.
        - Management API.
    - Security Group.
- Resource groups used to deploy and configure the solution: 
    - Virtual Machine resource group:
        - Bot Channels registration.
        - Virtual Machine.
        - Event Grid.
    - Architecture resource group:
        - Application Insights.
        - Storage account.
        - Web App and App service plan.
        - Fucntion App and App service plan.
        - Cosmos DB.

## App Registrations for authentication in different components in Azure AD tenant
To secure and connect several of the resources used for the solution, we need to create several app registrations, each with its own permissions and settings. Several of the following instructions include creating application credentials. We recommend creating a Key Vault resource in Azure to store these credentials securely. We also recommend keeping track of the application IDs generated for each app registration to simplify the configuration of the applications during the project.  

- [App Registrations](app_registrations.md#app-registrations).
- [Security Group](security_group.md).

## Resources used to deploy and configure the solution:
### Resource Groups 
To prepare the cloud environment, we need to create multiple resources which must be separated according to the different components of the solution. For that, it is necessary to create two **resource groups**, both in the same **region** (e.g., **West US 2**). 

- **`resource-group-name`-bot-vm**: This group will be used to contain the resources related to the virtual machine that will host the core components of the application in Azure. 

- **`resource-group-name`-bot**: This group will contain the rest of the resources related to the APIs, functions, database, and web UI used to operate the solution.

> NOTE: It is suggested that `resource-group-name` be replaced by a name in line with the project.

To create the resource groups, check the [Create resource groups](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups) documentation.


### Virtual Machine resource group
The following resources form the core part of the solution, which is charge of connecting to the call and extract and inject the media feeds from the Teams Meeting call. 
All these resources should be created in the `resource-group-name`-bot-vm resource group. 
The app registration that was created to manage the state of the VM must be given access to this resource group with the Contributor role. This can be done in the **Access control (IAM)** menu of the resource group.

- [Bot Channels registration]().
- [Virtual Machine](bot_service_virtual_machine.md).
- [Event Grid](configure_event_grip.md).

### Architecture resource group
The following resources are used to manage the application and the bot. All these resources should be created in the `resource-group-name`-bot resource group:

- [Application Insights](application_insights.md).
- [Storage account](storage_account.md).
- [Web App and App service plan]().
- [Function App and App service plan]().
- [Cosmos DB](cosmos_db.md).