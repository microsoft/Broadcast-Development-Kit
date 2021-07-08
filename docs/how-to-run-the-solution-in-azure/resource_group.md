# Resource Group

## Getting Started

To organize the different components of the solution we recommend creating two **resource groups** in your Azure subscription, both in the same **region**(e.g., **West US 2**).

- **resource-group-name-bot** – This  group  will  contain  the  rest  of  the resources related to the APIs, functions, database, and web UI used to operate the solution.
- **resource-group-name-bot-vm** – This group will be used to contain the resources  related  to  the  virtual  machine  that  will  host  the  core components of the application in Azure

To create the resource groups, check the [Create resource groups](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups) documentation.

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)