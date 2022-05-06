# Azure Bot

To continue with the registration of the calling bot, we must create an [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0#create-an-azure-bot-resource). While doing so, consider the following settings:

- **Bot handle:** A meaningful name.
- **Subscription** Your Azure subscription.
- **Resource group**: The resource group where you want to create the resource  
- **Pricing tier:** F0.
- **Microsoft App ID:**
  - **Type of App**: Single Tenant
  - **Creation Type**: Click on `Use existing app registration`, then input the client ID, client secret and tenant ID fo the Azure Bot application registration you created in previous steps.
- Leave the rest of the settings as-is.

Once the Azure Bot is created, apply the following changes to the resource:

1. In the resource blade, click `Channels`, then go to the `Available channels` section, select the `Microsoft Teams` channel, and agree the Terms of Service.

2. In the `Messaging` tab, select Microsoft Teams Commercial.

3. Go to the `Calling` tab and check the `Enable calling` checkbox. In the `Webhook` URL, if you are configuring the solution to run it locally, add a dummy value. If you are configuring the solution to run it in Azure, add the path to the following route using your domain name:

   - `https://{domain-name}/api/calling`

4. Click `Apply` to save the changes.

> **Optional:** You can go to `Bot profile` section and upload an icon for this bot or change the name that it will show inside the meetings.

[← Back to How to run the solution in Azure](../how-to-run-the-solution-in-azure/README.md#azure-bot) | [Next: Deployments →](../how-to-run-the-solution-in-azure/README.md#deployments)

[← Back to How to run the solution locally](../how-to-run-the-solution-locally/README.md#azure-bot) | [Next: Tools →](../how-to-run-the-solution-locally/README.md#tools)
