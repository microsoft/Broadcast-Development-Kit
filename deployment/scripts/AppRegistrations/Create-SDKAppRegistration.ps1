function Create-SDKAppRegistration {
    param(
        $appName,
        $serviceName
    )

    if (-not $appName) {
        $appName = "${serviceName}-sdk-app"
        Write-Host "INF: App Registration name was not provided, creating as: $appName" -ForegroundColor yellow
    }

    Write-Host "Creating App Registration.." -ForegroundColor green
    $sdkAppId = $(az ad app create --display-name $appName --query appId --output tsv)

    Write-Host "Creating secret.." -ForegroundColor green
    $clientSecret = az ad app credential reset --id $sdkAppId | ConvertFrom-Json

    Write-Host "Creating Service principal.." -ForegroundColor green
    $sp = az ad sp create --id $sdkAppId

    $result = '{"appId":"'+$sdkAppId+'", "clientSecret": "'+$clientSecret.password+'"}'
    Write-Output $result
} 