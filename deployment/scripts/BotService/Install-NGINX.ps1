function Install-NGINX {
    param(
    $vmName,
    $vmResourceGroup,
    $container,
    $storageConnectionString,
    $nginxUri,
    $nginxConfigFile,
    $nssmUri
    )

    Write-Host "(VM) Downloading and Installing NGINX Zip from GitHub.." -ForegroundColor green
    $VM_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri " + $nginxUri + " -OutFile c:\\nginx.zip; Expand-Archive -Path c:\\nginx.zip -DestinationPath C:\\nginx -Force"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $VM_SCRIPT
    
    $storageExist = az storage container exists --name $container --connection-string $storageConnectionString | ConvertFrom-Json
    if (-not $storageExist.exists){
        Write-Host "Container $container does not exists. Creating..."
        az storage container create --name $container --connection-string $storageConnectionString
    }

    Write-Host "Uploading NGINX configuration file '$nginxConfigFile' to container..." -ForegroundColor green
    az storage blob upload -f $nginxConfigFile -c $container -n "nginx.conf" --connection-string $storageConnectionString
    Write-Host "Configuration uploaded" -ForegroundColor green

    Write-Host "(VM) Downloading NGINX Configuration.." -ForegroundColor green
    $CONFIG_SAS=az storage blob generate-sas --connection-string $storageConnectionString -c $container -n "nginx.conf" --permissions r --expiry $((Get-Date).AddMinutes(10).ToUniversalTime() | Get-Date -UFormat '+%Y-%m-%dT%H:%MZ') --https-only --full-uri
    $CONFIG_SAS = $CONFIG_SAS.replace('"', '''')
    $DOWNLOAD_CONFIG_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri " + $CONFIG_SAS + " -OutFile C:\\nginx\\nginx-rtmp-win32-dev\\conf\\nginx.conf; New-NetFirewallRule -DisplayName 'NGINX' -Direction Inbound -Program 'c:\nginx\nginx-rtmp-win32-dev\nginx.exe' -Action Allow"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $DOWNLOAD_CONFIG_SCRIPT

    Write-Host "(VM) Downloading and extracting NSSM installer.." -ForegroundColor green
    $DOWNLOAD_NSSM_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri $nssmUri -OutFile C:\nssm.zip; Expand-Archive -Path c:\\nssm.zip -DestinationPath C:\\nssm -Force"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $DOWNLOAD_NSSM_SCRIPT

    Write-Host "(VM) Adding NGINX as a Service..." -ForegroundColor green
    $CREATE_NGINX_SERVICE_SCRIPT = "c:\nssm\nssm-2.24\win64\nssm.exe install NGINX c:\nginx\nginx-rtmp-win32-dev\nginx.exe;c:\nssm\nssm-2.24\win64\nssm.exe start NGINX"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $CREATE_NGINX_SERVICE_SCRIPT
}

