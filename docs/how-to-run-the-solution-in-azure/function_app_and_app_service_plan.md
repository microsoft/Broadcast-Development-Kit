# Function App

## Introduction
To host the Bot Orchestrator function a Function App must be created in Azure. This document shows how to create the Function App for the Bot Orchestrator.

## Dependencies
To create the Function App service needed to deploy the Orchestrator function, the following resources must be already created:

- [App Service Plan](service_plan.md). 
- [Azure Storage Account](storage_account.md).
- [Application Insights](application_insights.md). 

### Settings
Fill the fields in the creation wizard with the following information:

- ***Basic:***
    - ***Resource Group:*** Select the [resource group](README.md#architecture-resource-group) created for the solution architecture.
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

To create the Function App and App Service Plan, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-function-app-portal#create-a-function-app).


[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)