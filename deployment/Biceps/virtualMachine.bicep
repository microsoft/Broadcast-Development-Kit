@description('Location for all resources.')
param location string = resourceGroup().location

@description('TenantID.')
param tenantId string = subscription().tenantId

@description('The base name of the web app.')
param serviceName string

@description('VM admin username')
param vmAdminUsername string

@secure()
@description('VM admin password')
param vmAdminPassword string

@description('SDK Client ID')
param sdkAppId string

@secure()
@description('SDK Client Secret')
param sdkClientSecret string

@description('Enable VM autoshutdown')
param autoShutdown string = 'Disabled'

@description('Gstreamer installer URL')
param gstreamerInstallerUri string = 'Disabled'

@description('Gstreamer path')
param gStreamerInstallationPath string = 'Disabled'

param vm_name string = '${serviceName}vm'
param networkInterfaces_vm_name string = '${vm_name}Network'
param publicIPAddresses_vm_ip_name string = '${vm_name}-ip'
param networkSecurityGroups_vm_nsg_name string = '${vm_name}-nsg'
param virtualNetworks_vnet_name string = '${vm_name}-vnet'
param schedules_shutdown_vm_name string = 'shutdown-computevm-${vm_name}'
param vmAccessAgentName string = '${serviceName}Agent'
param vmGstreamerScriptName string = '${serviceName}GScript'

resource networkSecurityGroups_vm_nsg 'Microsoft.Network/networkSecurityGroups@2020-11-01' = {
  name: networkSecurityGroups_vm_nsg_name
  location: location
  properties: {
    securityRules: [
      {
        name: 'RDP'
        properties: {
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '3389'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 300
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'HTTPS'
        properties: {
          description: 'Allows communication from the main API.'
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '443'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 310
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'SRT'
        properties: {
          description: 'Used for SRT protocol for media extraction & injection.'
          protocol: 'UDP'
          sourcePortRange: '*'
          destinationPortRange: '8880-9000'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 320
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'MediaPlatform'
        properties: {
          protocol: 'TCP'
          sourcePortRange: '*'
          destinationPortRange: '8445'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 330
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: []
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'RTMP'
        properties: {
          description: 'Used to inject & extract RTMP content.'
          protocol: 'TCP'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 340
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: [
            '1935-1936'
            '1940-1949'
          ]
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
      {
        name: 'RTMPS'
        properties: {
          description: 'Used to inject & extract RTMPS content.'
          protocol: 'TCP'
          sourcePortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Allow'
          priority: 350
          direction: 'Inbound'
          sourcePortRanges: []
          destinationPortRanges: [
            '2935-2936'
            '2940-2949'
          ]
          sourceAddressPrefixes: []
          destinationAddressPrefixes: []
        }
      }
    ]
  }
}

resource publicIPAddresses_vm_ip 'Microsoft.Network/publicIPAddresses@2020-11-01' = {
  name: publicIPAddresses_vm_ip_name
  location: location
  sku: {
    name: 'Basic'
    tier: 'Regional'
  }
  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
  }
}

resource virtualNetworks_mediapinwheelvm_vnet 'Microsoft.Network/virtualNetworks@2020-11-01' = {
  name: virtualNetworks_vnet_name
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.3.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.3.0.0/24'
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}

resource virtualMachines_vm 'Microsoft.Compute/virtualMachines@2022-03-01' = {
  name: vm_name
  location: location
  identity:{
    type: 'SystemAssigned'
  }
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_F8s_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: '20h2-pro-g2'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: '${vm_name}Disk'
        caching: 'ReadWrite'
        createOption: 'FromImage'
        managedDisk: {
            storageAccountType: 'StandardSSD_LRS'
        }
    }
      dataDisks: []
    }
    osProfile: {
      computerName: vm_name
      adminUsername: vmAdminUsername
      adminPassword: vmAdminPassword
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        patchSettings: {
          patchMode: 'AutomaticByOS'
          assessmentMode: 'ImageDefault'
          enableHotpatching: false
        }
      }
      secrets: []
      allowExtensionOperations: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: networkInterfaces_vm563.id
          properties: {
            deleteOption: 'Detach'
          }
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
    licenseType: 'Windows_Client'
  }
}

resource schedules_shutdown_computevm_vm 'microsoft.devtestlab/schedules@2018-09-15' = {
  name: schedules_shutdown_vm_name
  location: location
  properties: {
    status: autoShutdown
    taskType: 'ComputeVmShutdownTask'
    dailyRecurrence: {
      time: '1900'
    }
    timeZoneId: 'Argentina Standard Time'
    notificationSettings: {
      status: 'Disabled'
      timeInMinutes: 30
      notificationLocale: 'en'
    }
    targetResourceId: virtualMachines_vm.id
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_HTTPS 'Microsoft.Network/networkSecurityGroups/securityRules@2020-11-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'HTTPS'
  properties: {
    description: 'Allows communication from the main API.'
    protocol: 'TCP'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 310
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_MediaPlatform 'Microsoft.Network/networkSecurityGroups/securityRules@2020-11-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'MediaPlatform'
  properties: {
    protocol: 'TCP'
    sourcePortRange: '*'
    destinationPortRange: '8445'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 330
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_RDP 'Microsoft.Network/networkSecurityGroups/securityRules@2020-11-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'RDP'
  properties: {
    protocol: 'TCP'
    sourcePortRange: '*'
    destinationPortRange: '3389'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 300
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_SRT 'Microsoft.Network/networkSecurityGroups/securityRules@2020-11-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'SRT'
  properties: {
    description: 'Used for SRT protocol for media extraction & injection.'
    protocol: 'UDP'
    sourcePortRange: '*'
    destinationPortRange: '8880-9000'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 320
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_RTMP 'Microsoft.Network/networkSecurityGroups/securityRules@2022-01-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'RTMP'
  properties: {
    description: 'Used to inject & extract RTMP content.'
    protocol: 'TCP'
    sourcePortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 340
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '1935-1936'
      '1940-1949'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource networkSecurityGroups_avidpinwheelvm_nsg_RTMPS 'Microsoft.Network/networkSecurityGroups/securityRules@2022-01-01' = {
  parent: networkSecurityGroups_vm_nsg
  name: 'RTMPS'
  properties: {
    description: 'Used to inject & extract RTMPS content.'
    protocol: 'TCP'
    sourcePortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 350
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '2935-2936'
      '2940-2949'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
}

resource virtualNetworks_mediapinwheelvm_vnet_default 'Microsoft.Network/virtualNetworks/subnets@2020-11-01' = {
  parent: virtualNetworks_mediapinwheelvm_vnet
  name: 'default'
  properties: {
    addressPrefix: '10.3.0.0/24'
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
}

resource networkInterfaces_vm563 'Microsoft.Network/networkInterfaces@2020-11-01' = {
  name: networkInterfaces_vm_name
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          privateIPAddress: '10.3.0.4'
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIPAddresses_vm_ip.id
          }
          subnet: {
            id: virtualNetworks_mediapinwheelvm_vnet_default.id
          }
          primary: true
          privateIPAddressVersion: 'IPv4'
        }
      }
    ]
    dnsSettings: {
      dnsServers: []
    }
    enableAcceleratedNetworking: true
    enableIPForwarding: false
    networkSecurityGroup: {
      id: networkSecurityGroups_vm_nsg.id
    }
  }
}

resource vm_name_enablevmaccess 'Microsoft.Compute/virtualMachines/extensions@2022-03-01' = {
  parent: virtualMachines_vm
  name: vmAccessAgentName
  location: location
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.Compute'
    type: 'VMAccessAgent'
    typeHandlerVersion: '2.0'
    settings: {
    }
    protectedSettings: {
    }
  }
}

resource vm_script_install_gstreamer 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  dependsOn: [virtualMachines_vm, vm_name_enablevmaccess]
  name: vmGstreamerScriptName
  location: location
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.32.0'
    retentionInterval: 'P1D'
    scriptContent: 'az login --service-principal -u ${sdkAppId} -p ${sdkClientSecret} --tenant ${tenantId}; az vm run-command invoke --command-id RunPowerShellScript --name ${vm_name} -g ${resourceGroup().name} --scripts  \'wget ${gstreamerInstallerUri} -OutFile c:\\gstreamer.msi;Start-Process -Wait -FilePath "c:\\gstreamer.msi" -ArgumentList "/qn INSTALLLEVEL=1000"; [Environment]::SetEnvironmentVariable("PATH", $env:PATH + ";${gStreamerInstallationPath}", "Machine")\''
  }
}

output vmName string = vm_name
output vmIpAdress string = publicIPAddresses_vm_ip.properties.ipAddress
output vmIdentity string = virtualMachines_vm.identity.principalId
