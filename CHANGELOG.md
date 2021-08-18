# Changelog

Here you can find the changelog for the pre-release versions that are available as [releases in this repo](https://github.com/microsoft/Broadcast-Development-Kit/releases).

## Notice

This is a **PRE-RELEASE** project and is still in development. This project uses the [application-hosted media bot](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/requirements-considerations-application-hosted-media-bots) SDKs and APIs, which are still in **BETA**.

The code in this repository is provided "AS IS", without any warranty of any kind. Check the [LICENSE](LICENSE) for more information.

## 0.5.0-dev

- Fixed an issue where some participants in the call were not able to see the video injected by the bot.
    - Note that this fix is actually a workaround for a behavior in Microsoft Teams where the Teams client didn't render the video if the injection was started more than 3 minutes after the bot was joined to the meeting. The bot now shows a **slate** in the meeting when no injection is active to prevent the video socket in the Teams client from closing.
    - You can personalize this slate following the instructions in the [Customize the Broadcast Development Kit Slate image](docs/common/customize_bdk_slate_image.md) document.
- Added support for extracting video from the call using the RTMP/RTMPS protocols in pull mode. Previously, only push mode was supported for extractions with RTMP/RTMPS.
- Updated to the latest version of the **Microsoft Graph Communications** SDKs (v1.2.0.3144).
- Change how the status of the service is managed to separate the state of the **BotService** from the state of the underlying virtual machine.
- Updated the initialization logic of the **BotService** when running as a Windows service, to reduce the chance of Windows killing the service if the start-up process takes too long.

## 0.4.0-dev

- Initial pre-release of the solution.
