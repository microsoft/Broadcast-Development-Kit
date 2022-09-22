function Add-VMKeyvaultPolicy {
    param (
    $keyvaultName,
    $vmIdentity
    )

    $key = az keyvault set-policy --name $keyvaultName --object-id $vmIdentity --secret-permissions get list --certificate-permissions get list
}
