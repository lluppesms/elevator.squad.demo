// ----------------------------------------------------------------------------------------------------
// This BICEP file is the main entry point for the azd command
// ----------------------------------------------------------------------------------------------------
// Note: Resources need azd-env-name tag and azd-service-name tags (it's already in the bicep files):
//   See https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/faq
// ----------------------------------------------------------------------------------------------------
param name string
param location string
param runDateTime string = utcNow()

// --------------------------------------------------------------------------------
targetScope = 'subscription'

// --------------------------------------------------------------------------------
var tags = {
    Application: name
    LastDeployed: runDateTime
    'azd-env-name': name
}
var deploymentSuffix = '-${runDateTime}'

// --------------------------------------------------------------------------------
resource resourceGroup 'Microsoft.Resources/resourceGroups@2020-06-01' = {
    name: 'rg-${name}'
    location: location
    tags: tags
}

module resources './Bicep/main.bicep' = {
    name: 'resources-${deploymentSuffix}'
    scope: resourceGroup
    params: {
        location: location
        appName: name
        environmentCode: 'azd'
        deploymentType: 'webapp'
    }
}
