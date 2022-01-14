@description('Github repository.')
param repositoryUrl string

@description('GitHub branch.')
param branch string

@description('GitHub repository token.')
@secure()
param repositoryToken string

@description('Azure AD ClientId of API application.')
param clientId string

@description('Azure AD API Applications audience.')
param applicationIdURI string

@allowed([
  'Free'
  'Standard'
])
param staticWebAppSku string = 'Standard'

@allowed([
  'Standard_LRS'
  'Standard_ZRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Premium_LRS'
])
param storageType string = 'Standard_LRS'

@description('Custom domain assigned to the CDN')
param customDomain string

@description('Alert email address.')
param alertEmailAddress string

@description('SignalR Service Pricing tier. Check details at https://azure.microsoft.com/en-us/pricing/details/signalr-service/')
@allowed([
  'Free_F1'
  'Standard_S1'
])
param signalRServicePricingTier string

@description('SignalR Service unit count')
@minValue(1)
@allowed([
  1
  2
  5
  10
  20
  50
  100
])
param signalRServiceUnits int

@description('WebPush Public Key. Used in sending web notifications.')
param webPushPublicKey string

@description('WebPush Private Key. Used in sending web notifications.')
param webPushPrivateKey string

@description('Location for all resources.')
param location string = resourceGroup().location

var appName = 'mychess'
var appInsightsName = 'ai-${appName}'
var appSignalRName = 'mychess${uniqueString(resourceGroup().id)}'
var storageName = 'mychess${uniqueString(resourceGroup().id)}'
var appAlertActionGroupName = '${appName}ActionGroup'
var appInsightsExceptionQueryName = '${appName}AppInsightsExceptionQuery'
var customDomainUri = 'https://${customDomain}'

resource storageResource 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageName
  location: location
  sku: {
    name: storageType
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
  }
}

resource appInsightsResource 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource staticWebAppResource 'Microsoft.Web/staticSites@2021-02-01' = {
  name: appName
  location: location
  properties: {
    repositoryUrl: repositoryUrl
    branch: branch
    repositoryToken: repositoryToken
    buildProperties: {
      appLocation: 'src/MyChess.Client'
      apiLocation: 'src/MyChess.Functions'
    }
  }
  sku: {
    name: staticWebAppSku
    tier: staticWebAppSku
  }
}

resource appSettingsResource 'Microsoft.Web/staticSites/config@2021-01-15' = {
  parent: staticWebAppResource
  name: 'appsettings'
  properties: {
    AzureSignalRConnectionString: appSignalRResource.listKeys().primaryConnectionString
    Storage: 'DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${storageResource.listKeys().keys[0].value}'
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsightsResource.properties.InstrumentationKey
    'WebPush:PublicServerUri': customDomainUri
    'WebPush:PublicKey': webPushPublicKey
    'WebPush:PrivateKey': webPushPrivateKey
    'AzureAD:ClientId': clientId
    'AzureAD:Audience': applicationIdURI
  }
}

resource appSignalRResource 'Microsoft.SignalRService/signalR@2021-10-01' = {
  name: appSignalRName
  location: location
  sku: {
    name: signalRServicePricingTier
    capacity: signalRServiceUnits
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
    ]
  }
}

resource actionGroupResource 'microsoft.insights/actionGroups@2019-06-01' = {
  name: appAlertActionGroupName
  location: 'Global'
  properties: {
    groupShortName: 'webAppAG'
    enabled: true
    emailReceivers: [
      {
        name: 'notify by email'
        emailAddress: alertEmailAddress
        useCommonAlertSchema: true
      }
    ]
    smsReceivers: []
    webhookReceivers: []
    itsmReceivers: []
    azureAppPushReceivers: [
      {
        name: 'notify by app'
        emailAddress: alertEmailAddress
      }
    ]
    automationRunbookReceivers: []
    voiceReceivers: []
    logicAppReceivers: []
    azureFunctionReceivers: []
    armRoleReceivers: [
      {
        name: 'owner'
        roleId: '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
        useCommonAlertSchema: true
      }
    ]
  }
}

resource appInsightsExceptionQueryResource 'microsoft.insights/scheduledqueryrules@2018-04-16' = {
  name: appInsightsExceptionQueryName
  location: location
  properties: {
    description: 'Scheduled query to find exceptions occurred in the app in last 5 minutes.'
    enabled: 'true'
    source: {
      query: 'exceptions'
      authorizedResources: []
      dataSourceId: appInsightsResource.id
      queryType: 'ResultCount'
    }
    schedule: {
      frequencyInMinutes: 5
      timeWindowInMinutes: 5
    }
    action: {
      'odata.type': 'Microsoft.WindowsAzure.Management.Monitoring.Alerts.Models.Microsoft.AppInsights.Nexus.DataContracts.Resources.ScheduledQueryRules.AlertingAction'
      severity: '2'
      trigger: {
        thresholdOperator: 'GreaterThan'
        threshold: 0
      }
      aznsAction: {
        actionGroup: [
          actionGroupResource.id
        ]
      }
    }
  }
}

output instrumentationKey string = appInsightsResource.properties.InstrumentationKey
output cdnCustomDomainUri string = customDomainUri
