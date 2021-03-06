Param (
    [Parameter(HelpMessage = "Deployment target resource group")] 
    [string] $ResourceGroupName = "rg-mychess-local",

    [Parameter(HelpMessage = "Deployment target resource group location")] 
    [string] $Location = "North Europe",

    [Parameter(HelpMessage = "Deployment environment name")] 
    [string] $EnvironmentName = "local",

    [Parameter(HelpMessage = "CDN name (must be globally unique and map to custom domain name)")] 
    [string] $CDN = "mychess-local",

    [Parameter(Mandatory = $true, HelpMessage = "Custom domain name for the CDN")] 
    [string] $CustomDomain,

    [Parameter(Mandatory = $true, HelpMessage = "Alert email address")]
    [string] $AlertEmailAddress,

    [Parameter(HelpMessage="SignalR Pricing tier. Check details at https://azure.microsoft.com/en-us/pricing/details/signalr-service/")] 
    [ValidateSet("Free_F1", "Standard_S1")]
    [string] $SignalRServicePricingTier = "Free_F1",

    [Parameter(HelpMessage="SignalR Service unit count")] 
    [ValidateSet(1, 2, 5, 10, 20, 50, 100)]
    [int] $SignalRServiceUnits = 1,

    [Parameter(Mandatory = $true, HelpMessage = "WebPush Public Key")]
    [string] $WebPushPublicKey,
    
    [Parameter(Mandatory = $true, HelpMessage = "WebPush Private Key")]
    [string] $WebPushPrivateKey,

    [Parameter(HelpMessage = "App root folder path to publish e.g. ..\src\MyChessReact\build\")] 
    [string] $AppRootFolder,

    [string] $Template = "$PSScriptRoot\azuredeploy.json",
    [string] $TemplateParameters = "$PSScriptRoot\azuredeploy.parameters.json"
)

$ErrorActionPreference = "Stop"

$date = (Get-Date).ToString("yyyy-MM-dd-HH-mm-ss")
$deploymentName = "Local-$date"

if ([string]::IsNullOrEmpty($env:BUILD_BUILDNUMBER)) {
    Write-Host (@"
Not executing inside Azure DevOps Release Management.
Make sure you have done "Login-AzAccount" and
"Select-AzSubscription -SubscriptionName name"
so that script continues to work correctly for you.
"@)
}
else {
    $deploymentName = $env:BUILD_BUILDNUMBER
}

if ($null -eq (Get-AzResourceGroup -Name $ResourceGroupName -Location $Location -ErrorAction SilentlyContinue)) {
    Write-Warning "Resource group '$ResourceGroupName' doesn't exist and it will be created."
    New-AzResourceGroup -Name $ResourceGroupName -Location $Location -Verbose
}

$azureADdeployment = . $PSScriptRoot\deploy_aad_apps.ps1 -EnvironmentName $EnvironmentName

# Additional parameters that we pass to the template deployment
$additionalParameters = New-Object -TypeName hashtable
$additionalParameters['cdn'] = $CDN
$additionalParameters['customDomain'] = $CustomDomain
$additionalParameters['clientId'] = $azureADdeployment.ApiApp
$additionalParameters['applicationIdURI'] = $azureADdeployment.ApplicationIdURI
$additionalParameters['alertEmailAddress'] = $AlertEmailAddress
$additionalParameters['signalRServicePricingTier'] = $SignalRServicePricingTier
$additionalParameters['signalRServiceUnits'] = $SignalRServiceUnits
$additionalParameters['webPushPublicKey'] = $WebPushPublicKey
$additionalParameters['webPushPrivateKey'] = $WebPushPrivateKey

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
    $null -eq $result.Outputs.cdnCustomDomainName -or
    $null -eq $result.Outputs.cdnCustomDomainUri) {
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
$cdnCustomDomainUri = $result.Outputs.cdnCustomDomainUri.value

# Enable CDN managed certificate to enable https on custom domain
$cdnCustomDomain = Get-AzCdnCustomDomain -ResourceGroupName $ResourceGroupName -ProfileName $cdnName -EndpointName $cdn -CustomDomainName $cdnCustomDomainName
if ("Disabled" -eq $cdnCustomDomain.CustomHttpsProvisioningState) {
    Enable-AzCdnCustomDomainHttps -ResourceGroupName $ResourceGroupName -ProfileName $cdnName -EndpointName $cdn -CustomDomainName $cdnCustomDomainName
}

$webStorageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $webStorageName
Enable-AzStorageStaticWebsite -Context $webStorageAccount.Context -IndexDocument index.html -ErrorDocument404Path 404.html
$webStorageUri = $webStorageAccount.PrimaryEndpoints.Web
Write-Host "Static website endpoint: $webStorageUri"

# Publish variable to the Azure DevOps agents so that they
# can be used in follow-up tasks such as application deployment
Write-Host "##vso[task.setvariable variable=Custom.WebStorageName;]$webStorageName"
Write-Host "##vso[task.setvariable variable=Custom.WebStorageUri;]$webStorageUri"
Write-Host "##vso[task.setvariable variable=Custom.WebAppName;]$webAppName"
Write-Host "##vso[task.setvariable variable=Custom.WebAppUri;]https://$CustomDomain"

$azureADdeployment = . $PSScriptRoot\deploy_aad_apps.ps1 `
    -EnvironmentName $EnvironmentName `
    -SPAUri $cdnCustomDomainUri `
    -UpdateReplyUrl # Update reply urls

if (![string]::IsNullOrEmpty($AppRootFolder)) {
    . $PSScriptRoot\deploy_web.ps1 `
        -ResourceGroupName $ResourceGroupName `
        -FunctionsUri $webAppUri `
        -SPAAppAppID $azureADdeployment.SPAApp `
        -ApiAppAppID $azureADdeployment.ApiApp `
        -ApiApplicationIdURI $azureADdeployment.ApplicationIdURI `
        -IntrumentationKey $instrumentationKey `
        -WebPushPublicKey $WebPushPublicKey `
        -WebStorageName $webStorageAccount.StorageAccountName `
        -AppRootFolder $AppRootFolder

    # Purge the CDN cache
    Unpublish-AzCdnEndpointContent `
        -ResourceGroupName $ResourceGroupName `
        -ProfileName $cdnName `
        -EndpointName $cdn `
        -PurgeContent "/*"
}
