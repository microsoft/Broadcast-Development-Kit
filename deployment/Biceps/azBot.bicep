param botDisplayName string
param botHandle string
param botAppRegitrationId string
param botResourceEndpoint string = 'https://test.com'

resource azBot 'Microsoft.BotService/botServices@2021-05-01-preview' = {
  name: botHandle
  location: 'global'
  sku:{
   name:'F0'
  }
  kind: 'azurebot'
  properties:{
    displayName: botDisplayName
    iconUrl: 'https://docs.botframework.com/static/devportal/client/images/bot-framework-default.png'
    msaAppId: botAppRegitrationId
    msaAppType: 'MultiTenant'
    luisAppIds: []
    isStreamingSupported: false
    schemaTransformationVersion: '1.3'
    isCmekEnabled: false
    disableLocalAuth: false
    endpoint: botResourceEndpoint
  }
}

resource msTeams_Channel 'Microsoft.BotService/botServices/channels@2021-05-01-preview' = {
  parent: azBot
  name: 'MsTeamsChannel'
  location: 'global'
  properties: {
    properties: {
      enableCalling: true
      incomingCallRoute: 'graphPma'
      isEnabled: true
      deploymentEnvironment: 'CommercialDeployment'
      acceptedTerms: true
    }
    channelName: 'MsTeamsChannel'
    location: 'global'
  }
}

resource msWeb_Chat_Channel 'Microsoft.BotService/botServices/channels@2021-05-01-preview' = {
  parent: azBot
  name: 'WebChatChannel'
  location: 'global'
  properties: {
    properties: {
      sites: [
        {
          siteName: 'Default Site'
          isEnabled: true
          isWebchatPreviewEnabled: true
        }
      ]
    }
    channelName: 'WebChatChannel'
    location: 'global'
  }
}
