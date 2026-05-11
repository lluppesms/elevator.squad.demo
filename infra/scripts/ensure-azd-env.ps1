Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-CurrentValue {
    param([Parameter(Mandatory = $true)][string]$Name)

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $null
    }

    return $value
}

function Ensure-Value {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Prompt,
        [string]$Default = ''
    )

    $current = Get-CurrentValue -Name $Name
    if ($null -ne $current) {
        Write-Host "$Name already set to '$current'."
        return
    }

    $suffix = if ([string]::IsNullOrWhiteSpace($Default)) { '' } else { " [$Default]" }
    $value = Read-Host "$Prompt$suffix"
    if ([string]::IsNullOrWhiteSpace($value)) {
        $value = $Default
    }

    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "A value is required for $Name."
    }

    azd env set $Name $value | Out-Null
    Write-Host "$Name set to '$value'."
}

Ensure-Value -Name 'APP_NAME' -Prompt 'App name (globally unique)' -Default 'elevator'
Ensure-Value -Name 'DEPLOYMENT_TYPE' -Prompt 'Deployment type' -Default 'webapp'
Ensure-Value -Name 'ENVCODE' -Prompt 'Environment code' -Default 'dev'
Ensure-Value -Name 'INSTANCE_NUMBER' -Prompt 'Instance number' -Default '1'

$locationDefault = Get-CurrentValue -Name 'AZURE_LOCATION'
if ([string]::IsNullOrWhiteSpace($locationDefault)) {
    $locationDefault = 'centralus'
}
Ensure-Value -Name 'RESOURCE_GROUP_LOCATION' -Prompt 'Resource group location' -Default $locationDefault
