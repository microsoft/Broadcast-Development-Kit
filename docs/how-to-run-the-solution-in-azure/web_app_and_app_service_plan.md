# Web App and App service plan

## Introduction.
A Web App and App Service Plan for it must be created to host the Management API. This document is intend to show, how to create a Web App in the resource group and fill the fields in the creation wizard with the following information: 

- ***Resource Group***: The resource group create in a previous step.
- ***Name***: A meaningful name.
- ***Publish***: Code.
- ***Runtime stack***: .NET Core 3.1 (LTS).
- ***Region***: Same region as the rest of the resources.
- ***App Service Plan***: For the service plan, please select create a new one with the following values:
    - ***Name***: Change the name to something meaningful.
    - ***Sku and size***: Shared D1.
    > Note: This tier (Shared D1) is to reduce costs during the test. It can be increased if needed.
- ***Monitoring***: Enable application insights and select the instance that was created in a previous step.

## Deploy the Management API into the Web App Service.
One the Web App with the corresponding App service plan was successful created, we can deploy the solution build into it. 

To deploy the Management API into the Web App Service created we can follow this steps:

1. Open the solution in **Visual Studio**.
1. In Solution Explorer, right-click in the project `ManagementApi` node and choose **Publish**.
1. In **Publish**, select **Azure** and then **Next**.  
![image]()
1. Choose in the **specific destination** the option Azure App Service (Windows).  
![image]()
1. Select your subscription and in the **Function Apps** panel, select the Function App that was created from the Azure Portal, and click **Finish**.  
![image]()
1. In the **Publish** page, select **Publish**. Visual Studio builds, packages, and publishes the app to Azure, and then launches the app in the default browser.

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)



