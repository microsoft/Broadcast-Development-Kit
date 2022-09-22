@description('Location for all resources.')
param location string = resourceGroup().location

@description('The base name of the web app.')
param serviceName string

@description('The Object Id of the user that is currently signed in by the azure cli.')
param userObjectId string

@description('The Tenant Id of the user that is currently signed in by the azure cli.')
param userTenantId string

@description('The ASP.NET Core environment that will be set for API web apps (via ASPNETCORE_ENVIRONMENT environment parameter).')
param aspnetcoreEnvironment string = 'Production'

@description('The provisioned IOPs for the main environment CosmosDB database.')
param cosmosDBProvisionedThroughput int = 400

@description('The environment type.')
@allowed([
  'dev'
  'qa'
  'prd'
])
param envType string = 'prd'

@description('App service hosting plan SKU type for Web apps.')
@allowed([
  'shared'
  'standard'
])
param webAppServicePlanSKU string = 'shared'

@description('App service hosting plan SKU type for Azure Functions.')
@allowed([
  'shared'
  'standard'
])
param functionAppServicePlanSKU string = 'shared'

@description('Tenant id of SP required for graph client.')
param graphClientTenantId string = ''

@description('Client id of SP required for graph client.')
param graphClientClientId string = ''

@description('Client id of SP required to start/stop the vm.')
param azServicePrincipalClientId string = ''

@description('Subscription id of SP required to start/stop the vm.')
param azServicePrincipalSubscriptionId string 

@description('Tenant id of SP required to start/stop the vm.')
@secure()
param azServicePrincipalTenantId string = ''

@description('Auth API app registration client id')
param authApiClientId string = ''

@description('Auth instance')
param authInstance string = 'https://login.microsoftonline.com/'

@description('Auth tenant id')
param authTenantId string = subscription().tenantId

@description('Auth aad producers group id')
param authGroupId string = ''

@description('Auth BotService API registration client id')
param authBotServiceApiClientId string = ''

@description('Auth BotService Client app registration client id')
param authBotServiceClientClientId string = ''

@secure()
@description('Client secret of the Azure SDK Service app registration')
param azServicePrincipalClientSecret string = ''

@secure()
@description('The client secret of the BotService app registration')
param authBotServiceClientClientSecret string = ''

@secure()
@description('client secret of the Azure Bot app registration.')
param graphClientClientSecret string = ''

var apiAppName = '${serviceName}-api'
var functionAppName = '${serviceName}-function'
var appServicePlanSKULookup = {
  shared: {
    name: 'B1'
    tier: 'Shared'
    size: 'B1'
    family: 'B'
    capacity: 0
  }
  standard: {
    name: 'S1'
    tier: 'Standard'
    size: 'S1'
    family: 'S'
    capacity: 1
  }
}
var commonTags = {
  Service: serviceName
}

var cosmosAccountName = '${serviceName}-db'
var cosmosDatabaseName = 'broadcast-bot-database'
var insightsHiddenLinkTags = {
  'hidden-link:${resourceGroup().id}/providers/Microsoft.Web/sites/${apiAppName}': 'Resource'
}
var appInsightsName = '${serviceName}-insights'
var webHostingPlanName = '${serviceName}-web-plan'
var functionHostingPlanName = '${serviceName}-fn-plan'
var storageAccountName = replace(serviceName, '-', '')
var keyvaultName = '${serviceName}-keyvault'

resource keyvault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyvaultName
  location: location
  properties: {
    accessPolicies: []
    enableRbacAuthorization: false
    enableSoftDelete: false
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    tenantId: subscription().tenantId
    sku:{
      name: 'standard'
      family: 'A'
    }
    networkAcls:{
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2015-05-01' = {
  name: appInsightsName
  location: location
  tags: union(insightsHiddenLinkTags, commonTags)
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'IbizaWebAppExtensionCreate'
  }
}

resource webHostingPlan 'Microsoft.Web/serverfarms@2016-09-01' = {
  name: webHostingPlanName
  location: location
  tags: commonTags
  sku: appServicePlanSKULookup[webAppServicePlanSKU]
  kind: 'app'
  properties: {
    name: webHostingPlanName
    perSiteScaling: false
    reserved: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

resource functionHostingPlan 'Microsoft.Web/serverfarms@2016-09-01' = {
  name: functionHostingPlanName
  location: location
  tags: commonTags
  sku: appServicePlanSKULookup[functionAppServicePlanSKU]
  kind: 'functionapp'
  properties: {
    name: functionHostingPlanName
    perSiteScaling: false
    reserved: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-04-01' = {
  name: storageAccountName
  location: location
  tags: commonTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2021-03-01-preview' = {
  name: cosmosAccountName
  location: location
  tags: commonTags
  kind: 'GlobalDocumentDB'
  identity: {
    type: 'None'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    disableKeyBasedMetadataWriteAccess: false
    enableFreeTier: false
    enableAnalyticalStorage: false
    createMode: 'Default'
    databaseAccountOfferType: 'Standard'
    networkAclBypass: 'None'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Strong'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    networkAclBypassResourceIds: []
  }
}

resource cosmosAccount_cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-03-01-preview' = {
  parent: cosmosAccount
  name: cosmosDatabaseName
  properties: {
    resource: {
      id: cosmosDatabaseName
    }
    options: {
      throughput: cosmosDBProvisionedThroughput
    }
  }
}

resource cosmosAccount_cosmosDatabase_Audit 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-03-01-preview' = {
  parent: cosmosAccount_cosmosDatabase
  name: 'Audit'
  properties: {
    resource: {
      id: 'Audit'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/EntityId'
        ]
        kind: 'Hash'
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource cosmosAccount_cosmosDatabase_Call 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-03-01-preview' = {
  parent: cosmosAccount_cosmosDatabase
  name: 'Call'
  properties: {
    resource: {
      id: 'Call'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource cosmosAccount_cosmosDatabase_ParticipantStream 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-03-01-preview' = {
  parent: cosmosAccount_cosmosDatabase
  name: 'ParticipantStream'
  properties: {
    resource: {
      id: 'ParticipantStream'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource cosmosAccount_cosmosDatabase_Service 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-03-01-preview' = {
  parent: cosmosAccount_cosmosDatabase
  name: 'Service'
  properties: {
    resource: {
      id: 'Service'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource cosmosAccount_cosmosDatabase_Stream 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-03-01-preview' = {
  parent: cosmosAccount_cosmosDatabase
  name: 'Stream'
  properties: {
    resource: {
      id: 'Stream'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

resource apiApp 'Microsoft.Web/sites@2018-11-01' = {
  name: apiAppName
  location: location
  tags: commonTags
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    clientAffinityEnabled: true
    enabled: true
    httpsOnly: true
    serverFarmId: webHostingPlan.id
    siteConfig: {}
  }
}

resource apiApp_appsettings 'Microsoft.Web/sites/config@2018-11-01' = {
  parent: apiApp
  name: 'appsettings'
  properties: {
    AllowedHosts: '*'
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(appInsights.id, '2015-05-01').InstrumentationKey
    APPINSIGHTS_PROFILERFEATURE_VERSION: 'disabled'
    APPINSIGHTS_SNAPSHOTFEATURE_VERSION: 'disabled'
    ApplicationInsightsAgent_EXTENSION_VERSION: '~2'
    ASPNETCORE_ENVIRONMENT: aspnetcoreEnvironment
    DiagnosticServices_EXTENSION_VERSION: 'disabled'
    InstrumentationEngine_EXTENSION_VERSION: 'disabled'
    'Logging:LogLevel:Default': 'Information'
    'Settings:CosmosDbConfiguration:EndpointUrl': reference(cosmosAccount.id, '2015-04-08').documentEndpoint
    'Settings:CosmosDbConfiguration:DatabaseName': cosmosDatabaseName
    'Settings:CosmosDbConfiguration:PrimaryKey': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--CosmosDbConfiguration--PrimaryKey)'
    'Settings:BuildVersion': '0.0.0'
    'Settings:StorageConfiguration:ConnectionString': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--StorageConfiguration--ConnectionString)'
    'Settings:GraphClientConfiguration:TenantId': graphClientTenantId
    'Settings:GraphClientConfiguration:ClientId': graphClientClientId
    'Settings:GraphClientConfiguration:ClientSecret': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--GraphClientConfiguration--ClientSecret)'
    'Settings:AzureAdConfiguration:ClientId': authApiClientId
    'Settings:AzureAdConfiguration:Instance': authInstance
    'Settings:AzureAdConfiguration:TenantId': authTenantId
    'Settings:AzureAdConfiguration:GroupId': authGroupId
    'Settings:AzServicePrincipalConfiguration:ApplicationClientId': azServicePrincipalClientId
    'Settings:AzServicePrincipalConfiguration:ApplicationClientSecret': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--AzServicePrincipalConfiguration--ApplicationClientSecret)'
    'Settings:AzServicePrincipalConfiguration:SubscriptionId': azServicePrincipalSubscriptionId
    'Settings:AzServicePrincipalConfiguration:TenantId': azServicePrincipalTenantId
    'Settings:BotServiceAuthenticationConfiguration:BotServiceApiClientId': authBotServiceApiClientId
    'Settings:BotServiceAuthenticationConfiguration:ClientId': authBotServiceClientClientId
    'Settings:BotServiceAuthenticationConfiguration:ClientSecret': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--BotServiceAuthenticationConfiguration--ClientSecret)'
    SnapshotDebugger_EXTENSION_VERSION: 'disabled'
    XDT_MicrosoftApplicationInsights_BaseExtensions: 'disabled'
    XDT_MicrosoftApplicationInsights_Mode: 'recommended'
  }
}

resource apiAppName_Microsoft_ApplicationInsights_AzureWebSites 'Microsoft.Web/sites/siteextensions@2018-11-01' = {
  parent: apiApp
  name: 'Microsoft.ApplicationInsights.AzureWebSites'
}

resource functionApp 'Microsoft.Web/sites@2018-11-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionHostingPlan.id
  }
}

resource functionAppName_appsettings 'Microsoft.Web/sites/config@2018-11-01' = {
  parent: functionApp
  name: 'appsettings'
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: reference(appInsights.id, '2015-05-01').InstrumentationKey
    APPINSIGHTS_PROFILERFEATURE_VERSION: 'disabled'
    APPINSIGHTS_SNAPSHOTFEATURE_VERSION: 'disabled'
    ApplicationInsightsAgent_EXTENSION_VERSION: '~2'
    AZURE_FUNCTIONS_ENVIRONMENT: 'Development'
    FUNCTIONS_EXTENSION_VERSION: '~3'
    AzureWebJobsStorage: '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--StorageConfiguration--ConnectionString)'
    BuildVersion: '0.0.0'
    Environment: envType
    'DevelopmentConfiguration:DefaultBotApiBaseUrl': 'localhost:9441'
    'CosmosDbConfiguration:EndpointUrl': reference(cosmosAccount.id, '2015-04-08').documentEndpoint
    'CosmosDbConfiguration:DatabaseName': cosmosDatabaseName
    'CosmosDbConfiguration:PrimaryKey': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--CosmosDbConfiguration--PrimaryKey)'
    'AzServicePrincipalConfiguration:ApplicationClientId': azServicePrincipalClientId
    'AzServicePrincipalConfiguration:ApplicationClientSecret': '@Microsoft.KeyVault(VaultName=${keyvaultName};SecretName=Settings--AzServicePrincipalConfiguration--ApplicationClientSecret)'
    'AzServicePrincipalConfiguration:SubscriptionId': azServicePrincipalSubscriptionId
    'AzServicePrincipalConfiguration:TenantId': azServicePrincipalTenantId
    'AzureAdConfiguration:Instance': authInstance
    'AzureAdConfiguration:TenantId': authTenantId
  }
}

resource keyVaultAccessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  name: 'add'
  parent: keyvault
  properties: {
    accessPolicies: [{
      objectId: userObjectId
      tenantId: userTenantId
      permissions: {
        secrets: [
          'all'
        ]
        certificates: [
          'all'
        ]
      }
    }
    {
      objectId: functionApp.identity.principalId
      tenantId: functionApp.identity.tenantId
      permissions: {
        secrets: [
          'get'
          'list'
        ]
        certificates: [
          'get'
          'list'
        ]
      }
    }
    {
      objectId: apiApp.identity.principalId
      tenantId: apiApp.identity.tenantId
      permissions: {
        secrets: [
          'get'
          'list'
        ]
        certificates: [
          'get'
          'list'
        ]
      }
    }
  ]
  }
}

resource cosmosKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--CosmosDbConfiguration--PrimaryKey'
  properties:{
    attributes: {
      enabled: true
    }
    value: cosmosAccount.listKeys().primaryMasterKey
  }
}

resource storageKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--StorageConfiguration--ConnectionString'
  properties:{
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys('${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageAccountName}', '2019-04-01').keys[0].value}'
  }
}

resource azServicePrincipalSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--AzServicePrincipalConfiguration--ApplicationClientSecret'
  properties:{
    attributes: {
      enabled: true
    }
    value: azServicePrincipalClientSecret
  }
}

resource botServiceClientSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--BotServiceAuthenticationConfiguration--ClientSecret'
  properties:{
    attributes: {
      enabled: true
    }
    value: authBotServiceClientClientSecret
  }
}

resource graphClientSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--GraphClientConfiguration--ClientSecret'
  properties:{
    attributes: {
      enabled: true
    }
    value: graphClientClientSecret
  }
}

resource botAadSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyvault
  name: 'Settings--BotConfiguration--AadAppSecret'
  properties:{
    attributes: {
      enabled: true
    }
    value: graphClientClientSecret
  }
}

output apiAppName string = apiAppName
output functionAppName string = functionAppName
output keyvaultName string = keyvaultName
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${listKeys('${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageAccountName}', '2019-04-01').keys[0].value}'
output appinsightInstrumentationKey string = appInsights.properties.InstrumentationKey
output managementApiUri string = apiApp.properties.defaultHostName
output cosmosDatabase string = cosmosDatabaseName
output cosmosEndpoint string = reference(cosmosAccount.id, '2015-04-08').documentEndpoint
output cosmosConnectionString string = 'AccountEndpoint=${reference(cosmosAccount.id, '2015-04-08').documentEndpoint};AccountKey=${cosmosAccount.listKeys().primaryMasterKey};'

