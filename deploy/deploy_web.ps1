Param (
    [Parameter(HelpMessage = "Deployment target resource group")] 
    [string] $ResourceGroupName = "rg-mychess-local",
    
    [Parameter(HelpMessage = "Azure Functions root uri")] 
    [string] $FunctionsUri,

    [Parameter(HelpMessage = "SPA Application Id")] 
    [string] $SPAAppAppID,

    [Parameter(HelpMessage = "API Application Id")] 
    [string] $ApiAppAppID,

    [Parameter(HelpMessage = "API Application IdURI")] 
    [string] $ApiApplicationIdURI,
        
    [Parameter(HelpMessage = "App Insights Instrumentation Key")] 
    [string] $IntrumentationKey,

    [Parameter(HelpMessage = "Deployment target storage account name")] 
    [string] $WebStorageName,

    [Parameter(HelpMessage = "App root folder path to publish e.g. ..\src\MyChessWeb\wwwroot\")] 
    [string] $AppRootFolder
)

$ErrorActionPreference = "Stop"

function GetContentType([string] $extension) {
    if ($extension -eq ".html") {
        return "text/html"
    }
    elseif ($extension -eq ".svg") {
        return "image/svg+xml"
    }
    elseif ($extension -eq ".css") {
        return "text/css"
    }
    elseif ($extension -eq ".js") {
        return "text/javascript"
    }
    elseif ($extension -eq ".json") {
        return "application/json"
    }
    return "text/plain"
}

$AppRootFolder = (Resolve-Path $AppRootFolder).Path
Write-Host "Processing folder: $AppRootFolder"

$storageAccount = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $WebStorageName

$webStorageUri = $storageAccount.PrimaryEndpoints.Web
Write-Host "Static website endpoint: $webStorageUri"

"{ `"endpoint`": `"$FunctionsUri`", `
   `"clientId`": `"$SPAAppAppID`", `
   `"applicationIdURI`": `"$ApiApplicationIdURI`", `
   `"instrumentationKey`": `"$IntrumentationKey`" }" | Set-Content (Join-Path -Path $AppRootFolder -ChildPath configuration.json)

Get-ChildItem -File -Recurse $AppRootFolder `
| ForEach-Object { 
    $name = $_.FullName.Replace($AppRootFolder, "")
    $contentType = GetContentType($_.Extension)
    $properties = @{"ContentType" = $contentType }

    Write-Host "Deploying file: $name"
    Set-AzStorageBlobContent -File $_.FullName -Blob $name -Container `$web -Context $storageAccount.Context -Properties $properties -Force
}
