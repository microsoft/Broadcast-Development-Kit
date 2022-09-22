function Deploy-ManagementApi {
    param(
    $publishOutputSrc,
    $managementResourceGroup,
    $apiAppName
    )

    Write-Host "Compressing Management API..." -ForegroundColor green
    $zipFile = $publishOutputSrc + "\\managementapi.zip"
    Compress-Archive -Path "${publishOutputSrc}\\*" -DestinationPath $zipFile

    Write-Host "Deploying Management API..." -ForegroundColor green
    az webapp deployment source config-zip `
        --src $zipFile `
        --resource-group $managementResourceGroup `
        --name $apiAppName
}
