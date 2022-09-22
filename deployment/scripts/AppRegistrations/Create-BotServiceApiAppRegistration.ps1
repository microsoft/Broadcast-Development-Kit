function Create-BotService-Api-App-Reg{
    param(
        $appName,
        $serviceName
    )

    if (-not $appName) {
        $appName = "${serviceName}-botservice-api"
        Write-Host "INF: App Registration name was not provided, creating as: $appName" -ForegroundColor yellow
    }

    Write-Host "Creating app registration" -ForegroundColor green
    $botServiceAppId=$(az ad app create --display-name $appName --query appId --output tsv)

    #"Bot Service API appId: $BOT_SERVICE_API_APP_ID"
    $botServiceApiObjId = az ad app show --id $botServiceAppId --query id

    Write-Host "Setting accessTokenAcceptedVersion=2 in Bot Service API application" -ForegroundColor green
    az rest --method PATCH --uri "https://graph.microsoft.com/v1.0/applications/$botServiceApiObjId" --headers 'Content-Type=application/json' --body '{\"api\":{\"requestedAccessTokenVersion\": 2}}'

    Write-Host "Creating app role" -ForegroundColor green
    az ad app update --id $botServiceAppId --app-roles=@bot_service_api_role.json

    Write-Host "Exposing an API" -ForegroundColor green
    az ad app update --id $botServiceAppId --identifier-uris api://$botServiceAppId

    Write-Host "Creating Service Principal" -ForegroundColor green
    $sp = az ad sp create --id $botServiceAppId

    Write-Output $botServiceAppId
}
