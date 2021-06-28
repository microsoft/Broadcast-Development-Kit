# Prerequisites
Before configuring the solution to run it locally and/or in azure we must have the following prerequisites:


- A domain name - This domain name will be used to host the solution's core components.
- An SSL wildcard certificate - A valid wildcard certificate for the domain mentioned in the previous point. This is required to establish a TLS/TCP control channel between the bot's media platform and the calling clouds. The certificate must be in .pem and .pfx formats.
- An Office 365 tenant with Microsoft Teams enabled - If the organization is already using Microsoft Teams for their meetings, then you already have an Office 365 tenant configured.
- [An Azure Bot](azure_bot.md) - This will be used to add calling capabilities to the the bot, to authenticate against the Microsoft Graph APIs and connect the solution to the meetings that are hosted in your tenant.