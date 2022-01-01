Param (
    [Parameter(HelpMessage = "Deployment target resource group")] 
    [string] $ResourceGroupName = "rg-mychess-local",

    [Parameter(HelpMessage = "Deployment target resource group location")] 
    [string] $Location = "West Europe",

    [Parameter(HelpMessage = "Deployment environment name")] 
    [string] $EnvironmentName = "local",

    [Parameter(HelpMessage = "GitHub Repo")] 
    [string] $Repo = "jannemattila/mychess",

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

    [string] $Template = "$PSScriptRoot\azuredeploy.json",
    [string] $TemplateParameters = "$PSScriptRoot\azuredeploy.parameters.json"
)

$ErrorActionPreference = "Stop"

az group create -n $ResourceGroupName -l $Location
az staticwebapp create `
    --name $CDN `
    --resource-group $ResourceGroupName `
    --source "https://github.com/$Repo" `
    --location $Location `
    --sku Standard `
    --branch main `
    --app-location "src/MyChess.Client" `
    --api-location "src/MyChess.Functions" `
    --branch (git branch --show-current) `
    --login-with-github

az staticwebapp appsettings set `
    --name $CDN `
    --resource-group $ResourceGroupName `
    --setting-names key1=val1 key2=val2
