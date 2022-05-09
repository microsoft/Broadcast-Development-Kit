# Azure Key Vault

The Azure Key Vault instance will be used to store the solution's secrets and the SSL certificate. To create the Application Insights, please review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/key-vault/general/quick-create-portal#create-a-vault), and use the following settings:

- ***Resource Group:*** Select the resource group created for the main resources.
- ***Key vault name:*** A meaningful name.
- ***Region:*** Same region as the rest of the resources.
- ***Pricing tier:*** Standard.

## Domain certificate

As part of the configuration, you must import the domain certificate key vault, and add its password and thumbprint as secrets.

### Import domain certificate

To import the SSL certificate with .pfx file format into key vault, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate?tabs=azure-portal#import-a-certificate-to-your-key-vault)

### Add certificate secrets

To add the password and the thumbprint as secrets, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-portal#add-a-secret-to-key-vault), and consider the following names for both secrets:

- **Password:** `Settings--BotConfiguration--CertificatePassword`
- **Thumbprint:** `Settings--BotConfiguration--CertificateThumbprint`

## Assign access policies for Web App Service, Azure Function App Service and Virtual Machine

In order to allow to both app services and the virtual machine to get access to the Key Vault secrets, it is necessary to assign access policies for the app services and the virtual machine. To know how to do it, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/key-vault/general/assign-access-policy?tabs=azure-portal#assign-an-access-policy), and consider the following settings:

- **Key Permissions:** Get, List
- **Secret Permissions:** Get, List
- **Certificate Permissions:** Get, List
- **Select Principal**: Object (principal) Id of the Web App Service/Function App service system managed identity

> NOTE: You must repeat this process for each app service and the virtual machine.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Cosmos DB →](cosmos-db.md#cosmos-db)
