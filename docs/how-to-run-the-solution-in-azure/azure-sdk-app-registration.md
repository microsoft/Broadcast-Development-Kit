# Azure SDK app registration

This documents explains how to create and configure the Azure SDK app registration to enable the backend components to get access to Azure resources through Azure SDK.

## Creation of the app registration

To create the app registrations, review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app#register-an-application) that will explain how to do it, and consider the following settings:

- ***Name:*** Meaningful name.
- ***Supported account types:*** Accounts in this organizational directory only (`your-organization` only - Single tenant).

## Setup of the app registration

### Add a client secret

Finally, you must [add a client secret](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app#add-a-client-secret), copy the value and add it to the key vault as a secret with the following name:
`Settings--AzServicePrincipalConfiguration--ApplicationClientSecret`.

## Give access to the virtual machine

In order to allow to the backend components to turn on/off the virtual machine through Azure SDK, you give the app registration access to the virtual machine with `Contributor Role` through **Access control (IAM)** menu of the virtual machine resource.

[← Back to How to run the solution in Azure](README.md#app-registrations) | [Next: Security Group →](README.md#security-group)
