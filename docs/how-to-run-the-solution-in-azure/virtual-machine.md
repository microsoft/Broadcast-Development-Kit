# Virtual Machine

This document explains how to create the virtual machine where the Bot Service API is going to be hosted, and how to configure it.

## Create the virtual machine in Azure

To create the Virtual Machine, check the following document [Create a Windows Virtual Machine in the Azure Portal](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/quick-create-portal).

While creating the virtual machine, consider the following settings:

- ***Subscription:*** The azure subscription where you want to create the VM.
- ***Resource Group:*** The resource group where you want to create the VM. We recommend creating a specific resource group for the VM, so you can easily identify the VM resources in case of resource deletion.
- ***Virtual Machine Name:*** A meaningful name for the VM.
- ***Image:*** Windows 10 Pro, Version 20H2.
- ***Size:*** Please refer to the [Virtual Machine size](#virtual-machine-size) section below.
- ***Username:*** A meaningful username.
- ***Password:*** A meaningful password.

### Virtual Machine size

The computational power of the VM must be chosen based on the number of simultaneous streams required. The following table shows some of the Azure VM options where the BDK has been tested.

| Virtual Machine size                | Number of simultaneous active streams | Percentage of CPU usage | Notes                          |
|-------------------------------------|:-------------------------------------:|:-----------------------:|--------------------------------|
| F4s_v2 (4 vCPUs and 8 GB of RAM)    | Up to 2                               | 95%                     | Not recommended for production workloads. Only for testing purpose. |
| F8s_v2 (8 vCPUs and 16 GB of RAM)   | Up to 4                               | 70%                     | 4 Stream extractions or 3 Stream extractions + 1 stream Injection.  |
| F16s_v2 (16 vCPUs and 32 GB of RAM) | Up to 7                               | 70%                     | 7 Stream extractions or 6 Stream extractions + 1 stream Injection.  |
| F32s_v2 (32 vCPUs and 32 GB of RAM) | Up to 10                              | 50%                     | 9 Stream extractions + 1 stream injection                           |

The results shown in the table above are for reference only. Please take into account the following considerations:

- Stream processing (decoding, normalization, and encoding) is performed by the CPU. It is not recommended to exceed the CPU workload above 70% to guarantee the streams quality.
- The results may vary based on the processing required on each stream to decode, normalize, and encode them. For example, streams with fewer variations in image changes tend to require less CPU consumption in the encoding process.

### Network Security Group inbound rules

Once the virtual machine is created, we must add inbound rules in the network security group, and then change the private IP address to static.

#### Inbound rules

| Name            | Port      | Protocol | Purpose                                                                 |
|-----------------|-----------|----------|-------------------------------------------------------------------------|
| SRT             | 8880-9000 | UDP      | Used for SRT protocol for media extraction & injection.                 |
| HTTPS           | 443       | TCP      | Allows communication from the main API.                                 |
| MediaPlatform   | 8445      | TCP      | Used to establish communication between the bot and the media platform. |
| RTMP            | 1935-1936, 1940-1949 | TCP      | Used to inject & extract RTMP content.                       |
| RTMPS           | 2935-2936, 2940-2949 | TCP      | Used to inject & extract RTMPS content.                      |

#### Change private IP address to static

To change the private IP address to static, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/virtual-network/ip-services/virtual-networks-static-private-ip-arm-pportal#change-private-ip-address-to-static).

## Configure the virtual machine

Before starting using the virtual machine, we must install the applications listed below.

> **IMPORTANT**: The disk D:\ is a temporary disk (files are deleted after shutdown/restart of the virtual machine) so you must install all the applications in C:\.

### SSL Certificate

Your wildcard SSL certificate can be installed in the virtual machine using one of these approaches:

1. **Automatically through the Bot Service.** If you have uploaded the SSL certificate to the Azure Key Vault after creating the key vault ([Azure Key Vault](azure-key-vault.md)), the Bot Service will take care of installing the latest certificate. If you haven't uploaded it, consider uploading it or following the approach number two.

2. **Install the SSL certificate manually in the VM.** The main advantage of this is that you won't need to upload your PFX and password to the Azure Key Vault, but you will need to change the certificate manually once it expires. To use this approach check the instructions in [Manual installation of your domain certificate](../common/install-domain-certificate.md).

### GStreamer

Download the GStreamer installer from this [link](https://gstreamer.freedesktop.org/data/pkg/windows/1.18.6/mingw/gstreamer-1.0-mingw-x86_64-1.18.6.msi). Once you have downloaded the installer and started the installation process, choose the custom installation and make sure that all modules have been selected and the installation path is in C:\.

> **IMPORTANT**: Remember to select all GStreamer modules/plugins while installing GStreamer as a custom installation.

After GStreamer installation, add the GStreamer bin folder path to the path environment variable.

### VCRedist

Download [VCRedist](https://aka.ms/vs/16/release/vc_redist.x64.exe) and install it.

### NGINX

Follow this guide [How to Install and configure NGINX with RTMP module on Windows](../common/install-and-configure-nginx-with-rtmp-module-on-windows.md) to install and configure NGINX with RTMP module on windows, and configure it as a Windows service.

## Enable Managed Identity

To allow to the virtual machine to get access to key vault, you have to enable a system assigned managed identity. To do so, please review the following [Microsoft Documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/qs-configure-portal-windows-vm#enable-system-assigned-managed-identity-on-an-existing-vm), and take note of the **Object (principal) ID**, you will need it in future steps to configure the Azure Key Vault.

[← Back to How to run the solution in Azure](README.md#provision-azure-resources) | [Next: Azure Key Vault →](azure-key-vault.md#azure-key-vault)
