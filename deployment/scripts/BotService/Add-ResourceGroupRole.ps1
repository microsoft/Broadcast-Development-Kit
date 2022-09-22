function Add-ResourceGroupRole {
    param(
    $sdkAppClientId,
    $resourceGroup
    )

    $result = az ad sp show --id $sdkAppClientId | ConvertFrom-Json
    $objectId = $result.id

    az role assignment create --assignee $objectId --role "Contributor" --resource-group $resourceGroup
}
