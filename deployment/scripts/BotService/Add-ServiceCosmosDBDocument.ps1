function Add-ServiceCosmosDBDocument {
    param (
    $dnsRecordName,
    $dnsZoneName,
    $vmName,
    $subscriptionId,
    $vmResourceGroup,
    $connectionString,
    $database
    )

    $botDns = "${dnsRecordName}.${dnsZoneName}" 

    cd cosmosdb
    npm i
    npm i ts-node -g
    ts-node ./sync_static_items.ts --connectionString $connectionString --db $database --dns $botDns --vmName $vmName --subscriptionId $subscriptionId --vmResourceGroup $vmResourceGroup
    cd..
}