# Application Insights

This Application Insights instance will be used to log all the events happening in the solution. To create the Application Insights, please review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-workspace-resource), and use the following settings:

- ***Resource Group:*** Select the resource group created for the main resources.
- ***Name:*** A meaningful name.
- ***Region:*** Same region as the rest of the resources.
- ***Resource mode:*** Classic.

Once the **Application Insights** has been created, within the **Overview** option on the left panel we can view the Instrumentation key. The **instrumentation key** identifies the resource that you want to associate your telemetry data with. You will need to copy the **instrumentation key** to use it later.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: App Service Plan →](app-service-plan.md#app-service-plan)
