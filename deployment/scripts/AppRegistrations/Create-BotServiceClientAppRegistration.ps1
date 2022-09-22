function Create-BotServiceClientAppRegistration {
    param(
        $botServiceAppId,
        $appName,
        $serviceName
    )

    if (-not $appName) {
        $appName = "${serviceName}-botservice-client"
        Write-Host "INF: App Registration name was not provided, creating as: $appName" -ForegroundColor yellow
    }

    Write-Host  "Creating Bot Service Client app regitration.." -ForegroundColor green
    $botServiceClientAppId = az ad app create --display-name $appName --query appId --output tsv

    Write-Host "Obtaining Role.." -ForegroundColor green
    $botServiceRoleId = $(az ad app show --id $botServiceAppId --query "appRoles[].id" --output tsv)

    Write-Host "Updating API permissions.." -ForegroundColor green
    $requiredResourceAccess = '[{"resourceAppId":"00000003-0000-0000-c000-000000000000","resourceAccess":[{"id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d","type": "Scope"}]},{"resourceAppId":"'+$botServiceAppId+'","resourceAccess":[{"id":"'+$botServiceRoleId+'","type":"Role"}]}]' | ConvertTo-Json 
    az ad app update --id $botServiceClientAppId --required-resource-accesses $requiredResourceAccess

    Write-Host "Creating secret.." -ForegroundColor green
    $botServiceSecret = $(az ad app credential reset --id $botServiceClientAppId) | ConvertFrom-Json

    Write-Host "Creating Service Principal" -ForegroundColor green
    $sp = az ad sp create --id $botServiceClientAppId

    $result = '{"appId":"'+$botServiceClientAppId+'", "clientSecret": "'+$botServiceSecret.password+'"}'
    Write-Output $result
}