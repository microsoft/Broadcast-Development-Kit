# Web App

To host the Management API a Web App service must be created in Azure. This document shows how to create the Web App for this API. To create the Web App, please review the following the [Create a web app](#create-a-web-app) section.

## Create a web app

Sign in to the [Azure portal](https://portal.azure.com/learn.docs.microsoft.com) using the same account you used to activate the sandbox.

1. On the Azure portal menu, or from the Home page, select Create a resource. Everything you create on Azure is a resource. The Create a resource pane appears. Here, you can search for the resource you want to create, or select one of the popular resources that people create in the Azure portal.

2. In the Create a resource menu, select Web.

3. Select Web App. If you don't see it, in the search box, search for and select Web App. The Create Web App pane appears.

4. On the Basics tab, enter the following settings:
   - ***Basic:***
     - ***Resource Group:*** Select the resource group created for the main resources.
     - ***Name:*** A meaningful name.
     - ***Publish:*** Code.
     - ***Runtime stack:*** .NET Core 3.1 (LTS).
     - ***Operating System:*** Windows.
     - ***Region:*** Same region as the rest of the resources.
     - ***App Service Plan:***
       - ***Windows plan:*** Select the [App Service plan](app-service-plan.md) created in the previous steps.
   - ***Monitoring:***
     - Enable [application insights](application-insights.md) and select the instance created in previous steps.

5. Select **Review + Create** to go to the review pane, and then select Create. The portal shows the deployment pane, where you can view the status of your deployment.

## Enable Managed Identity

To allow to the App Service to get access to key vault (through Key Vault References), you have to enable a system assigned managed identity. To do so, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp#add-a-system-assigned-identity), and take note of the **Object (principal) ID**, you will need it in future steps to configure the Azure Key Vault.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Function App →](function-app.md)
