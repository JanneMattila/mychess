Param (
    [Parameter(HelpMessage = "Deployment target resource group")] 
    [string] $ResourceGroupName = "rg-mychess-local",

    [Parameter(HelpMessage = "Deployment target resource group location")] 
    [string] $Location = "West Europe",

    [Parameter(HelpMessage = "Deployment environment name")] 
    [string] $EnvironmentName = "local",

    [Parameter(HelpMessage = "GitHub Repo")]
    [string] $Repo = "https://github.com/jannemattila/mychess",
    
    [Parameter(HelpMessage = "GitHub branch")] 
    [string] $Branch = "main",
    
    [Parameter(HelpMessage = "GitHub token")] 
    [securestring] $GitHubToken,

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

    [string] $Template = "$PSScriptRoot\azuredeploy.bicep",
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
$additionalParameters['repositoryUrl'] = $Repo
$additionalParameters['branch'] = $Branch
$additionalParameters['repositoryToken'] = $GitHubToken
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

if ($null -eq $result.Outputs.customDomainUri) {
    Throw "Template deployment didn't return web app information correctly and therefore deployment is cancelled."
}

$result

$customDomainUri = $result.Outputs.customDomainUri.value

# Publish variable to the Azure DevOps agents so that they
# can be used in follow-up tasks such as application deployment
Write-Host "##vso[task.setvariable variable=Custom.WebAppUri;]https://$CustomDomain"

$azureADdeployment = . $PSScriptRoot\deploy_aad_apps.ps1 `
    -EnvironmentName $EnvironmentName `
    -SPAUri $customDomainUri `
    -UpdateReplyUrl # Update reply urls
