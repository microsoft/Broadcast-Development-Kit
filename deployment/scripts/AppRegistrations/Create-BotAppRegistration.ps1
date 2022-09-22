function Create-BotAppRegistration {
    param(
        $appName,
        $serviceName
    )

    if (-not $appName) {
        $appName = "${serviceName}-bot-app"
        Write-Host "INF: App Registration name was not provided, creating as: $appName" -ForegroundColor yellow
    }

    Write-Host "Creating app registration.." -ForegroundColor green
    $botAppId = $(az ad app create --display-name $appName --sign-in-audience AzureADMultipleOrgs --query appId --output tsv)

    Write-Host "Setting required permissions.." -ForegroundColor green
    az ad app update --id $botAppId --required-resource-accesses=@botpermissions.json

    Write-Host "Creating secret.." -ForegroundColor green
    $botCredentials = $(az ad app credential reset --id $botAppId --append) | ConvertFrom-Json

    Write-Host "Creating Service Principal.." -ForegroundColor green
    $sp = az ad sp create --id $botAppId

    $result = '{"appId":"'+$botAppId+'", "clientSecret": "'+$botCredentials.password+'"}'
    Write-Output $result
}