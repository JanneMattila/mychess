Param (
    [Parameter(HelpMessage="Deployment target resource group")] 
    [string] $ResourceGroupName = "rg-mychess-local",

    [Parameter(HelpMessage="Deployment target resource group location")] 
    [string] $Location = "North Europe",

    [Parameter(HelpMessage="Deployment environment name")] 
    [string] $EnvironmentName = "local",

    [Parameter(HelpMessage="CDN name (must be globally unique and map to custom domain name)")] 
    [string] $CDN = "mychess-local",

    [Parameter(Mandatory=$true, HelpMessage="Custom domain name for the CDN")] 
    [string] $CustomDomain,

    [Parameter(HelpMessage="App root folder path to publish e.g. ..\src\MyChessReact\build\")] 
    [string] $AppRootFolder,

    [string] $Template = "$PSScriptRoot\azuredeploy.json",
    [string] $TemplateParameters = "$PSScriptRoot\azuredeploy.parameters.json"
)

$ErrorActionPreference = "Stop"

$date = (Get-Date).ToString("yyyy-MM-dd-HH-mm-ss")
$deploymentName = "Local-$date"

if ([string]::IsNullOrEmpty($env:BUILD_BUILDNUMBER))
{
    Write-Host (@"
Not executing inside Azure DevOps Release Management.
Make sure you have done "Login-AzAccount" and
"Select-AzSubscription -SubscriptionName name"
so that script continues to work correctly for you.
"@)
}
else
{
    $deploymentName = $env:BUILD_BUILDNUMBER
}

if ($null -eq (Get-AzResourceGroup -Name $ResourceGroupName -Location $Location -ErrorAction SilentlyContinue))
{
    Write-Warning "Resource group '$ResourceGroupName' doesn't exist and it will be created."
    New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Verbose
}

$azureADdeployment = . $PSScriptRoot\deploy_aad_apps.ps1 -EnvironmentName $EnvironmentName

# Additional parameters that we pass to the template deployment
$additionalParameters = New-Object -TypeName hashtable
$additionalParameters['cdn'] = $CDN
$additionalParameters['customDomain'] = $CustomDomain
$additionalParameters['clientId'] = $azureADdeployment.ApiApp
$additionalParameters['tenantId'] = $azureADdeployment.TenantId
$additionalParameters['applicationIdURI'] = $azureADdeployment.ApplicationIdURI

$result = New-AzResourceGroupDeployment `
    -DeploymentName $deploymentName `
    -ResourceGroupName $ResourceGroupName `
    -TemplateFile $Template `
    -TemplateParameterFile $TemplateParameters `
    @additionalParameters `
    -Mode Complete -Force `
    -Verbose

if ($null -eq $result.Outputs.webStorageName -or
    $null -eq $result.Outputs.webStorageName -or
    $null -eq $result.Outputs.webAppName -or
    $null -eq $result.Outputs.webAppUri -or
    $null -eq $result.Outputs.instrumentationKey -or
    $null -eq $result.Outputs.cdnName -or
    $null -eq $result.Outputs.cdnCustomDomainName)
{
    Throw "Template deployment didn't return web app information correctly and therefore deployment is cancelled."
}

$result

$appStorageName = $result.Outputs.appStorageName.value
$webStorageName = $result.Outputs.webStorageName.value
$webAppName = $result.Outputs.webAppName.value
$webAppUri = $result.Outputs.webAppUri.value
$instrumentationKey = $result.Outputs.instrumentationKey.value
$cdnName = $result.Outputs.cdnName.value
$cdnCustomDomainName = $result.Outputs.cdnCustomDomainName.value

# Enable CDN managed certificate to enable https on custom domain
$cdnCustomDomain = Get-AzCdnCustomDomain -ResourceGroupName $ResourceGroupName -ProfileName $cdnName -EndpointName $cdn -CustomDomainName $cdnCustomDomainName
if ("Disabled" -eq $cdnCustomDomain.CustomHttpsProvisioningState)
{
    Enable-AzCdnCustomDomainHttps -ResourceGroupName $ResourceGroupName -ProfileName $cdnName -EndpointName $cdn -CustomDomainName $cdnCustomDomainName
}

$webStorageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $webStorageName
Enable-AzStorageStaticWebsite -Context $webStorageAccount.Context -IndexDocument index.html -ErrorDocument404Path 404.html
$webStorageUri = $webStorageAccount.PrimaryEndpoints.Web
Write-Host "Static website endpoint: $webStorageUri"

# Create table to the storage if it does not exist
$tableName = "games"
$appStorageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $appStorageName
if ($null -eq (Get-AzStorageTable -Context $appStorageAccount.Context -Name $tableName -ErrorAction SilentlyContinue))
{
    Write-Warning "Table '$tableName' doesn't exist and it will be created."
    New-AzStorageTable -Context $appStorageAccount.Context -Name $tableName
}

# Publish variable to the Azure DevOps agents so that they
# can be used in follow-up tasks such as application deployment
Write-Host "##vso[task.setvariable variable=Custom.WebStorageName;]$webStorageName"
Write-Host "##vso[task.setvariable variable=Custom.WebStorageUri;]$webStorageUri"
Write-Host "##vso[task.setvariable variable=Custom.WebAppName;]$webAppName"
Write-Host "##vso[task.setvariable variable=Custom.WebAppUri;]https://$CustomDomain"

$azureADdeployment = . $PSScriptRoot\deploy_aad_apps.ps1 `
    -EnvironmentName $EnvironmentName `
    -SPAUri $webStorageUri `
    -UpdateReplyUrl # Update reply urls

if (![string]::IsNullOrEmpty($AppRootFolder))
{
    . $PSScriptRoot\deploy_web.ps1 `
        -ResourceGroupName $ResourceGroupName `
        -FunctionsUri $webAppUri `
        -IntrumentationKey $instrumentationKey `
        -WebStorageName $webStorageAccount.StorageAccountName `
        -AppRootFolder $AppRootFolder
}
