# Storage Account

The Storage Account will be used by the Azure function you are going to setup later. To create a Storage account in Azure, please review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) and use the following settings:

- ***Resource Group:*** Select the resource group created for the main resources.
- ***Storage account Name***: A meaningful name.
- ***Region***: Same region as the rest of the resources.
- ***Performance***: Standard.
- ***Redundancy***: Locally-redundant storage (LRS).

Finally, navigate to the **Access keys** option you have under the **Settings** section on the resource blade, take note of the `Connection string` and add it to the key vault as a secret with the following name:
`Settings--StorageConfiguration--ConnectionString`.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Setup DNS for the virtual machine →](README.md#setup-dns-for-the-virtual-machine)
