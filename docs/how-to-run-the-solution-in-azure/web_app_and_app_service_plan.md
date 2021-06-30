# Web App and App service plan

## Introduction.
A Web App and App Service Plan for it must be created to host the Management API.

This document is intend to show, how to create a Web App in the resource group and fill the fields in the creation wizard with the following information: 

- ***Resource Group:*** The resource group create in a previous step.
- ***Name:*** A meaningful name.
- ***Publish:*** Code.
- ***Runtime stack:*** .NET Core 3.1 (LTS).
- ***Region:*** Same region as the rest of the resources.
- ***App Service Plan:*** For the service plan, please select create a new one with the following values:
    - ***Name:*** Change the name to something meaningful.
    - ***Sku and size:*** Shared D1.
    > **NOTE**: This tier (Shared D1) is to reduce costs during the test. It can be increased if needed.
- ***Monitoring:*** Enable application insights and select the instance that was created in a previous step.

To create the Wep App and App Service Plan, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/app-service/?WT.mc_id=Portal-Microsoft_Azure_Marketplace).

Once the **Web App** is created from the Azure Portal, it is necessary to deploy the solution. For that, it is suggested to review the following [documentation](deploy_web_app.md).

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)



