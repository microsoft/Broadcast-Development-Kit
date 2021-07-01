# Function App and App service plan

## Introduction
A Function App and App Service Plan for it must be created to host the Azure VM Management. This document is intend to show, how to create a Function App in the resource group and fill the fields in the creation wizard with the following information:

- ***Resource Group:*** Select the resource group created for the solution architecture.
- ***Name:*** A meaningful name.
- ***Publish:*** Code.
- ***Runtime stack:*** .NET Core 3.1 (LTS).
- ***Region:*** Same region as the rest of the resources.
- ***Hosting:*** 
    - ***Storage account:*** Select the first storage account that was created in the previous steps. 
    - ***Operative system:*** Windows. 
    - ***Plan:*** App Service plan. 
    - ***Windows Plan:*** Click on **Create** new and change the name to something meaningful. 
    - ***Sku and size:*** Basic B1. 
- ***Monitoring:*** Enable application insights and select the instance that was created in a previous step.

To create the Function App and App Service Plan, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-function-app-portal#create-a-function-app).

Once the **Function App** is created from the Azure Portal, it is necessary to deploy the solution. For that, it is suggested to review the following [documentation](deploy_function_app.md).

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)