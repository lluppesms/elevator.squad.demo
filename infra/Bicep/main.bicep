// --------------------------------------------------------------------------------
// Main Bicep file that creates web deployment infrastructure for one environment
// --------------------------------------------------------------------------------
param appName string = ''
param environmentCode string = 'dev'
param location string = resourceGroup().location
param instanceNumber string = '1'

@description('Deployment type. This repo currently deploys webapp resources.')
param deploymentType string = 'webapp'

param servicePlanName string = ''
param servicePlanResourceGroupName string = ''
param webAppKind string = 'linux' // 'linux' or 'windows'
param webSiteSku string = 'B1'

// calculated variables disguised as parameters
param runDateTime string = utcNow()

// --------------------------------------------------------------------------------
var deploymentSuffix = '-${runDateTime}'
var commonTags = {
  LastDeployed: runDateTime
  Application: appName
  Environment: environmentCode
}
var resourceGroupName = resourceGroup().name
var deploymentTypeNormalized = toLower(deploymentType)
var deployWebAppEffective = contains([
  'webapp'
  'appservice'
  'all'
], deploymentTypeNormalized)

// --------------------------------------------------------------------------------
module resourceNames 'resourcenames.bicep' = {
  name: 'resourcenames${deploymentSuffix}'
  params: {
    appName: appName
    environmentCode: environmentCode
    instanceNumber: instanceNumber
  }
}

module logAnalyticsWorkspaceModule './modules/monitor/loganalyticsworkspace.bicep' = if (deployWebAppEffective) {
  name: 'logAnalytics${deploymentSuffix}'
  params: {
    logAnalyticsWorkspaceName: resourceNames.outputs.logAnalyticsWorkspaceName
    location: location
    commonTags: commonTags
  }
}

module identity './modules/iam/identity.bicep' = if (deployWebAppEffective) {
  name: 'appIdentity${deploymentSuffix}'
  params: {
    identityName: resourceNames.outputs.userAssignedIdentityName
    location: location
    tags: commonTags
  }
}

module appServicePlanModule './modules/webapp/websiteserviceplan.bicep' = if (deployWebAppEffective) {
  name: 'appService${deploymentSuffix}'
  params: {
    location: location
    commonTags: commonTags
    sku: webSiteSku
    appServicePlanName: servicePlanName == '' ? resourceNames.outputs.webSiteAppServicePlanName : servicePlanName
    existingServicePlanName: servicePlanName
    existingServicePlanResourceGroupName: servicePlanResourceGroupName
    webAppKind: webAppKind
  }
}

module webSiteModule './modules/webapp/website.bicep' = if (deployWebAppEffective) {
  name: 'webSite${deploymentSuffix}'
  params: {
    webSiteName: resourceNames.outputs.webSiteName
    location: location
    appInsightsLocation: location
    commonTags: commonTags
    environmentCode: environmentCode
    webAppKind: webAppKind
    managedIdentityId: identity!.outputs.managedIdentityId
    managedIdentityPrincipalId: identity!.outputs.managedIdentityPrincipalId
    workspaceId: logAnalyticsWorkspaceModule!.outputs.id
    appServicePlanName: appServicePlanModule!.outputs.name
    appServicePlanResourceGroupName: appServicePlanModule!.outputs.resourceGroupName
  }
}

// --------------------------------------------------------------------------------
output SUBSCRIPTION_ID string = subscription().subscriptionId
output RESOURCE_GROUP_NAME string = resourceGroupName
output DEPLOYMENT_TYPE string = deploymentTypeNormalized
output HOST_NAME string = deployWebAppEffective ? webSiteModule!.outputs.hostName : ''
output WEB_HOST_NAME string = deployWebAppEffective ? webSiteModule!.outputs.hostName : ''
output WEB_URL string = deployWebAppEffective ? 'https://${webSiteModule!.outputs.hostName}' : ''
