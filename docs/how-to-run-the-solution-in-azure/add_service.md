# How to Add the Service 

## Introduction
In order to start using the Azure environment once all the components have been deployed and configured, it is necessary to configure/register a service into the Cosmos DB.  

## Dependencies
To configure/register the service, the following dependencies need to be created:

- [Management API ](deploy_web_app.md).
- [BotOrchestrator deployed](deploy_function_app.md).
- [Cosmos DB](cosmos_db.md).

>**NOTE**: The Web and the Function Apps not only need to be created but also both components (Management API and Orchestrator Function) need to be deployed and configured.

### Configure/Register
The service is configured/registered through the Management API by making a `POST` request to the `/api/service`. The snippet below shows the `payload` request needed.

```json
{
    "friendlyName": "{{serviceName}}",
    "resourceGroup": "{{virtualMachineResourceGroup}}",
    "subscriptionId": "{{subscriptionIdOfResourceGroup}}",
    "name": "{{virtualMachineName}}",
    "dns": "{{virtualMachineDnsName}}",
    "isDefault": "{{serviceDefault}}"
}
```

| Placeholder                            | Description                                                                         |
|----------------------------------------|-------------------------------------------------------------------------------------|
| serviceName                            | A meaningful name for the service to be configured/registered, e.g. `Test service`. |
| virtualMachineResourceGroup            | The [resource group](readme.md#resource-groups) name where the virtual machine was created. |
| subscriptionIdOfResourceGroup          | The subscription Id where the virtual machine resource group was created.           |
| virtualMachineName                     | The name of the [virtual machine](bot_service_virtual_machine.md)                   |
| serviceDefault                         | Indicates whether the service to be added is the default, set it to `true`.     |

### Configure/register the service example
You can use any HTTP client to configure/register the service to be used by the solution. In this example, the client used is `Postman`.

Open `Postman` and create a new `POST` request pointing to the following endpoint: https://{{webAppUrl}}/api/service 

| Placeholder                            | Description                                                                         |
|----------------------------------------|-------------------------------------------------------------------------------------|
| webAppUrl                              | This is the [Web App](web_app_and_app_service_plan.md) service in Azure URL where the Management API was deployed      |

In the authorization tab, select `Bearer Token` for `Type` and add the authorization token in the corresponding `Token` input.

![Postman authorization header](./images/postman_add_service_auth_header.png)

To get the authorization token for the Management API resource you can follow the steps described in this [document](authorization_token.md).

In the header tab, add (if it does not exist) a new key `Content-Type` with the value `application/json`.

In the body tab select raw and complete by copying the following

![Postman select body type](./images/postman_add_service_payload.png)

Click on send to configure/register the service. 

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)
