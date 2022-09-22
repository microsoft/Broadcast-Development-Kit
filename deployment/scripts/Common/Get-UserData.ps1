function Get-UserData {
    $user = az account show | ConvertFrom-Json
    $userDetails = az ad user show --id $user.user.name | ConvertFrom-Json

    $result = '{"tenantId": "'+$user.homeTenantId+'","objectId": "'+$userDetails.id+'", "subscriptionId": "'+$user.id+'" }'
    Write-Output $result 
}