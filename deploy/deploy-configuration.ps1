Param (
    [Parameter(HelpMessage = "Client Id")] 
    [string] $ClientID,

    [Parameter(HelpMessage = "API Application IdURI")] 
    [string] $ApplicationIdURI,
        
    [Parameter(HelpMessage = "App Insights Instrumentation Key")] 
    [string] $IntrumentationKey,
        
    [Parameter(HelpMessage = "WebPush Public Key")] 
    [string] $WebPushPublicKey,

    [Parameter(HelpMessage = "App root folder path to publish e.g. ..\src\MyChess.Client\wwwroot\")] 
    [string] $AppRootFolder = "src/MyChess.Client/wwwroot"
)

$ErrorActionPreference = "Stop"

$AppRootFolder = (Resolve-Path $AppRootFolder).Path
Write-Host "Processing folder: $AppRootFolder"

$settings = @{
    AzureAd = @{
        Authority ="htts://login.microsoftonline.com/common"
        ClientId = $ClientID
        ValidateAuthority = $true
    }
    applicationIdURI = $ApplicationIdURI
    instrumentationKey = $IntrumentationKey
    webPushPublicKey = $WebPushPublicKey
}
$settings | ConvertTo-Json | Set-Content (Join-Path -Path $AppRootFolder -ChildPath appsettings.json)
