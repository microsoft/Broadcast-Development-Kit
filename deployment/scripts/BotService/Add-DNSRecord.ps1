function Add-DNSRecord {
    param(
        $currentSubscriptionId,
        $dnsSubscriptionId,
        $zoneName,
        $dnsResourceGroup,
        $dnsRecordName,
        $vmAdress
    )

    Write-Host "Changing azure account to dns subscription"  -ForegroundColor green
    az account set --subscription $dnsSubscriptionId

    Write-Host "Adding dns record.." -ForegroundColor green
    az network dns record-set a add-record --resource-group $dnsResourceGroup --zone-name $zoneName --record-set-name $dnsRecordName --ipv4-address $vmAdress

    Write-Host "Changing azure account to previous subscription.." -ForegroundColor green
    az account set --subscription $currentSubscriptionId
}
