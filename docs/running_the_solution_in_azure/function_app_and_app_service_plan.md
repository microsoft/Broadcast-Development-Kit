# Function App and App service plan

## Introduction
A Function App and App Service Plan for it must be created to host the Management API. This document is intend to show, how to create a Function App in the resource group and fill the fields in the creation wizard with the following information:

 ***Resource Group***: The resource group create in a previous step.
- ***Name***: A meaningful name.
- ***Publish***: Code.
- ***Runtime stack***: .NET Core 3.1 (LTS).
- ***Region***: Same region as the rest of the resources.
- **Hosting**: 
    - **Storage account:** Select the first storage account that was created in the previous steps. 
    - **Operative system:** Windows 
    - **Plan:** App Service plan 
    - **Windows Plan:** Click on **Create** new and change the name to something meaningful. 
    - **Sku and size:** Basic B1. 
- **Monitoring**: Enable application insights and select the instance that was created in a previous step.

