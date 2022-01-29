Install-Module AzTable

$tableNames = "Users","UserFriends","UserNotifications","UserSettings","UserID2User","GamesWaitingForYou","GamesWaitingForOpponent","GamesArchive"

Login-AzAccount

# Get source storage context
Select-AzSubscription -Subscription SourceSub
$sourceResourceGroup = "rg-mychess-source"
$sourceStorageName = "mychesssource"

$sourceStorage = Get-AzStorageAccount -ResourceGroupName $sourceResourceGroup -Name $sourceStorageName
$sourceStorageContext = $sourceStorage.Context

# Get destination storage context
Select-AzSubscription -Subscription DestinationSub
$destinationResourceGroup = "rg-mychess-destination"
$destinationStorageName = "mychessdestination"

$destinationStorage = Get-AzStorageAccount -ResourceGroupName $destinationResourceGroup -Name $destinationStorageName
$destinationStorageContext = $destinationStorage.Context

# Process each table
foreach($tableName in $tableNames)
{
  New-AzStorageTable -Name $tableName -Context $destinationStorageContext -ErrorAction Continue

  $sourceTable = (Get-AzStorageTable -Name $tableName -Context $sourceStorageContext).CloudTable
  $destinationTable = (Get-AzStorageTable -Name $tableName -Context $destinationStorageContext).CloudTable

  $sourceRows = Get-AzTableRow -table $sourceTable
  $sourceRows | ft
  foreach($sourceRow in $sourceRows){
    $properties = @{}
    $sourceRow.psobject.properties | Foreach { $properties[$_.Name] = $_.Value }
    $properties.Etag = ""

    Add-AzTableRow `
      -Table $destinationTable `
      -PartitionKey $sourceRow.PartitionKey `
      -RowKey $sourceRow.RowKey `
      -Property $properties `
      -UpdateExisting
  }
}
