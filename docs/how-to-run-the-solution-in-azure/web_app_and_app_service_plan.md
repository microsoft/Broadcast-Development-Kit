# Web App and App service plan

## Introduction.
A Web App and App Service Plan for it must be created to host the Management API.

This document is intend to show, how to create a Web App in the resource group and fill the fields in the creation wizard with the following information: 

- ***Resource Group:*** Select the resource group created for the solution architecture.
- ***Name:*** A meaningful name.
- ***Publish:*** Code.
- ***Runtime stack:*** .NET Core 3.1 (LTS).
- ***Operating System:*** Windows.
- ***Region:*** Same region as the rest of the resources.
- ***App Service Plan:*** For the service plan, please select create a new one with the following values:
    - ***Name:*** Change the name to something meaningful.
    - ***Sku and size:*** Shared D1.
    > **NOTE**: This tier (Shared D1) is to reduce costs during the test. It can be increased if needed.
- ***Monitoring:*** Enable application insights and select the instance that was created in a previous step.

### Create Web App in Azure
1. In the [Azure Portal](), click **Create a resource** > **Web** > **Web App**.

    ![imagen](images/web_app_in_portal.png)
1. Select the subscription and complete the fields following the indications in the previous section, and then press the tab **Monitoring**.
    
    ![imagen](images/web_app_create.png)
1. Enable the Application Insights by checking the option **Yes**, and select the Application Insights created in previous steps, then press the button **Review + create**.

    ![imagen](images/web_app_monitoring_disable.png)
1. Verify the information created and click on the **Create** button to finish with the creation.

Once the **Web App** is created from the Azure Portal, it is necessary to deploy the solution. For that, it is suggested to review the following [documentation](deploy_web_app.md).

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)