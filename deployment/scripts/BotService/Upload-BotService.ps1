function Upload-BotService {
   param(
        $storageConnectionString,
        $publishOutputSrc,
        $containerName
    )

    Write-Host "Compressing BotService..." -ForegroundColor green
    $zipFile = $publishOutputSrc + "\\botService.zip"
    Compress-Archive -LiteralPath $publishOutputSrc -DestinationPath $zipFile

    $storageExist = az storage container exists --name $containerName --connection-string "$storageConnectionString" | ConvertFrom-Json
    if (-not $storageExist.exists){
        Write-Host "Container $artifacts does not exists. Creating..." -ForegroundColor green
        az storage container create --name $containerName --connection-string "$storageConnectionString"
    }

    Write-Host "Generating SAS Token..." -ForegroundColor green
    $BOT_SAS=az storage blob generate-sas --connection-string "$storageConnectionString" -c $containerName -n "botService.zip" --permissions rw --expiry $((Get-Date).AddMinutes(30).ToUniversalTime() | Get-Date -UFormat '+%Y-%m-%dT%H:%MZ') --https-only --full-uri
    
    Write-Host "Uploading artifact $zipFile to container $containerName..." -ForegroundColor green
    .\libs\azcopy.exe copy "${zipFile}" "${BOT_SAS}" --recursive=true
}
