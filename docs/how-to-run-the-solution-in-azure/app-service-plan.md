# App Service Plan

This App Service Plan will define the resources available to execute the Management API and Azure function. To create the App Service Plan, please review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/app-service/app-service-plan-manage#create-an-app-service-plan) and use the following settings:

- ***Resource Group:*** Select the resource group created for the main resources.
- ***Name:*** A meaningful name.
- ***Operating System:*** Windows.
- ***Region:*** Same region as the rest of the resources.
- ***Pricing Tier***
  - ***Sku and size:*** Basic B1.
    > **NOTE**: The tier (Shared D1) can be used to reduce costs during the test. However note that this can cause issues, like the Azure Functions not processing the messages from the queues.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Web App →](web-app.md#web-app)
