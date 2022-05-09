# Azure Bot app registration

This documents explains how to create and configure the Azure Bot app registration you will use to configure the Azure Bot resource.

## Creation of the app registration

To create the app registrations, review the following [Microsoft documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app#register-an-application) that will explain how to do it, and consider the following settings:

- ***Name:*** Meaningful name.
- ***Supported account types:*** Accounts in this organizational directory only (`your-organization` only - Single tenant).

## Setup of the app registration

### API permissions

From the Azure Bot app registration view, go to the **API permissions** option that is in the resource blade, click the **Add a permission** button, select the **Microsoft APIs** tab, **Microsoft Graph** and then **Application permissions**.

Once there, add the permissions shown in the table below:

API / Permission name  | Type | Display | Description | Admin consent
----------|----------|----------|----------|----------
 Calls.AccessMedia.All  | Application | Access media streams in a call as an app preview. | Allows the app to get direct access to media streams in a call, without a signed-in user. | Yes
 Calls.Initiate.All | Application | Initiate outgoing 1:1 calls from the app preview.| Allows the app to place outbound calls to a single user and transfer calls to users in your organization’s directory, without a signed-in user. | Yes
 Calls.InitiateGroupCall.All  | Application | Initiate outgoing group calls from the app preview | Allows the app to place outbound calls to multiple users and add participants to meetings in your organization, without a signed-in user. | Yes
 Calls.JoinGroupCall.All | Application | Join group calls and meetings as an app preview. | Allows the app to join group calls and scheduled meetings in your organization, without a signed-in user. The app is joined with the privileges of a directory user to meetings in your tenant. | Yes
 Calls.JoinGroupCallAsGuest.All | Application | Join group calls and meetings as a guest preview. | Allows the app to anonymously join group calls and scheduled meetings in your organization, without a signed-in user. The app is joined as a guest to meetings in your tenant. | Yes
 User.Read | Delegated | Sign in and read user profile | Allows users to sign-in to the app, and allows the app to read the profile of signed-in users. It also allows the app to read basic company information of signed-in users | No
 User.Read.All | Application | Read all users' full profiles | Allows the app to read user profiles without a signed in user. We use it to retrieve get the user photos | Yes

Several of the permissions that were configured in this application require to be consented by an admin of the Azure AD tenant. To grant this consent, ask an Azure AD administrator to access the following URL and accept the permissions for this application (replace the {{applicationId}} placeholder with the id of the application registration previously created):

- `https://login.microsoftonline.com/common/adminconsent?client_id={{applicationId}}&state=1`

> **Note**: Despite that constructing the Tenant Admin Consent URL requires a configured Redirect URI/Reply URL in the App Registration Portal, to grant consent manually we don't need to add it to the reply URL in the consent URL. To add reply URLs for your bot, access your bot registration, choose Advanced Options > Edit Application Manifest. Add your Redirect URI to the field reply URLs.

### Add a client secret

Finally, you must [add a client secret](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app#add-a-client-secret).

If you are following the guide to configure and run the solution in Azure, you must copy the secret value and add it to the key vault as a secret with the following names:

- `Settings--BotConfiguration--AadAppSecret`
- `Settings--GraphClientConfiguration--ClientSecret`

If you are following the guide to configure and run the solution locally, just take note of this value and copy it in a secure place, you will use it for later configuration.

[← Back to How to run the solution in Azure](../how-to-run-the-solution-in-azure/README.md#azure-bot) | [Next: Azure Bot →](azure-bot.md#azure-bot)

[← Back to How to run the solution locally](../how-to-run-the-solution-locally/README.md) | [Next: Azure Bot →](azure-bot.md#azure-bot)
