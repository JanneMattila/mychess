Param (
    [Parameter(HelpMessage = "Application name")] 
    [string] $AppName = "My Chess",

    [Parameter(HelpMessage = "Deployment environment name")] 
    [string] $EnvironmentName = "local",
    
    [Parameter(HelpMessage = "Flag to indicate if AzureAD applications reply urls should be updated")] 
    [switch] $UpdateReplyUrl,

    [Parameter(HelpMessage = "SPA Uri")] 
    [string] $SPAUri = "http://localhost:5000/",
    
    [Parameter(HelpMessage = "API Backend Uri")] 
    [string] $APIUri = "http://localhost:7071/"
)

$ErrorActionPreference = "Stop"

# Use existing Azure context to login to Azure AD
$context = Get-AzContext
$accountId = $context.Account.Id
$tenant = $context.Tenant.TenantId
$scope = "https://graph.windows.net" # Azure AD Graph API
$dialog = [Microsoft.Azure.Commands.Common.Authentication.ShowDialog]::Never

$azureSession = [Microsoft.Azure.Commands.Common.Authentication.AzureSession]::Instance.AuthenticationFactory.Authenticate($context.Account, $context.Environment, $tenant, $null, $dialog, $null, $scope)

# Azure AD Graph API token
$accessToken = $azureSession.AccessToken

$aadInstalledModule = Get-Module -Name "AzureAD" -ListAvailable
if ($null -eq $aadInstalledModule) {
    Install-Module AzureAD -Scope CurrentUser -Force
}
else {
    Import-Module AzureAD
}

Connect-AzureAD -AadAccessToken $accessToken -AccountId $accountId -TenantId $tenant | Out-Null

if ("Prod" -eq $EnvironmentName) {
    $spaAppName = "$AppName"
    $apiAppName = "$AppName Backend"
}
else {
    $spaAppName = "$AppName $EnvironmentName"
    $apiAppName = "$AppName Backend $EnvironmentName"
}

Get-AzureADApplication -Filter "DisplayName eq '$spaAppName'"
$spaApp = Get-AzureADApplication -Filter "DisplayName eq '$spaAppName'"
$apiApp = Get-AzureADApplication -Filter "DisplayName eq '$apiAppName'"

if ($null -ne $spaApp) {
    # Applications have been already created
    Write-Host "Applications have been already created"

    if ($UpdateReplyUrl) {
        if ($spaApp.Homepage -ne $SPAUri) {
            Write-Host "Updating SPA urls"
            Set-AzureADApplication `
                -ObjectId $spaApp.ObjectId `
                -ReplyUrls $SPAUri `
                -Homepage $SPAUri
        }
        else {
            Write-Host "No need to update SPA urls"
        }

        if ($apiApp.Homepage -ne $APIUri) {
            Write-Host "Updating API urls"
            Set-AzureADApplication -ObjectId $apiApp.ObjectId -Homepage $APIUri
        }
        else {
            Write-Host "No need to update API urls"
        }
    }
}
else {
    ######################
    # Setup functions app:
    # - Expose API "User.ReadWrite"
    # - Expose API "Games.ReadWrite"
    $permissions = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]

    # Custom identifiers for our APIs
    $permissionUserReadWrite = "74f7cc22-157a-4c09-9039-d03645fda085" # "User.ReadWrite"
    $permissionGamesReadWrite = "e49b5223-2def-45c2-a632-b48b07c93124" # "Games.ReadWrite"

    $userReadWritePermission = New-Object Microsoft.Open.AzureAD.Model.OAuth2Permission
    $userReadWritePermission.Id = $permissionUserReadWrite
    $userReadWritePermission.Value = "User.ReadWrite"
    $userReadWritePermission.Type = "User"
    $userReadWritePermission.AdminConsentDisplayName = "Admin consent for granting read-write access to user data"
    $userReadWritePermission.AdminConsentDescription = "Admin consent for granting read-write access to user data"
    $userReadWritePermission.UserConsentDisplayName = "Read-write access to user data"
    $userReadWritePermission.UserConsentDescription = "Read-write access to user data"
    $permissions.Add($userReadWritePermission)

    $gamesReadWritePermission = New-Object Microsoft.Open.AzureAD.Model.OAuth2Permission
    $gamesReadWritePermission.Id = $permissionGamesReadWrite
    $gamesReadWritePermission.Value = "Games.ReadWrite"
    $gamesReadWritePermission.Type = "User"
    $gamesReadWritePermission.AdminConsentDisplayName = "Admin consent for granting read-write access to games data"
    $gamesReadWritePermission.AdminConsentDescription = "Admin consent for granting read-write access to games data"
    $gamesReadWritePermission.UserConsentDisplayName = "Read-write access to games data"
    $gamesReadWritePermission.UserConsentDescription = "Read-write access to games data"
    $permissions.Add($gamesReadWritePermission)

    #
    # Note: "New-AzureADApplication" does not yet support
    # setting up "signInAudience" to "AzureADandPersonalMicrosoftAccount"
    # 
    $postfix = $EnvironmentName.ToLower()
    $apiApp = New-AzureADApplication -DisplayName $apiAppName `
        -AvailableToOtherTenants $true `
        -IdentifierUris "api://mychess.backend.$postfix" `
        -PublicClient $false `
        -Oauth2Permissions $permissions
    $apiApp

    New-AzureADServicePrincipal -AppId $apiApp.AppId

    ###########################
    # Setup SPA app:
    $spaAccesses = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]

    # API permission for "User.ReadWrite" in backend app
    $spaUserReadWrite = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
    $spaUserReadWrite.Id = $userReadWritePermission.Id # "User.ReadWrite"
    $spaUserReadWrite.Type = "Scope"

    # API permission for "Games.ReadWrite" in backend app
    $spaGamesReadWrite = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
    $spaGamesReadWrite.Id = $gamesReadWritePermission.Id # "Games.ReadWrite"
    $spaGamesReadWrite.Type = "Scope"   

    $spaApi = New-Object Microsoft.Open.AzureAD.Model.RequiredResourceAccess
    $spaApi.ResourceAppId = $apiApp.AppId # Backend app
    $spaApi.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]
    $spaApi.ResourceAccess.Add($spaUserReadWrite)
    $spaApi.ResourceAccess.Add($spaGamesReadWrite)

    # Add required accesses
    $spaAccesses.Add($spaApi)

    #
    # Note: "New-AzureADApplication" does not yet support
    # setting up "signInAudience" to "AzureADandPersonalMicrosoftAccount"
    # 
    Write-Warning @"
You need to *manually* update these two properties:
- "signInAudience" to value "AzureADandPersonalMicrosoftAccount"
- "accessTokenAcceptedVersion" to value 2
"@
    $spaApp = New-AzureADApplication -DisplayName $spaAppName `
        -AvailableToOtherTenants $true `
        -Oauth2AllowImplicitFlow $true `
        -Homepage $SPAUri `
        -ReplyUrls $SPAUri `
        -RequiredResourceAccess $spaAccesses
    $spaReaderApp

    New-AzureADServicePrincipal -AppId $spaApp.AppId

    Write-Host "Updating SPA icon"
    Set-AzureADApplicationLogo `
        -ObjectId $spaApp.ObjectId `
        -FilePath $PSScriptRoot\Logo_48x48.png
}

$values = new-object psobject -property @{
    SPAApp           = $spaApp.AppId;
    ApiApp           = $apiApp.AppId;
    TenantId         = $tenant;
    ApplicationIdURI = $apiApp.IdentifierUris[0];
}
return $values
