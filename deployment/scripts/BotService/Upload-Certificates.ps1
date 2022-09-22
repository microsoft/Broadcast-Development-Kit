function Upload-Certificates {
    param(
    $vmName,
    $vmResourceGroup,
    $container,
    $storageConnectionString,
    $certsPath
    )

    $storageExist = az storage container exists --name $container --connection-string $storageConnectionString | ConvertFrom-Json
    if (-not $storageExist.exists){
        Write-Host "Container $container does not exists. Creating..." -ForegroundColor green
        az storage container create --name $container --connection-string $storageConnectionString
    }

    Write-Host "Uploading certs file '$certsPath' to container..." -ForegroundColor green
    az storage blob upload -f $certsPath -c $container -n "certs.zip" --connection-string $storageConnectionString
    Write-Host "Certs uploaded to container" -ForegroundColor green

    $CONFIG_SAS=az storage blob generate-sas --connection-string $storageConnectionString -c $container -n "certs.zip" --permissions r --expiry $((Get-Date).AddMinutes(10).ToUniversalTime() | Get-Date -UFormat '+%Y-%m-%dT%H:%MZ') --https-only --full-uri

    Write-Host "(VM) Downloading Certs.." -ForegroundColor green
    $CONFIG_SAS = $CONFIG_SAS.replace('"', '''')
    $DOWNLOAD_CONFIG_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri " + $CONFIG_SAS + " -OutFile C:\\certs.zip;Expand-Archive -Path c:\\certs.zip -DestinationPath C:\\certs -Force"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $DOWNLOAD_CONFIG_SCRIPT

}