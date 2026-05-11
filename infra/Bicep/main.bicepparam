// ----------------------------------------------------------------------------------------------------
// Shared Pipeline Parameter File (Azure DevOps + GitHub Actions)
// ----------------------------------------------------------------------------------------------------
using './main.bicep'

param appName = '#{APP_NAME}#'
param environmentCode = '#{ENVCODE}#'
param location = '#{RESOURCE_GROUP_LOCATION}#'
param instanceNumber = '#{INSTANCE_NUMBER}#'
param deploymentType = '#{DEPLOYMENT_TYPE}#'
param webAppKind = 'linux' // 'linux' or 'windows'
