# Function App

To host the Bot Orchestrator function a Function App must be created in Azure. This document shows how to create the Function App for the Bot Orchestrator. To create the Function App and App Service Plan, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-function-app-portal#create-a-function-app) and use the following settings:

- ***Basic:***
  - ***Resource Group:*** Select the resource group created for the main resources.the solution architecture.
  - ***Name:*** A meaningful name.
  - ***Publish:*** Code.
  - ***Runtime stack:*** .NET Core 3.1 (LTS).
    - ***Region:*** Same region as the rest of the resources.
- ***Hosting:***
  - ***Storage account:*** Select the first [Storage Account](storage_account.md) that was created in the previous steps.
  - ***Operative system:*** Windows.
  - ***Plan:***
    - ***Plan type:*** App Service plan.
    - ***Windows Plan:*** Select the [App Service plan](service_plan.md) created in the previous steps.
- ***Monitoring:*** Enable [application insights](application_insights.md) and select the instance that was created in a previous step.

## Enable Managed Identity

To allow to the Azure Function App Service to get access to key vault (through Key Vault References), you have to enable a system assigned managed identity. To do so, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp#add-a-system-assigned-identity), and take note of the **Object (principal) ID**, you will need it in future steps to configure the Azure Key Vault.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Virtual machine →](virtual-machine.md#virtual-machine)
