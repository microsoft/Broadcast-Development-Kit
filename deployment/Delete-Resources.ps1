param(
    $config = "config.json"
)

if(-not (Test-Path -Path $config)){
    Write-Host "'${config}' configuration file is required" -ForegroundColor red
    Exit
}

$configuration = Get-Content $config | ConvertFrom-Json

$serviceName = $configuration.name
$botAppName = $configuration.appRegistrations.botAppName
$botServiceApiAppName = $configuration.appRegistrations.botServiceApiAppName
$botServiceClientAppName = $configuration.appRegistrations.botServiceClientAppName
$managementApiAppName = $configuration.appRegistrations.managementApiAppName
$sdkAppName = $configuration.appRegistrations.sdkAppName

if (-not $serviceName){
    Write-Host "Error: A name should be provided in the configuration settings." -ForegroundColor red
    Exit
}

if (-not $botAppName) {
    $botAppName = "${serviceName}-bot-app"
}

if (-not $botServiceApiAppName) {
    $botServiceApiAppName = "${serviceName}-botservice-api"
}

if (-not $botServiceClientAppName) {
    $botServiceClientAppName = "${serviceName}-botservice-client"
}

if (-not $managementApiAppName) {
    $managementApiAppName = "${serviceName}-management-api"
}

if (-not $sdkAppName) {
    $sdkAppName = "${serviceName}-sdk-app"
}

$managementResourceGroup = $configuration.managementResourceGroup
$vmResourceGroup = $configuration.vmResourceGroup

if ( -not $managementResourceGroup){
    $managementResourceGroup = "${serviceName}-rg"
}

$vmResourceGroup = $configuration.vmResourceGroup
if ( -not $vmResourceGroup){
    $vmResourceGroup = "${serviceName}vm-rg"
}

$currentSubscriptionId = (az account show --query id --output tsv)
$dnsSubscriptionId= $configuration.dnsZone.dnsSubscriptionId
$zoneName = $configuration.dnsZone.zoneName
$dnsResourceGroup = $configuration.dnsZone.resourceGroup
$dnsRecordName = $configuration.dnsZone.dnsRecordName


Write-Host "`nDeleting Reource Groups.." -ForegroundColor white -BackgroundColor black

$existManagementRg=az group exists -n $managementResourceGroup
if($existManagementRg -eq $true){
    Write-Host "`nDeleting $managementResourceGroup resource group.." -ForegroundColor green
    az group delete -n $managementResourceGroup --no-wait
    Write-Host "The deletion process has started, this may take several minutes" -ForegroundColor green
} else{
    Write-Host "INF: The $managementResourceGroup doesn't exist" -ForegroundColor red
}

$existVmRg=az group exists -n $vmResourceGroup
if($existVmRg -eq $true){
    Write-Host "`nDeleting $vmResourceGroup resource group.." -ForegroundColor green
    az group delete -n $vmResourceGroup --no-wait --force-deletion-types Microsoft.Compute/virtualMachines
    Write-Host "The deletion process has started, this may take several minutes" -ForegroundColor green
} else{
    Write-Host "INF: The $vmResourceGroup doesn't exist" -ForegroundColor red
}



Write-Host "`nDeleting App Registrations.." -ForegroundColor white -BackgroundColor black
$appRegistrationList = @($botAppName, $botServiceApiAppName, $botServiceClientAppName, $managementApiAppName, $sdkAppName)
foreach($appName in $appRegistrationList){
    $appRegs=az ad app list --display-name $appName | ConvertFrom-Json
    if($appRegs[0].displayName -eq $appName ){
        az ad app delete --id $appRegs[0].appId
        Write-Host "`n${appName} has been deleted" -ForegroundColor green
    }
}

Write-Host "Deleting DNS Record.." -ForegroundColor white -BackgroundColor black
Write-Host "Changing to dns subscription.."  -ForegroundColor green
az account set --subscription $dnsSubscriptionId

Write-Host "Deleting dns record.." -ForegroundColor green
az network dns record-set a delete --resource-group $dnsResourceGroup --zone-name $zoneName --name $dnsRecordName

Write-Host "Back to previous subscription.." -ForegroundColor green
az account set --subscription $currentSubscriptionId

Write-Host "Deletion process finished" -ForegroundColor green