Param (
    [Parameter(HelpMessage = "Application name")] 
    [string] $AppName = "My Chess",

    [Parameter(HelpMessage = "Deployment environment name")] 
    [string] $EnvironmentName = "local",
    
    [Parameter(HelpMessage = "Flag to indicate if AzureAD applications reply urls should be updated")] 
    [switch] $UpdateReplyUrl,

    [Parameter(HelpMessage = "SPA Uri")] 
    [string] $SPAUri = "http://localhost:5000/"
)

$ErrorActionPreference = "Stop"

# Custom identifiers for our APIs
$permissionUserReadWrite = "74f7cc22-157a-4c09-9039-d03645fda085" # "User.ReadWrite"
$permissionGamesReadWrite = "e49b5223-2def-45c2-a632-b48b07c93124" # "Games.ReadWrite"

# Use existing Azure context to login to Azure AD
$context = Get-AzContext
$tenant = $context.Tenant.TenantId

Write-Host "Preparing Microsoft.Graph module..."

$installedModule = Get-Module -Name "Microsoft.Graph" -ListAvailable
if ($null -eq $installedModule) {
    Install-Module Microsoft.Graph -Scope CurrentUser
}
else {
    Import-Module Microsoft.Graph
}

Write-Host "Connecting to Microsoft Graph..."

$accessToken = Get-AzAccessToken -ResourceTypeName MSGraph -TenantId $tenant
Connect-MgGraph -AccessToken $accessToken.Token

if ("Prod" -eq $EnvironmentName) {
    $spaAppName = "$AppName App"
    $apiAppName = "$AppName Backend"
}
else {
    $spaAppName = "$AppName App ($EnvironmentName)"
    $apiAppName = "$AppName Backend ($EnvironmentName)"
}

Write-Host "Finding applications '$spaAppName' and '$apiAppName'..."

$spaApp = Get-MgApplication -Search "DisplayName:$spaAppName" -ConsistencyLevel eventual
$apiApp = Get-MgApplication -Search "DisplayName:$apiAppName" -ConsistencyLevel Eventual
if ($null -ne $spaApp) {
    Write-Host "Applications have been already created"

    if ($UpdateReplyUrl) {

        Write-Host "Validating redirect uri"

        if ($spaApp.Spa.RedirectUris -ne $SPAUri) {
            Write-Host "Updating redirect uri"
            $spaApp.Web.HomePageUrl = $SPAUri
            $spaApp.Spa.RedirectUris = $SPAUri
            Update-MgApplication -ApplicationId $spaApp.Id -Spa $spaApp.Spa -Web $spaApp.Web
        }
        else {
            Write-Host "No need to update redirect uri"
        }
    }
}
else {
    Write-Host "Creating new applications"

    $spaApp = New-MgApplication -DisplayName $spaAppName `
        -SignInAudience AzureADandPersonalMicrosoftAccount `
        -Web @{ HomePageUrl = $SPAUri } `
        -Spa @{ RedirectUris = $SPAUri }

    # https://github.com/microsoftgraph/msgraph-sdk-powershell/issues/1028
    # -LogoInputFile $PSScriptRoot\Logo_48x48.png

    New-MgServicePrincipal -AppId $spaApp.AppId

    ######################
    # Setup functions app:
    # - Expose API "User.ReadWrite"
    # - Expose API "Games.ReadWrite"
    $permissions = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope]

    $userReadWritePermission = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope
    $userReadWritePermission.Id = $permissionUserReadWrite
    $userReadWritePermission.Value = "User.ReadWrite"
    $userReadWritePermission.Type = "User"
    $userReadWritePermission.AdminConsentDisplayName = "Admin consent for granting read-write access to user data"
    $userReadWritePermission.AdminConsentDescription = "Admin consent for granting read-write access to user data"
    $userReadWritePermission.UserConsentDisplayName = "Read-write access to user data"
    $userReadWritePermission.UserConsentDescription = "Read-write access to user data"
    $permissions.Add($userReadWritePermission)

    $gamesReadWritePermission = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope
    $gamesReadWritePermission.Id = $permissionGamesReadWrite
    $gamesReadWritePermission.Value = "Games.ReadWrite"
    $gamesReadWritePermission.Type = "User"
    $gamesReadWritePermission.AdminConsentDisplayName = "Admin consent for granting read-write access to games data"
    $gamesReadWritePermission.AdminConsentDescription = "Admin consent for granting read-write access to games data"
    $gamesReadWritePermission.UserConsentDisplayName = "Read-write access to games data"
    $gamesReadWritePermission.UserConsentDescription = "Read-write access to games data"
    $permissions.Add($gamesReadWritePermission)

    # Define SPA app to be pre-authorized app of API app
    $preAuthorizedApp = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPreAuthorizedApplication
    $preAuthorizedApp.AppId = $spaApp.AppId
    $preAuthorizedApp.DelegatedPermissionIds = $permissionUserReadWrite, $permissionGamesReadWrite
    
    $preAuthorizedApps = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphPreAuthorizedApplication]
    $preAuthorizedApps.Add($preAuthorizedApp)    

    $postfix = $EnvironmentName.ToLower()
    $apiApp = New-MgApplication -DisplayName $apiAppName `
        -SignInAudience AzureADandPersonalMicrosoftAccount `
        -IdentifierUris "api://mychess.backend.$postfix" `
        -Api @{
        Oauth2PermissionScopes      = $permissions.ToArray()
        PreAuthorizedApplications   = $preAuthorizedApps
        RequestedAccessTokenVersion = 2
    }

    New-MgServicePrincipal -AppId $apiApp.AppId

    ###########################
    # Finalize Setup of SPA app:
    $spaAccesses = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]

    # API permission for "User.ReadWrite" in backend app
    $spaUserReadWrite = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess
    $spaUserReadWrite.Id = $userReadWritePermission.Id # "User.ReadWrite"
    $spaUserReadWrite.Type = "Scope"

    # API permission for "Games.ReadWrite" in backend app
    $spaGamesReadWrite = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess
    $spaGamesReadWrite.Id = $gamesReadWritePermission.Id # "Games.ReadWrite"
    $spaGamesReadWrite.Type = "Scope"   

    $spaApi = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess
    $spaApi.ResourceAppId = $apiApp.AppId # Backend app
    $resourceAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]
    $resourceAccess.Add($spaUserReadWrite)
    $resourceAccess.Add($spaGamesReadWrite)
    $spaApi.ResourceAccess = $resourceAccess

    # Add required accesses
    $spaAccesses.Add($spaApi)

    Write-Host "Updating SPA API Permissions"
    Update-MgApplication -ApplicationId $spaApp.Id -RequiredResourceAccess $spaAccesses
}

$values = new-object psobject -property @{
    SPAApp           = $spaApp.AppId;
    ApiApp           = $apiApp.AppId;
    TenantId         = $tenant;
    ApplicationIdURI = $apiApp.IdentifierUris[0];
}
return $values
