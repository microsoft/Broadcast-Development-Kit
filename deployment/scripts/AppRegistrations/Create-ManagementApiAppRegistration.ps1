function Create-ManagementApiAppRegistration {
    param(
        $appName,
        $serviceName
    )

    if (-not $appName) {
        $appName = "${serviceName}-management-api"
        Write-Host "INF: App Registration name was not provided, creating as: $appName" -ForegroundColor yellow
    }

    Write-Host "Creating app regitration.." -ForegroundColor green
    $managementApiAppId = $(az ad app create --display-name $appName --sign-in-audience AzureADMultipleOrgs --query appId --output tsv)
    $managementApiObjId = az ad app show --id $managementApiAppId --query id

    Write-Host "Setting accessTokenAcceptedVersion = 2.." -ForegroundColor green
    az rest --method patch --uri "https://graph.microsoft.com/v1.0/applications/$managementApiObjId" --headers 'Content-Type=application/json' --body '{\"api\":{\"requestedAccessTokenVersion\": 2}}'

    Write-Host "Setting SecurityGroup.." -ForegroundColor green
    az ad app update --id $managementApiAppId --set groupMembershipClaims=SecurityGroup

    $optionalClaims='{"idToken":[{"name":"groups","source": null,"essential": false,"additionalProperties":["emit_as_roles"]}],"accessToken": [],"saml2Token": []}' | ConvertTo-Json 
    az ad app update --id $managementApiAppId --optional-claims $optionalClaims

    Write-Host "Setting permissions.." -ForegroundColor green
    $requiredResourceAccess='[{"resourceAppId":"00000003-0000-0000-c000-000000000000","resourceAccess":[{"id":"7427e0e9-2fba-42fe-b0c0-848c9e6a8182","type": "Scope"},{"id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d","type": "Scope"}]}]' | ConvertTo-Json
    az ad app update --id $managementApiAppId --required-resource-accesses $requiredResourceAccess

    Write-Host "Creating api role.." -ForegroundColor green
    az ad app update --id $managementApiAppId --app-roles=@management_api_role.json

    Write-Host "Exposing an API.." -ForegroundColor green
    az ad app update --id $managementApiAppId --identifier-uris	api://$managementApiAppId
    
    Write-Host "Adding API Read Scope.." -ForegroundColor green
    $scopeJSONHash = @{
        adminConsentDescription="Access Broadcaster for Teams as producer"
        adminConsentDisplayName="Access Broadcaster for Teams as producer"
        id="2f7ec34d-20a2-4900-836b-4cf8399ed52e"
        isEnabled=$true
        type="User"
        userConsentDescription="Access Broadcaster for Teams as user"
        userConsentDisplayName="Access Broadcaster for Teams as user"
        value="access_as_producer"
    }

    $azAppOID = (az ad app show --id $managementApiAppId | ConvertFrom-JSON).id
    $accesstoken = (az account get-access-token --resource-type ms-graph --query accessToken --output tsv)
    $header = @{
        'Content-Type' = 'application/json'
        'Authorization' = 'Bearer ' + $accesstoken
    }

    $bodyAPIAccess = @{
        'api' = @{
            'oauth2PermissionScopes' = @($scopeJSONHash)
        }
    }|ConvertTo-Json -d 3

    Invoke-RestMethod -Method patch -Uri "https://graph.microsoft.com/v1.0/applications/$azAppOID" -Headers $header -Body $bodyAPIAccess 

    Write-Host "Creating Service Principal" -ForegroundColor green
    $sp = az ad sp create --id $managementApiAppId

    Write-Output $managementApiAppId
}