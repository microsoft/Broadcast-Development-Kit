# Virtual Machine

## Getting Started

This document explains how to create the virtual machine where the Bot Service API is going to be hosted, and how to configure it.

## Dependencies

To continue with the Virtual Machine documentation, the following dependencies need to be created:

- [Storage Account](storage_account.md).
- [SSL Certificate](../prerequisites/README.md).

## Create the virtual machine in Azure

To create the Virtual Machine, check the following document [Create a Windows Virtual Machine in the Azure Portal](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/quick-create-portal).

While creating the virtual machine, consider the following settings:

- **_Subscription:_** The azure subscription where you want to create the VM.
- **_Resource Group:_** The resource group where you want to create the VM. We recommend creating a specific resource group for the VM, so you can easily identify the VM resources in case of resource deletion.
- **_Virtual Machine Name:_** A meaningful name for the VM.
- **_Image:_** Windows 10 Pro, Version 20H2.
- **_Size:_** Please refer to the [Virtual Machine size](#virtual-machine-size) section below.
- **_Username:_** A meaningful username.
- **_Password:_** A meaningful password.

### Virtual Machine size

The computational power of the VM must be chosen based on the number of simultaneous streams required. The following table shows some of the Azure VM options where the BDK has been tested.

| Virtual Machine size                | Number of simultaneous active streams | Percentage of CPU usage | Notes                                                               |
| ----------------------------------- | :-----------------------------------: | :---------------------: | ------------------------------------------------------------------- |
| F4s_v2 (4 vCPUs and 8 GB of RAM)    |                Up to 2                |           95%           | Not recommended for production workloads. Only for testing purpose. |
| F8s_v2 (8 vCPUs and 16 GB of RAM)   |                Up to 4                |           70%           | 4 Stream extractions or 3 Stream extractions + 1 stream Injection.  |
| F16s_v2 (16 vCPUs and 32 GB of RAM) |                Up to 7                |           70%           | 7 Stream extractions or 6 Stream extractions + 1 stream Injection.  |
| F32s_v2 (32 vCPUs and 32 GB of RAM) |               Up to 10                |           50%           | 9 Stream extractions + 1 stream injection                           |

The results shown in the table above are for reference only. Please take into account the following considerations:

- Stream processing (decoding, normalization, and encoding) is performed by the CPU. It is not recommended to exceed the CPU workload above 70% to guarantee the streams quality.
- The results may vary based on the processing required on each stream to decode, normalize, and encode them. For example, streams with fewer variations in image changes tend to require less CPU consumption in the encoding process.

> **NOTE**: The maximum number of streams that can be simultaneously extracted by the bot can be limited by setting the `NumberOfMultiviewSockets` property in the [Bot Service app settings file](storage_account.md#environment-json-file-settings-example). E.g. If you want to limit the maximum number of extractions to N, just set this property to N - 1.

### Network Security Group inbound rules

Once the virtual machine is created, we must add inbound rules in the network security group.

**Inbound rules**

| Name          | Port                 | Protocol | Purpose                                                                 |
| ------------- | -------------------- | -------- | ----------------------------------------------------------------------- |
| SRT           | 8880-9000            | UDP      | Used for SRT protocol for media extraction & injection.                 |
| HTTPS         | 443                  | TCP      | Allows communication from the main API.                                 |
| MediaPlatform | 8445                 | TCP      | Used to establish communication between the bot and the media platform. |
| RTMP          | 1935-1936, 1940-1949 | TCP      | Used to inject & extract RTMP content.                                  |
| RTMPS         | 2935-2936, 2940-2949 | TCP      | Used to inject & extract RTMPS content.                                 |

## Configure the virtual machine

Before starting using the virtual machine, we must install the applications listed below.

> **IMPORTANT**: The disk D:\ is a temporary disk (files are deleted after shutdown/restart of the virtual machine) so you must install all the applications in C:\.

### SSL Certificate

Your wildcard SSL certificate can be installed in the virtual machine using one of these approaches:

1. **Install the SSL certificate manually in the VM.** The main advantage of this is that you won't need to upload your PFX and password to the storage account, but you will need to change the certificate manually once it expires. To use this approach check the instructions in [Manual installation of your domain certificate](../common/install_domain_certificate.md).

2. **Automatically through the Bot Service.** The main advantage is that the Bot Service will take care of installing the latest certificate as long as it is available in the storage account. The downside is that you will need to upload the certificate and password to the storage account. To delegate the installation to the Bot Service, check the instructions in [Storage Account](storage_account.md).

### Gstreamer

Download the GStreamer installer from this [link](https://gstreamer.freedesktop.org/data/pkg/windows/1.18.4/mingw/gstreamer-1.0-mingw-x86_64-1.18.4.msi). Once you have downloaded the installer and started the installation process, choose the custom installation and make sure that all modules have been selected and the installation path is in C:\.

> **IMPORTANT**: Remember to select all GStreamer modules/plugins while installing GStreamer as a custom installation.

After GStreamer installation, add the GStreamer bin folder path to the path environment variable.

### VCRedist

Download [VCRedist](https://aka.ms/vs/16/release/vc_redist.x64.exe) and install it.

### NGINX

Follow this guide [How to Install and configure NGINX with RTMP module on Windows](../common/install_and_configure_nginx_with_rtmp_module_on_windows.md) to install and configure NGINX with RTMP module on windows, and configure it as a Windows service.

### Environment variables

In order to run the bot, we need to configure some environment variables that the bot will read in order to get access to its configuration settings and certificate.

> **IMPORTANT** Before performing these steps, the storage account with the bot configurations must be already created to set the environment's variables.

![Set Environment Variables](./images/set_environment_variables.png)

![Set Systema Variables](./images/set_system_variables.png)

| **Placer**            | **Description**                                                                                                      |
| --------------------- | -------------------------------------------------------------------------------------------------------------------- |
| STORAGE_ACCOUNT       | Name of the [Storage account](app_registrations.md) where the files are being stored.                                |
| BLOB_CONTAINER        | Name the container of [Storage account](app_registrations.md).                                                       |
| BLOB_SAS_QUERY        | SAS key to get access to the container files of [Storage account](app_registrations.md).                             |
| APP_SETTING_FILE_NAME | Name of the bot app settings file uploaded into the config container in the [Storage account](app_registrations.md). |
| CERTIFICATE_NAME      | Name of `.pfx` [wilcard certificate](../prerequisites/README.md) for the domain.                                     |

> **NOTE**: The `BLOB_SAS_QUERY` must include the '?' at the beginning. This token has an expiration date, be aware of this date to renew the access token.

## Bot Service

For instructions on how to deploy the Bot Service to the virtual machine you created check the [Deploy the Bot Service](deploy_bot_service.md) document.

[← Back to How to Run the Solution in Azure](README.md#how-to-run-the-solution-in-azure)
