Param (
    [Parameter(HelpMessage="Application name")] 
    [string] $AppName = "My Chess",

    [Parameter(HelpMessage="Deployment environment name")] 
    [string] $EnvironmentName = "local",
    
    [Parameter(HelpMessage="Flag to indicate if AzureAD applications reply urls should be updated")] 
    [switch] $UpdateReplyUrl,

    [Parameter(HelpMessage="SPA Uri")] 
    [string] $SPAUri = "http://localhost:5000/",
    
    [Parameter(HelpMessage="API Backend Uri")] 
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
if ($null -eq $aadInstalledModule)
{
    Install-Module AzureAD -Scope CurrentUser -Force
}
else
{
    Import-Module AzureAD
}

Connect-AzureAD -AadAccessToken $accessToken -AccountId $accountId -TenantId $tenant | Out-Null

$spaAppName = "$AppName $EnvironmentName"
$apiAppName = "$AppName API $EnvironmentName"

$spaApp = Get-AzureADApplication -SearchString $spaAppName
$apiApp = Get-AzureADApplication -SearchString $apiAppName

if ($null -ne $spaApp)
{
    # Applications have been already created
    Write-Host "Applications have been already created"

    if ($UpdateReplyUrl)
    {
        Set-AzureADApplication -ObjectId $spaApp.ObjectId -ReplyUrls $SPAUri
        Set-AzureADApplication -ObjectId $apiApp.ObjectId -ReplyUrls $APIUri
    }
}
else
{
    ######################
    # Setup functions app:
    # - Expose API "User.ReadWrite"
    # - Expose API "Games.ReadWrite"
    $permissions = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.OAuth2Permission]

    # Known identifiers of Microsoft Graph API
    $microsoftGraphAPI = "00000003-0000-0000-c000-000000000000"
    $userRead = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # "User.Read"

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
    $apiApp = New-AzureADApplication -DisplayName $apiAppName `
        -AvailableToOtherTenants $true `
        -IdentifierUris "api://mychess-func.$EnvironmentName" `
        -PublicClient $false `
        -Oauth2Permissions $permissions
    $apiApp

    $apiSpn = New-AzureADServicePrincipal -AppId $apiApp.AppId

    ###########################
    # Setup SPA app:
    $spaAccesses = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]

    # API permission for "User.Read" in Microsoft Graph
    $spaUserReadGraph = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
    $spaUserReadGraph.Id = $userRead # "User.Read"
    $spaUserReadGraph.Type = "Scope"

    $spaGraph = New-Object Microsoft.Open.AzureAD.Model.RequiredResourceAccess
    $spaGraph.ResourceAppId = $microsoftGraphAPI # "Microsoft Graph API"
    $spaGraph.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]
    $spaGraph.ResourceAccess.Add($spaUserReadGraph)

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
    $spaAccesses.Add($spaGraph)
    $spaAccesses.Add($spaApi)

    #
    # Note: "New-AzureADApplication" does not yet support
    # setting up "signInAudience" to "AzureADandPersonalMicrosoftAccount"
    # 
    $spaApp = New-AzureADApplication -DisplayName $spaAppName `
        -AvailableToOtherTenants $true `
        -Oauth2AllowImplicitFlow $true `
        -Homepage $SPAUri `
        -ReplyUrls $SPAUri `
        -RequiredResourceAccess $spaAccesses
    $spaReaderApp

    $spaSpn = New-AzureADServicePrincipal -AppId $spaApp.AppId
}

$values = new-object psobject -property @{
    SPAApp = $spaSpn.AppId;
    ApiApp = $apiApp.AppId;
    TenantId = $tenant;
    ApplicationIdURI = $apiApp.IdentifierUris[0];
}
return $values
