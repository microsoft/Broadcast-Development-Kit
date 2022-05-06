# Changelog

Here you can find the changelog for the pre-release versions that are available as [releases in this repo](https://github.com/microsoft/Broadcast-Development-Kit/releases).

## Notice

This is a **PRE-RELEASE** project and is still in development. This project uses the [application-hosted media bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/requirements-considerations-application-hosted-media-bots) SDKs and APIs, which are still in **BETA**.

The code in this repository is provided "AS IS", without any warranty of any kind. Check the [LICENSE](LICENSE) for more information.

## Upgrade path

To upgrade from one version to the next one you simply need to deploy the new version to the resources you already have in Azure. However, some configuration settings might change between one version and the next one. You can find a quick summary of what settings have changed between each version in the [upgrade path documentation](docs/upgrade_path.md).

## 0.6.0-dev

- Updated injection logic to support new features/fix bugs
  - Added support to resume an injection without having to stop a current injection and start it again when an SRT/RTMP source is disconnected and reconnected.
    - When a user starts an injection but the injection pipeline doesn't receive content, the injection remains in `Ready` state, and the bot only injects the static image. Once the injection pipeline starts receiving content,  the injection goes from `Ready` to `Receiving` state and the bot switches the injection from the static image to the actual content.
    - If the injection pipeline stops receiving content (e.g.: because the source dropped), the injection goes from `Receiving` to `Not Receiving` state, and the bot starts injecting the static image again. Once the injection pipeline starts receiving content again,  the injection goes from `Not Receiving` to `Receiving` state and the bot switches the injection from the static image to the actual content.
- Added support to change the injection's volume.
- Added support to hide/display an injection without stopping the injection (closing the SRT/RTMP connection).
- Added support to remove the bot from the meeting after a configurable time (in seconds) without participants.
- Fixed the bug where the bot's mute state wasn't updated where it was muted from Microsoft Teams client.
- Updated to the latest version of the **Microsoft Graph Communications** SDKs (v1.2.0.4161) and its dependencies.
- Updated to the latest version of the **Microsoft.Skype.Bots.Media**  (1.23.0.49-alpha)
- Configuration management
  - Added Key Vault and moved the domain certificate and secrets there. Now the Web App Service and Function App service use Key Vault references. Regarding the Bot Service hosted in the virtual machine, we remove its settings from the storage account. Now the bot gets the non-sensitive settings from a local appSettings.json and the secrets from the Key Vault.
- Added **Swashbuckle.AspNetCore** package in Management API to expose Swagger Documentation

## 0.5.0-dev

- Fixed an issue where some participants in the call were not able to see the video injected by the bot.
  - Note that this fix is actually a workaround for a behavior in Microsoft Teams where the Teams client didn't render the video if the injection was started more than 3 minutes after the bot was joined to the meeting. The bot now shows a **slate** in the meeting when no injection is active to prevent the video socket in the Teams client from closing.
  - You can personalize this slate following the instructions in the [Customize the Broadcast Development Kit Slate image](docs/common/customize_bdk_slate_image.md) document.
- Added support for extracting video from the call using the RTMP/RTMPS protocols in pull mode. Previously, only push mode was supported for extractions with RTMP/RTMPS.
- Updated to the latest version of the **Microsoft Graph Communications** SDKs (v1.2.0.3144).
- Changed how the status of the service is managed to separate the state of the **BotService** from the state of the underlying virtual machine.
- Updated the initialization logic of the **BotService** when running as a Windows service, to reduce the chance of Windows killing the service if the start-up process takes too long.

## 0.4.0-dev

- Initial pre-release of the solution.
