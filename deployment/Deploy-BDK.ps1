param(
    $config = "config.json"
)

$error.clear()

if(-not (Test-Path -Path $config)){
    Write-Host "'${config}' configuration file is required" -ForegroundColor red
    Exit
}

$configuration = Get-Content $config | ConvertFrom-Json

Write-Host "Config" $configuration

$managementApiSrc = "..\\src\\ManagementApi"
$managementApiPublishOutput = "bin\\output"
$managementApiCompressOutput = "..\\src\\ManagementApi\\bin\\output"
$functionAppSrc = "..\\src\\OrchestratorFunction"
$functionAppPublishOutput = "bin\\output"
$functionAppCompressOutput = "..\\src\\OrchestratorFunction\\bin\\output"
$botserviceSrc = "..\\src\\BotService"
$botservicePublishOutput = "bin\\botService"
$botserviceCompressOutput = "..\\src\\BotService\\bin\\botService"
$artifactContainerName = "artifacts"
$nginxConfigFile = ".\\scripts\\botService\\nginx.conf"

$serviceName = $configuration.name
if (-not $serviceName){
    Write-Host "Error: A name should be provided in the configuration settings." -ForegroundColor red
    Exit
}

$managementResourceGroup = $configuration.managementResourceGroup
if ( -not $managementResourceGroup){
    $managementResourceGroup = "${serviceName}-rg"
    Write-Host "INF: A name for the management resource group was not provided, it will be: $managementResourceGroup" -ForegroundColor yellow
}

$vmResourceGroup = $configuration.vmResourceGroup
if ( -not $vmResourceGroup){
    $vmResourceGroup = "${serviceName}vm-rg"
    Write-Host "INF: A name for the VM resource group was not provided, it will be: $vmResourceGroup" -ForegroundColor yellow
}

. ".\scripts\Common\Get-UserData.ps1"

$userData = Get-UserData | ConvertFrom-Json
$userTenantId = $userData.tenantId
$userObjectId = $userData.objectId
$userCurrentSubscriptionId = $userData.subscriptionId

. ".\scripts\Common\Stop-OnPowershellError.ps1"
. ".\scripts\Common\Build-Project.ps1"

. ".\scripts\AppRegistrations\Create-BotServiceApiAppRegistration.ps1"
. ".\scripts\AppRegistrations\Create-BotServiceClientAppRegistration.ps1"
. ".\scripts\AppRegistrations\Create-ManagementApiAppRegistration.ps1"
. ".\scripts\AppRegistrations\Create-SDKAppRegistration.ps1"
. ".\scripts\AppRegistrations\Create-BotAppRegistration.ps1"

. ".\scripts\BotService\Add-ResourceGroupRole.ps1"
. ".\scripts\BotService\Add-VMKeyvaultPolicy.ps1"
. ".\scripts\BotService\Add-DNSRecord.ps1"
. ".\scripts\Management\Deploy-ManagementApi.ps1"
. ".\scripts\Management\Deploy-FunctionApp.ps1"
. ".\scripts\BotService\Upload-Certificates.ps1"
. ".\scripts\BotService\Import-PfxToKeyvault.ps1"
. ".\scripts\BotService\Install-VCRedist.ps1"
. ".\scripts\BotService\Install-NGINX.ps1"
. ".\scripts\BotService\Add-ServiceCosmosDBDocument.ps1"
. ".\scripts\BotService\Update-AppSettings.ps1"
. ".\scripts\BotService\Upload-BotService.ps1"
. ".\scripts\BotService\Deploy-BotService.ps1"

# CREATE APP REGISTRATIONS

Write-Host "Starting the creation of App Registrations" -ForegroundColor white -BackgroundColor black

Write-Host "`n1/28 App registration: Creating Bot service API.." -ForegroundColor white -BackgroundColor black
$botServiceAppId = Create-BotService-Api-App-reg `
    -appName $configuration.appRegistrations.botServiceApiAppName `
    -serviceName $serviceName

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n2/28 App registration: Creating Bot Service Client.." -ForegroundColor white -BackgroundColor black
$botServiceClientResult = Create-BotServiceClientAppRegistration `
    -botServiceAppId $botServiceAppId `
    -appName $configuration.appRegistrations.botServiceClientAppName `
    -serviceName $serviceName | ConvertFrom-Json

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n3/28 App registration: Creating Management API.." -ForegroundColor white -BackgroundColor black
$managementApiAppResult = Create-ManagementApiAppRegistration `
    -appName $configuration.appRegistrations.managementApiAppName `
    -serviceName $serviceName

Write-Host "Completed`n" -ForegroundColor green

Write-Host  "`n4/28 App registration: Creating SDK App Registration.." -ForegroundColor white -BackgroundColor black
$sdkAppResult = Create-SDKAppRegistration `
    -appName $configuration.appRegistrations.sdkAppName `
    -serviceName $serviceName | ConvertFrom-Json

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n5/28 App registration: Creating Bot App Registration.." -ForegroundColor white -BackgroundColor black
$botappResult = Create-BotAppRegistration `
    -appName $configuration.appRegistrations.botAppName `
    -serviceName $serviceName | ConvertFrom-Json

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

#Client Id of the Azure SDK Service app registration app registration.
$azServicePrincipalClientId = $sdkAppResult.appId
#Subscription Id of the Azure SDK app registration app registration.
#Tenant Id of Azure SDK app registration app registration.
$azServicePrincipalTenantId = $userTenantId
#Id of the Management API app registration created in Azure AD.
$authApiClientId = $managementApiAppResult
#Client Id of the Bot Service API app registration.
$authBotServiceApiClientId = $botServiceAppId
#TenantId of the Bot Service API app registration.
$authBotServiceApiTenantId = $userTenantId
#Client Id of the Bot Service Client app registration.
$authBotServiceClientClientId = $botServiceClientResult.appId
#Client Id of the Azure Bot app registration.
$graphClientClientId = $botappResult.appId
#Tenant Id of the Azure Bot app registration.
$graphClientTenantId = $userTenantId

#Client secret of the Azure SDK Service app registration. 
#SAVE IN KV AS: sbx2-Settings--AzServicePrincipalConfiguration--ApplicationClientSecret
$azServicePrincipalClientSecret = $sdkAppResult.clientSecret
# BotService client client secret
# sbx2-Settings--BotServiceAuthenticationConfiguration--ClientSecret
$authBotServiceClientClientSecret = $botServiceClientResult.clientSecret
#client secret of the Azure Bot app registration.
# sbx2-Settings--GraphClientConfiguration--ClientSecret
# sbx2-Settings--BotConfiguration--AadAppSecret
$graphClientClientSecret = $botappResult.clientSecret

## DEPLOY MANAGEMENT
Write-Host "`n6/28 Creating Management Resource Group:" $managementResourceGroup -ForegroundColor white -BackgroundColor black
az group create -l $configuration.managementResourceGroupLocation -n $managementResourceGroup

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n7/28 Deploying Management Resources.." -ForegroundColor white -BackgroundColor black
$managementResourcesCreated = az deployment group create --resource-group $managementResourceGroup --template-file "Biceps/managementApi.bicep" `
    --parameters serviceName=$serviceName `
    userTenantId=$userTenantId `
    userObjectId=$userObjectId `
    azServicePrincipalClientId=$azServicePrincipalClientId `
    azServicePrincipalTenantId=$azServicePrincipalTenantId `
    authApiClientId=$authApiClientId `
    authBotServiceApiClientId=$authBotServiceApiClientId `
    authBotServiceClientClientId=$authBotServiceClientClientId `
    graphClientClientId=$graphClientClientId `
    graphClientTenantId=$graphClientTenantId `
    azServicePrincipalClientSecret=$azServicePrincipalClientSecret `
    authBotServiceClientClientSecret=$authBotServiceClientClientSecret `
    graphClientClientSecret=$graphClientClientSecret `
    azServicePrincipalSubscriptionId=$userCurrentSubscriptionId | ConvertFrom-Json

if(-not $managementResourcesCreated){
    Write-Host "The creation of the resoures failed" -ForegroundColor red
    Exit
}

Write-Host "Completed`n" -ForegroundColor green

# MANAGEMENT OUTPUT VARIABLES
$keyvaultName = $managementResourcesCreated.properties.outputs.keyvaultName.value
$storageConnectionString = $managementResourcesCreated.properties.outputs.storageConnectionString.value
$appinsightInstrumentationKey = $managementResourcesCreated.properties.outputs.appinsightInstrumentationKey.value
$managementApiUri = $managementResourcesCreated.properties.outputs.managementApiUri.value
$functionAppName = $managementResourcesCreated.properties.outputs.functionAppName.value
$cosmosDatabase = $managementResourcesCreated.properties.outputs.cosmosDatabase.value
$cosmosEndpoint = $managementResourcesCreated.properties.outputs.cosmosEndpoint.value
$cosmosConnectionString = $managementResourcesCreated.properties.outputs.cosmosConnectionString.value
$apiAppName = $managementResourcesCreated.properties.outputs.apiAppName.value

Write-Host "`n8/28 Building Management API project.." -ForegroundColor white -BackgroundColor black
Build-Project `
    -projectSrc $managementApiSrc `
    -publishOutputSrc $managementApiPublishOutput 

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n9/28 Compress and deploy Management API project.." -ForegroundColor white -BackgroundColor black
Deploy-ManagementApi `
    -publishOutputSrc $managementApiCompressOutput `
    -managementResourceGroup $managementResourceGroup `
    -apiAppName $apiAppName

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n10/28 Building Function app project.." -ForegroundColor white -BackgroundColor black
Build-Project `
    -projectSrc $functionAppSrc `
    -publishOutputSrc $functionAppPublishOutput

Write-Host "Completed`n" -ForegroundColor green 
Stop-OnPowershellError

Write-Host "`n11/28 Compress and deploy Function app project.." -ForegroundColor white -BackgroundColor black
Deploy-FunctionApp `
    -publishOutputSrc $functionAppCompressOutput `
    -managementResourceGroup $managementResourceGroup `
    -functionAppName $functionAppName

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

# DEPLOY VM
Write-Host "`n12/28 Creating VM Resource group: " $vmResourceGroup -ForegroundColor white -BackgroundColor black
az group create -l $configuration.vmResourceGroupLocation -n $vmResourceGroup

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n13/28 Adding SDK role to VM resource group..." -ForegroundColor white -BackgroundColor black
Add-ResourceGroupRole `
    -sdkAppClientId $azServicePrincipalClientId `
    -resourceGroup $vmResourceGroup

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n14/28 Creating Azure bot.." -ForegroundColor white -BackgroundColor black
$botDisplayName = $configuration.botService.botDisplayName
$botHandle = $configuration.botService.botHandle
$azureBotCreated = az deployment group create --resource-group $vmResourceGroup --template-file "Biceps/azBot.bicep" `
    --parameters botDisplayName=$botDisplayName `
    botHandle=$botHandle `
    botAppRegitrationId=$graphClientClientId

if(-not $azureBotCreated){
    Write-Host "The creation of the resoures failed" -ForegroundColor red
    Exit
}

Write-Host "Completed`n" -ForegroundColor green

$vmAdminUser = $configuration.botService.vmUserName
$vmAdminPassword = $configuration.botService.vmPassword
$gstreamerInstallerUri = $configuration.botService.gstreamerInstallerUri
$gStreamerInstallationPath = $configuration.botService.gStreamerInstallationPath

Write-Host "`n15/28 Deploying VM Resources.." -ForegroundColor white -BackgroundColor black
$vmResourcesCreated = az deployment group create --resource-group $vmResourceGroup --template-file "Biceps/virtualMachine.bicep" `
    --parameters serviceName=$serviceName `
    vmAdminUsername=$vmAdminUser `
    sdkAppId=$azServicePrincipalClientId `
    sdkClientSecret=$azServicePrincipalClientSecret `
    gstreamerInstallerUri=$gstreamerInstallerUri `
    gStreamerInstallationPath=$gStreamerInstallationPath `
    vmAdminPassword=$vmAdminPassword | ConvertFrom-Json

if(-not $vmResourcesCreated){
    Write-Host "The creation of the resoures failed" -ForegroundColor red
    Exit
}

Write-Host "Completed`n" -ForegroundColor green

#VM OUTPUT VARIABLES
$vmName = $vmResourcesCreated.properties.outputs.vmName.value
$vmIpAdress = $vmResourcesCreated.properties.outputs.vmIpAdress.value
$vmIdentity =  $vmResourcesCreated.properties.outputs.vmIdentity.value

Write-Host "`n16/28 Adding VM to keyvault Access policy.." -ForegroundColor white -BackgroundColor black
Add-VMKeyvaultPolicy `
    -keyvaultName $keyvaultName `
    -vmIdentity $vmIdentity

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n17/28 Creating DNS record.." -ForegroundColor white -BackgroundColor black
Add-DNSRecord `
    -currentSubscriptionId $userCurrentSubscriptionId `
    -dnsSubscriptionId $configuration.dnsZone.dnsSubscriptionId `
    -zoneName $configuration.dnsZone.zoneName `
    -dnsResourceGroup $configuration.dnsZone.resourceGroup `
    -dnsRecordName $configuration.dnsZone.dnsRecordName `
    -vmAdress $vmIpAdress

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n18/28 Creating Eventgrid..." -ForegroundColor white -BackgroundColor black
$eventGridResourcesCreated = az deployment group create --resource-group $vmResourceGroup --template-file "Biceps/eventgrid.bicep"  `
    --parameters serviceName=$serviceName `
    functionsResourceGroup=$managementResourceGroup `
    functionsResourceName=$functionAppName

if(-not $eventGridResourcesCreated){
    Write-Host "The creation of the resoures failed" -ForegroundColor red
    Exit
}

Write-Host "Completed`n" -ForegroundColor green

Write-Host "`n19/28 VM Uploading certs.." -ForegroundColor white -BackgroundColor black
Upload-Certificates `
    -vmName $vmName `
    -vmResourceGroup $vmResourceGroup `
    -container $artifactContainerName `
    -certsPath $configuration.botService.localCertPath `
    -storageConnectionString "'$storageConnectionString'"

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n20/28 Importing Keyvault cert.." -ForegroundColor white -BackgroundColor black
Import-Certificate-To-Keyvault `
    -keyvaultName $keyvaultName `
    -certPath $configuration.botService.pfxCertificatePath `
    -pfxCertificatePassword $configuration.botService.pfxCertificatePassword `
    -pfxCertificateThumbprint $configuration.botService.pfxCertificateThumbprint 

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n21/28 VM Installing VCRedist.." -ForegroundColor white -BackgroundColor black
Install-VCRedist `
    -vmName $vmName `
    -vmResourceGroup $vmResourceGroup `
    -vcRedistUri $configuration.botService.vcRedistInstallerUri

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n22/28 VM Installing NGINX.." -ForegroundColor white -BackgroundColor black
Install-NGINX `
    -vmName $vmName `
    -vmResourceGroup $vmResourceGroup `
    -container $artifactContainerName `
    -nginxUri $configuration.botService.nginxInstallerUri `
    -nginxConfigFile $nginxConfigFile `
    -nssmUri $configuration.botService.nssmInstallerUri `
    -storageConnectionString "'$storageConnectionString'"

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n23/28 Update VM service document in cosmos DB.." -ForegroundColor white -BackgroundColor black
Add-ServiceCosmosDBDocument `
    -dnsRecordName $configuration.dnsZone.dnsRecordName `
    -dnsZoneName $configuration.dnsZone.zoneName `
    -vmName $vmName `
    -subscriptionId $userCurrentSubscriptionId `
    -vmResourceGroup $vmResourceGroup `
    -connectionString "$cosmosConnectionString" `
    -database $cosmosDatabase

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n24/28 Building BotService project.." -ForegroundColor white -BackgroundColor black
Build-Project `
    -projectSrc $botserviceSrc `
    -publishOutputSrc $botservicePublishOutput 

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n25/28 Updating BotService appSettings.." -ForegroundColor white -BackgroundColor black
Update-AppSettings `
    -publishOutputSrc $botserviceCompressOutput `
    -appinsightsinstrumentatioKey $appinsightInstrumentationKey `
    -keyvaultName $keyvaultName `
    -dnsRecordName $configuration.dnsZone.dnsRecordName `
    -dnsZoneName $configuration.dnsZone.zoneName `
    -botServiceApiClientId $authBotServiceApiClientId `
    -botServiceAppRegistrationTenantId $authBotServiceApiTenantId `
    -vmName $vmName `
    -managementUri $managementApiUri `
    -azureBotClientId $graphClientClientId `
    -azureBotTenantId $graphClientTenantId `
    -cosmosEndpoint $cosmosEndpoint `
    -cosmosDatabase $cosmosDatabase

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n26/28 Compress and Upload BotService project.." -ForegroundColor white -BackgroundColor black
Upload-BotService `
    -publishOutputSrc $botserviceCompressOutput `
    -containerName $artifactContainerName `
    -storageConnectionString "$storageConnectionString" 

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n27/28 VM Deploy BotService project.." -ForegroundColor white -BackgroundColor black
Deploy-BotService `
    -vmName $vmName `
    -vmResourceGroup $vmResourceGroup `
    -containerName $artifactContainerName `
    -storageConnectionString "$storageConnectionString"

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`n28/28 Restarting VM.." -ForegroundColor white -BackgroundColor black
    az vm restart --name $vmName --resource-group $vmResourceGroup

Write-Host "Completed`n" -ForegroundColor green
Stop-OnPowershellError

Write-Host "`nDeploy Finished.." -ForegroundColor white -BackgroundColor black

Write-Host "`nBot Consent URL" -ForegroundColor white -BackgroundColor black
Write-Host "Bot Service Client App Registration Consent URL" -ForegroundColor green
Write-Host "https://login.microsoftonline.com/common/adminconsent?client_id=${authBotServiceClientClientId}&state=1" -ForegroundColor green

Write-Host "Azure Bot App Registration Consent URL" -ForegroundColor green
Write-Host "https://login.microsoftonline.com/common/adminconsent?client_id=${graphClientClientId}&state=1" -ForegroundColor green