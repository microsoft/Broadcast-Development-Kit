@description('Location for all resources.')
param location string = resourceGroup().location

@description('The base name of the web app.')
param serviceName string

@description('Resource group where functions are deployed')
param functionsResourceGroup string

@description('Functions resource name')
param functionsResourceName string

param systemTopics_avidpinwheeleventgridtopic_name string = '${serviceName}eventgridtopic'
param topics_avidpinwheeleventgridtopic_name string = '${serviceName}eventgridtopic'
param eventsubscription_name string = '${serviceName}entsuscription'
param sites_avidpinwheelfn_externalid string = '${subscription().id}/resourceGroups/${functionsResourceGroup}/providers/Microsoft.Web/sites/${functionsResourceName}'

resource systemTopics_avidpinwheeleventgridtopic_resource 'Microsoft.EventGrid/systemTopics@2022-06-15' = {
  name: systemTopics_avidpinwheeleventgridtopic_name
  location: 'global'
  properties: {
    source: '${subscription().id}/resourceGroups/${resourceGroup().name}'
    topicType: 'Microsoft.Resources.ResourceGroups'
  }
}

resource topics_avidpinwheeleventgridtopic_resource 'Microsoft.EventGrid/topics@2022-06-15' = {
  name: topics_avidpinwheeleventgridtopic_name
  location: location
  identity: {
    type: 'None'
  }
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    dataResidencyBoundary: 'WithinGeopair'
  }
}

resource systemTopics_avidpinwheeleventgridtopic_avidpinwheeleventsuscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2022-06-15' = {
  parent: systemTopics_avidpinwheeleventgridtopic_resource
  name: eventsubscription_name
  properties: {
    destination: {
      properties: {
        resourceId: '${sites_avidpinwheelfn_externalid}/functions/virtual-machine-event-grid-handler'
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
      }
      endpointType: 'AzureFunction'
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Resources.ResourceWriteSuccess'
        'Microsoft.Resources.ResourceWriteFailure'
        'Microsoft.Resources.ResourceWriteCancel'
        'Microsoft.Resources.ResourceDeleteSuccess'
        'Microsoft.Resources.ResourceDeleteFailure'
        'Microsoft.Resources.ResourceDeleteCancel'
        'Microsoft.Resources.ResourceActionSuccess'
        'Microsoft.Resources.ResourceActionFailure'
        'Microsoft.Resources.ResourceActionCancel'
      ]
      enableAdvancedFilteringOnArrays: true
    }
    labels: []
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
}
