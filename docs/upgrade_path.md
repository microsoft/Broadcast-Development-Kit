# Upgrade path

Here you can find a quick summary of the configuration changes required to upgrade from a previous [pre-release version](https://github.com/microsoft/Broadcast-Development-Kit/releases) to the next one.

## From 0.4.0-dev to 0.5.0-dev

While all the configuration settings are the same between versions **0.4.0-dev** and **0.5.0-dev**, the later includes support for extracting feeds using RTMP/S in pull mode. This requires to apply some changes in the list of ports available in your virtual machine, as well as the configuration of the NGINX server that is used to listen to RTMP/S connections.

- Please review the [inbound rules](how-to-run-the-solution-in-azure/virtual_machine.md#network-security-group-inbound-rules) listed in the documentation and update your VM to match those rules.

- Check the latest [settings for the NGINX server](common/install-and-configure-nginx-with-rtmp-module-on-windows.md#installation) and update your NGINX configuration file in the VM to match these settings.

The version **0.5.0-dev** also included an slate image used by the bot when no injection is active. Optionally, you can [change the slate image to one of your choosing](common/customize_bdk_slate_image.md).

## From 0.5.0-dev to 0.6.0-dev

In this release we introduced some breaking changes related to solution's deployment/configuration.
We added Azure Key Vault, migrated the secrets there and change the way the Bot Service get access to its configuration among other features and bug fixes that you can see in the [CHANGELOG](../CHANGELOG.md).

To upgrade from a previous version to the `0.6.0-dev` we highly recommend reviewing the updated documentation to setup an [Azure](how-to-run-the-solution-in-azure/README.md) / [local](how-to-run-the-solution-locally/README.md) environment.

That being said, you will have to:

- [Create an Azure Key Vault](how-to-run-the-solution-in-azure/azure-key-vault.md#azure-key-vault)
  - Create azure resource
  - Upload domain certificate
  - Add certificate secrets
  - Assign access policies for Web App Service, Azure Function App Service and Virtual Machine
- Migrate secrets to Key Vault
  - Add secret for [Cosmos DB](how-to-run-the-solution-in-azure/cosmos-db.md) primary key with the following name:
    - `Settings--CosmosDbConfiguration--PrimaryKey`
  - Add secret for [Storage Account](how-to-run-the-solution-in-azure/storage-account.md) Connection string with the following name:
    - `Settings--StorageConfiguration--ConnectionString`
  - Add secret for [Bot Service Client app registration](how-to-run-the-solution-in-azure/bot-service-client-app-registration.md) with the following name:
    - `Settings--BotServiceAuthenticationConfiguration--ClientSecret`
  - Add secret for [Azure SDK app registration](how-to-run-the-solution-in-azure/azure-sdk-app-registration.md) with the following name:
    - `Settings--AzServicePrincipalConfiguration--ApplicationClientSecret`
  - Add secret for [Azure Bot app registration](how-to-run-the-solution-in-azure/azure-bot-app-registration.md) with the following names:
    - `Settings--BotConfiguration--AadAppSecret`
    - `Settings--GraphClientConfiguration--ClientSecret`
- Virtual Machine
  - Update GStreamer from 1.18.4 to 1.18.6 (We highly recommend uninstall the previous version and install the new one)
  - **[Optional]** Remove BotService environment variables (review old documentation). Keep GStreamer environment variables.
- **[Optional]** Storage Account
  - Remove PFX certificate
  - Remove Bot Service settings json
- Deployment
  - [Management API](how-to-run-the-solution-in-azure/management-api-deploy.md)
    - Publish latest version
    - Update App Service configuration settings (review [Configuring app settings](how-to-run-the-solution-in-azure/management-api-deploy.md#configuring-app-settings) section)
  - [Function App](how-to-run-the-solution-in-azure/function-app-deploy.md)
    - Publish latest version
    - Update App Service configuration settings (review [Configuring app settings](how-to-run-the-solution-in-azure/function-app-deploy.md#configuring-app-settings) section)
  - [Bot Service](how-to-run-the-solution-in-azure/bot-service-deploy.md)
    - Publish latest version
    - Update `appSettings.json` configuration settings (review [Updating appSettings.json](how-to-run-the-solution-in-azure/bot-service-deploy.md#updating-appsettingsjson)
