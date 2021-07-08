# Azure SDK Service Principal

## Getting Started

The solution has components that require interaction with Azure resources using the Azure SDK. To allow this interaction, we need to create a Service Principal for the solution and assign it `Contributor` access to the specified Azure Resources. In this guide, we are going to explain how to assing the role to the app registration created in previous steps.

## Dependencies
To continue with the Azure SDK Service Principal documentation, the following dependencies need to be created:

- [Resource Group](resource_group.md).

### Assign Contributor Role
For the time being, the solution needs to interact with the virtual machine where the Bot Service API is hosted (to turn on/turn off the virtual machine). To make this assignment, you must go to the **resource group** where the **virtual machine** was created and through **Access Control (IAM)** assign `Contributor` role to the application, please review the following Microsoft [documentation](https://docs.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal?tabs=current).

![Assign Contributor Role](./images/assign_contributor_role.png)

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)