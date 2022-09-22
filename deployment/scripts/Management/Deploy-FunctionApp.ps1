function Deploy-FunctionApp {
    param(
    $publishOutputSrc,
    $managementResourceGroup,
    $functionAppName
    )

    Write-Host "Compressing Function app.." -ForegroundColor green
    $zipFile = $publishOutputSrc + "\\functionapp.zip"
    Compress-Archive -Path "${publishOutputSrc}\\*" -DestinationPath $zipFile

    Write-Host "Deploying Function app.."  -ForegroundColor green
    az functionapp deployment source config-zip `
        --src $zipFile `
        --resource-group $managementResourceGroup `
        --name $functionAppName
    }