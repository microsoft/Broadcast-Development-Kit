function Import-Certificate-To-Keyvault {
    param(
        $certPath,
        $keyvaultName,
        $pfxCertificatePassword,
        $pfxCertificateThumbprint
    )

    Write-Host "Importing pfx Certificate to keyvault.." -ForegroundColor green
    $cer = az keyvault certificate import --vault-name $keyvaultName --name "Certificate" --file $certPath --password $pfxCertificatePassword

    Write-Host "Adding Cert password keyvault secret.." -ForegroundColor green
    $pa = az keyvault secret set --vault-name $keyvaultName --name "Settings--BotConfiguration--CertificatePassword" --value $pfxCertificatePassword

    Write-Host "Adding Cert thumbprint keyvault secret.." -ForegroundColor green
    $th = az keyvault secret set --vault-name $keyvaultName --name "Settings--BotConfiguration--CertificateThumbprint" --value $pfxCertificateThumbprint
}