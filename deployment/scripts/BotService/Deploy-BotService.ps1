function Deploy-BotService {
    param(
    $storageConnectionString,
    $containerName,
    $vmName,
    $vmResourceGroup
    )

    Write-Host "Creating SAS token.." -ForegroundColor green
    $BOT_SAS=az storage blob generate-sas --connection-string "$storageConnectionString" -c $containerName -n "botService.zip" --permissions r --expiry $((Get-Date).AddMinutes(30).ToUniversalTime() | Get-Date -UFormat '+%Y-%m-%dT%H:%MZ') --https-only --full-uri

    Write-Host "(VM) Downloading BotService Artifact.." -ForegroundColor green
    $BOT_SAS = $BOT_SAS.replace('"', '''')
    $VM_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri " + $BOT_SAS + " -OutFile c:\\botService.zip;"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $VM_SCRIPT

    Write-Host "(VM) Removing previous deploy..." -ForegroundColor green
    $UNISTALL_BOT_SCRIPT = "If (Test-Path -Path C:\\botService) {Stop-Service -Name 'Bot-Service'; sc.exe delete 'Bot-Service'; Remove-Item -Recurse -Path C:\\botService}"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $UNISTALL_BOT_SCRIPT

    Write-Host "(VM) Extracting BotService..." -ForegroundColor green
    $UNZIP_BOT = "Expand-Archive -Path c:\\botService.zip -DestinationPath C:\\"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $UNZIP_BOT

    Write-Host "(VM) Installing BotService as a Windows Service..." -ForegroundColor green
    $INSTALL_BOT = "New-Service -Name 'Bot-Service' -BinaryPathName c:\\botService\botService.exe -StartupType Automatic;New-NetFirewallRule -DisplayName 'BotService' -Direction Inbound -Program 'C:\botService\BotService.exe' -Action Allow"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $INSTALL_BOT
}
