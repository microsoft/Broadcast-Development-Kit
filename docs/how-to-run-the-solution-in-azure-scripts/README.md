# How to run the solution in Azure (with scripts)

## Prerequisites

* [Install Azure CLI v 2.40](https://docs.microsoft.com/en-us/cli/azure/)
* [Install NodeJs](https://nodejs.org/en/)

## Running the scripts

1. Create a config.json file with the following values in the deployment folder.

    ```json
    {
        "name": "{{name}}",
        "managementResourceGroup": "{{managementResourceGroup}}",
        "managementResourceGroupLocation": "{{managementResourceGroupLocation}}",
        "vmResourceGroup": "{{vmResourceGroup}}",
        "vmResourceGroupLocation": "{{vmResourceGroupLocation}}",
        "appRegistrations":{
            "botAppName": "{{botAppName}}",
            "botServiceApiAppName": "{{botServiceApiAppName}}",
            "botServiceClientAppName": "{{botServiceClientAppName}}",
            "managementApiAppName": "{{managementApiAppName}}",
            "sdkAppName": "{{sdkAppName}}"
        },
        "dnsZone": {
            "dnsSubscriptionId": "{{dnsSubscriptionId}}",
            "resourceGroup": "{{dnsResourceGroup}}",
            "zoneName": "{{zoneName}}",
            "dnsRecordName": "{{dnsRecordName}}"
        },
        "botService":{
            "gstreamerInstallerUri": "https://gstreamer.freedesktop.org/data/pkg/windows/1.18.2/mingw/gstreamer-1.0-mingw-x86_64-1.18.2.msi",
            "gStreamerInstallationPath": "c:\\gstreamer\\1.0\\mingw_x86_64\\bin",
            "nginxInstallerUri": "https://github.com/illuspas/nginx-rtmp-win32/archive/refs/heads/dev.zip",
            "nssmInstallerUri": "https://nssm.cc/release/nssm-2.24.zip",
            "vcRedistInstallerUri": "https://aka.ms/vs/16/release/vc_redist.x64.exe",
            "localCertPath": "{{localCertPath}}",
            "pfxCertificatePath": "{{pfxCertificatePath}}",
            "pfxCertificatePassword": "{{pfxCertificatePassword}}",
            "pfxCertificateThumbprint": "{{pfxCertificateThumbprint}}",
            "vmUserName": "{{vmUserName}}",
            "vmPassword": "{{vmPassword}}",
            "botDisplayName": "{{botDisplayName}}",
            "botHandle": "{{botHandle}}"
        }
    }
    ```

    Placeholder | Value |
    ---------|----------|
    {{name}} | Used to create the resources, should not exceed 15 characters length eg: `projectName`. |
    {{managementResourceGroup}} | Name of the resource group where the management resources will be deployed. If left empty, it is automatically generated based on the name parameter |
    {{managementResourceGroupLocation}} | Location where the resources will be deployed |
    {{vmResourceGroup}} | Resource group where the VM resources will be deployed. If left empty, it is automatically generated based on the name parameter |
    {{vmResourceGroupLocation}} | Location where the VM will be deployed |
    {{botAppName}} | Name for the App registration that will be created for the Azure bot. If left empty, it is automatically generated based on the name parameter |
    {{botServiceApiAppName}} | Name for the App registration that will be created for the Bot Service. If left empty, it is automatically generated based on the name parameter |
    {{botServiceClientAppName}} | Name for the App registration that will be created for the Bot Service Client. If left empty, it is automatically generated based on the name parameter |
    {{managementApiAppName}} | Name for the App registration that will be created for Management API. If left empty, it is automatically generated based on the name parameter |
    {{sdkAppName}} |  Name for the App registration that will be created for Azure SDK. If left empty, it is automatically generated based on the name parameter |
    {{dnsSubscriptionId}} | Subscription Id where the DNS Zone is created |
    {{dnsResourceGroup}} | Resource group where the DNS Zone is created |
    {{zoneName}} | DNS Zone name |
    {{dnsRecordName}} | DNS record name that will be created for the VM |
    {{localCertPath}} | Path where a .Zip file with the pem certificates are located. Used for nginx. Should not contain folders inside  |
    {{pfxCertificatePath}} | Path of the .pfx certificate in the local machine. Should not be compressed. |
    {{pfxCertificatePassword}} | Password of the pfx certificate |
    {{pfxCertificateThumbprint}} | Thumbprint of the certificate |
    {{vmUserName}} | Username for the VM |
    {{vmPassword}} | Password for the VM |
    {{botDisplayName}} | Display name for the Azure bot. Should be unique in Azure. |
    {{botHandle}} | Bot Handle of the Azure Bot, should be unique in Azure. |

2. Open a powershell console in the `deployment` path of the solution and [sign in](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli) using `az login`. Select the subscription where you want to deploy the resources.

3. Run the Deploy-BDK.ps1 script

4. Grant admin consent for the `API permitions` of the Azure Bot and Bot Service Client app registrations

## If you left the dns fields empty, you should update the following settings

1. Configure a domain name referencing to the virtual machine IP. eg: `sandbox.domain.co`
2. Go to the Cosmos DB created after running the scripts, and update the `Infrastructure.Dns` property of the document created in the Service container, with the domain name assigned to the virtual machine.
3. Login into the VM, open the `c:/BotService/appSettings.json` file and upload the following properties with the domain name assigned to the virtual machine.

* `HttpServer.Endpoints.Https.Host`
* `Settings.BotConfiguration.ServiceDnsName`
* `Settings.BotConfiguration.ServiceCname`
* `Settings.BotConfiguration.ServiceFqdn`

4. Restart the VM
